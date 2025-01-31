using expense_tracker.Dto.Request.UserRequest;
using expense_tracker.Dto.Response;
using expense_tracker.Model;

namespace expense_tracker.Service.IService;

public interface IUserService
{
    Task<UserCheckedByIdResponse> GetUserByIdAsync(int id);
    Task<SignupUapResponse> SignupByUaPAsync(SignUpByUapRequest request);
    Task<SignUpEmailVerificationResponse> EmailVerificationAsync(Guid session, VerifyEmailRequest request);
    Task<UserCreatedResponse> CreateUserAsync(Guid session,NameReuqest request);

    Task<LoginResponse> SignInViaUap(UserLoginRequest request);
}