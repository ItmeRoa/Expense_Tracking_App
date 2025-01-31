using expense_tracker.Dto.Payload;
using expense_tracker.Dto.Request.UserRequest;
using expense_tracker.Dto.Response;
using expense_tracker.Email;
using expense_tracker.Email.Msg;
using expense_tracker.Exception;
using expense_tracker.Model;
using expense_tracker.Repo.IRepo;
using expense_tracker.Service.IService;
using expense_tracker.Util;
using FluentEmail.Core;
using ILogger = Serilog.ILogger;

namespace expense_tracker.Service;

public class UserService : IUserService
{
    private readonly IUserRepo _userRepo;
    private readonly ILogger _logger;
    private readonly PasswordHasher _passwordHasher;
    private readonly RedisCaching _cache;
    private readonly OtpGenerator _otpGenerator;
    private readonly RazorPageRenderer _razorPageRenderer;
    private readonly IFluentEmail _fluentEmail;
    private readonly JwtGenerator _jwtGenerator;
    private const string RedisKeyOtp = "_session.opt.";
    private const string RedisEmailCreationPayloadKey = "_session.email.";
    private const string RedisVerifiedEmailCreationPayloadKey = "_session.email.verified.";
    private const string RedisRefreshTokenKey = "_user.";

    public UserService(IUserRepo userRepo, ILogger logger, RedisCaching cache, PasswordHasher passwordHasher,
        OtpGenerator otpGenerator, RazorPageRenderer razorPageRenderer, IFluentEmail fluentEmail,
        JwtGenerator jwtGenerator)
    {
        _userRepo = userRepo;
        _logger = logger;
        _cache = cache;
        _passwordHasher = passwordHasher;
        _otpGenerator = otpGenerator;
        _razorPageRenderer = razorPageRenderer;
        _fluentEmail = fluentEmail;
        _jwtGenerator = jwtGenerator;
    }

    private async Task SendEmailAsync(EmailMetadata emailMetadata, EmailVerificationMsg model)
    {
        var emailBody = await _razorPageRenderer.RenderTemplateAsync(emailMetadata.TemplatePath, model);
        var email = await _fluentEmail.To(emailMetadata.ToAddress)
            .Subject(emailMetadata.Subject)
            .Body(emailBody, isHtml: true)
            .SendAsync();

        if (email.Successful)
        {
            _logger.Information("Email sent successfully!");
        }
        else
        {
            _logger.Error("Failed to send email.");
            foreach (var error in email.ErrorMessages)
            {
                _logger.Error(error);
            }
        }
    }

    public async Task<UserCheckedByIdResponse> GetUserByIdAsync(int id)
    {
        User user = await _userRepo.GetUserByIdAsync(id);
        Subscription subscription = user.Subscriptions.First(s => s.SubscriptionStatus == "active");
        DateTime? endDate = subscription.EndDate;
        string remain = "Permanent";

        if (endDate.HasValue)
        {
            double remainingDays = (endDate.Value.ToUniversalTime() - DateTime.UtcNow).TotalDays;
            if (remainingDays < 0)
            {
                remain = "Expired";
            }
            else
            {
                remain = $"{remainingDays:F} days";
            }
        }


        return new UserCheckedByIdResponse
        {
            Message = $"Retrieve user by id {user.UserId}",
            UserInfo = new UserCheckedByIdModel
            {
                UserId = user.UserId,
                Email = user.Email,
                CreateAt = user.CreatedAt,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                IsEmailVerified = user.IsEmailVerified,
                UserSubscriptionPlan = subscription.Plan.PlanName,
                RemainingSubscription = remain
            }
        };
    }

    public async Task<SignupUapResponse> SignupByUaPAsync(SignUpByUapRequest request)
    {
        try
        {
            if (await _userRepo.UserDoesExistByEmailAsync(request.Email))
                throw new EntityAlreadyExistException($"The user with the email {request.Email} already exist");
            if (request.ConfirmPassword != request.Password) throw new ArgumentException("The password does not match");

            uint otp = _otpGenerator.GenerateSecureOtp();
            Guid session = Guid.NewGuid();


            await _cache.SetAsync($"{RedisKeyOtp}{session}", otp, TimeSpan.FromMinutes(10));
            await _cache.SetAsync($"{RedisEmailCreationPayloadKey}{session}", new RedisEmailUapPayload
            {
                Email = request.Email,
                Password = request.Password,
                IsEmailVerified = false
            }, TimeSpan.FromMinutes(15));

            EmailMetadata emailMetadata = new EmailMetadata
            {
                Subject = "Email Verification",
                ToAddress = request.Email,
                TemplatePath = "EmailVerification"
            };

            EmailVerificationMsg emailMsg = new EmailVerificationMsg
            {
                VerificationCode = otp,
                VerificationExpTime = 15
            };

            await SendEmailAsync(emailMetadata, emailMsg);

            return new SignupUapResponse
            {
                Message = "Signup success",
                Session = session,
                SignUpSessionExpAt = DateTime.UtcNow.AddMinutes(15).ToString("O")
            };
        }
        catch (EntityAlreadyExistException)
        {
            _logger.Warning("Please retry with a different email.");
            throw;
        }
    }

