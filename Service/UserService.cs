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
    private const string RedisKeyOtp = "_session.opt.";
    private const string RedisEmailCreationPayload = "_session.email.";
    private const string RedisVerifiedEmailCreationPayload = "_session.email.verified.";

    public UserService(IUserRepo userRepo, ILogger logger, RedisCaching cache, PasswordHasher passwordHasher,
        OtpGenerator otpGenerator, RazorPageRenderer razorPageRenderer, IFluentEmail fluentEmail)
    {
        _userRepo = userRepo;
        _logger = logger;
        _cache = cache;
        _passwordHasher = passwordHasher;
        _otpGenerator = otpGenerator;
        _razorPageRenderer = razorPageRenderer;
        _fluentEmail = fluentEmail;
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

    public Task<User> GetUserByIdAsync(int id)
    {
        return _userRepo.GetUserByIdAsync(id);
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
            await _cache.SetAsync($"{RedisEmailCreationPayload}{session}", new RedisEmailUapPayload
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
                SignUpSessionExpAt = DateTime.UtcNow.AddMinutes(15)
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
            await _cache.GetAsync<RedisEmailUapPayload>($"{RedisEmailCreationPayload}{session}") ??
            throw new CachedTokenException(
                "The user creation payload has expired or does not exist");

        payload.IsEmailVerified = true;
        await _cache.SetAsync($"{RedisVerifiedEmailCreationPayload}{session}", payload, TimeSpan.FromMinutes(10));

        return new SignUpEmailVerificationResponse
        {
            Message = "Email successfully verified.",
            Session = session,
            ExpiredAt = DateTime.UtcNow.AddMinutes(10)
        };
    }

    public Task<UserCreatedResponse> CreateUserAsync(Guid session, NameReuqest request)
    {
        throw new NotImplementedException();
    }
}