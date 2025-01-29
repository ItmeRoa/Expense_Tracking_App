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

            EmailMetadata emailMetadata = new EmailMetadata
            {
                Subject = "Email Verification",
                ToAddress = request.Email,
                TemplatePath = "EmailVerification"
            };

            EmailVerificationMsg emailMsg = new EmailVerificationMsg
            {
                ReceiverEmail = request.Email,
                VerificationCode = otp,
                VerificationExpTime = 15
            };
            RedisEmailUapPayload emailPayload = new RedisEmailUapPayload
            {
                Email = request.Email,
                Password = request.Password
            };

            await _cache.SetAsync($"otp.{request.Email}", otp, TimeSpan.FromMinutes(10));
            await _cache.SetAsync($"email.{request.Email}", emailPayload, TimeSpan.FromMinutes(15));
            await SendEmailAsync(emailMetadata, emailMsg);

            return new SignupUapResponse
            {
                Message = "Signup success",
                SignUpSessionExpAt = DateTime.UtcNow.AddMinutes(15)
            };
        }
        catch (EntityAlreadyExistException)
        {
            _logger.Warning("Please retry with a different email.");
            throw;
        }
    }

    public Task<SignUpEmailVerificationResponse> EmailVerificationAsync(VerifyEmailRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<UserCreatedResponse> CreateUserAsync(NameReuqest request)
    {
        throw new NotImplementedException();
    }
}