    public async Task<SignUpEmailVerificationResponse> EmailVerificationAsync(Guid session,
        VerifyEmailRequest request)
    {
        uint otp = await _cache.GetAsync<uint>($"{RedisKeyOtp}{session}");
        if (otp == 0) throw new CachedTokenException("The Otp has expired or does not exist");
        if (otp != request.OtpCode) throw new ArgumentException("The Otp does not match");

        RedisEmailUapPayload payload =
            await _cache.GetAsync<RedisEmailUapPayload>($"{RedisEmailCreationPayloadKey}{session}") ??
            throw new CachedTokenException(
                "The user creation payload has expired or does not exist");

        payload.IsEmailVerified = true;
        Guid newSession = Guid.NewGuid();
        await _cache.SetAsync($"{RedisVerifiedEmailCreationPayloadKey}{newSession}", payload, TimeSpan.FromMinutes(10));

        await _cache.RemoveAsync($"{RedisKeyOtp}{session}");
        await _cache.RemoveAsync($"{RedisEmailCreationPayloadKey}{session}");
        return new SignUpEmailVerificationResponse
        {
            Message = "Email successfully verified.",
            Session = newSession,
            ExpiredAt = DateTime.UtcNow.AddMinutes(10).ToString("O")
        };
    }

    public async Task<UserCreatedResponse> CreateUserAsync(Guid session, NameReuqest request)
    {
        RedisEmailUapPayload payload =
            await _cache.GetAsync<RedisEmailUapPayload>($"{RedisVerifiedEmailCreationPayloadKey}{session}") ??
            throw new CachedTokenException("The user creation payload has expired or does not exist");

        if (!payload.IsEmailVerified) throw new UnauthorizedAccessException("Email not verified");

        string middleName = request.MiddleName ?? "";
        string displayName = $"{request.FirstName} {middleName} {request.LastName}";


        User user = new User
        {
            FirstName = request.FirstName,
            MiddleName = middleName,
            LastName = request.LastName,
            Email = payload.Email,
            IsEmailVerified = payload.IsEmailVerified,
            DisplayName = displayName,
        };

        await _userRepo.CreateUserAsync(user, new UserAuth
        {
            PasswordHashed = _passwordHasher.HashPassword(payload.Password)
        });

        User createdUser = await _userRepo.GetUserByEmailAsync(user.Email);
        Subscription userSubscription = createdUser.Subscriptions.First(s => s.SubscriptionStatus == "active");
        string userSubscriptionPlan = userSubscription.Plan.PlanName;

        string accessToken =
            _jwtGenerator.GenerateAccessToken(createdUser.UserId.ToString(), createdUser.Email, userSubscriptionPlan);
        string refreshToken = _jwtGenerator.GenerateRefreshToken();

        await _cache.SetAsync($"{RedisRefreshTokenKey}{createdUser.UserId}", refreshToken, TimeSpan.FromDays(7));

        return new UserCreatedResponse
        {
            Token = accessToken,
            Message = "The user Successfully created",
            MetaData = new UserMetaData
            {
                UserId = createdUser.UserId,
                Email = createdUser.Email,
                UserSubscriptionPlan = userSubscriptionPlan,
                FirstName = createdUser.FirstName,
                MiddleName = createdUser.MiddleName,
                LastName = createdUser.LastName
            }
        };
    }

    public async Task<LoginResponse> SignInViaUap(UserLoginRequest request)
    {
        try
        {
            if (!await _userRepo.UserDoesExistByEmailAsync(request.Email))
                throw new EntityNotFoundException(typeof(User),
                    $"The user with the email {request.Email} does not exist.");

            User user = await _userRepo.GetUserByEmailAsync(request.Email);
            UserAuth userAuth = user.UserAuths.First(u => u.UserId == user.UserId);

            if (userAuth.PasswordHashed == null)
                throw new UserAccountDoesNotExistException($"{request.Email}",
                    "The user account does not exist by Email and password context, maybe you connect you had connected with Gmail.");

            if (!_passwordHasher.VerifyPassword(request.Password, userAuth.PasswordHashed))
                throw new UnauthorizedAccessException("The user credential is invalid");
            Subscription subscription = user.Subscriptions.First(s => s.SubscriptionStatus == "active");
            string userSubscriptionPlan = subscription.Plan.PlanName;
            RemainingTime? remain = null;
            if (subscription.EndDate != null)
            {
                TimeSpan remainTimes = subscription.EndDate.Value - DateTime.UtcNow;

                remain = new RemainingTime
                {
                    Days = remainTimes.Days,
                    Hours = remainTimes.Hours,
                    Minutes = remainTimes.Minutes
                };
            }

            string accessToken =
                _jwtGenerator.GenerateAccessToken(user.UserId.ToString(), user.Email, userSubscriptionPlan);
            string refreshToken = _jwtGenerator.GenerateRefreshToken();

            await _cache.SetAsync($"{RedisRefreshTokenKey}{user.UserId}", refreshToken, TimeSpan.FromDays(7));

            return new LoginResponse
            {
                Message = "User signin successfully",
                Token = accessToken,
                UserInfo = new UserInfoLoginData
                {
                    UserId = user.UserId,
                    IsEmailVerified = user.IsEmailVerified,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    SubscriptionDetails = new SubscriptionResponse
                    {
                        ExpiredAt = subscription.EndDate.GetValueOrDefault().ToString("O"),
                        UserSubscriptionPlan = userSubscriptionPlan,
                        RemainingTime = remain
                    }
                }
            };
        }
        catch (EntityNotFoundException)
        {
            _logger.Error("The user is not found");
            throw;
        }
        catch (UserAccountDoesNotExistException)
        {
            _logger.Error("The user account does not exist");
            throw;
        }
    }
}