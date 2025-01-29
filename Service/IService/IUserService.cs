using expense_tracker.Dto.Request.UserRequest;
using expense_tracker.Dto.Response;
using expense_tracker.Model;

namespace expense_tracker.Service.IService;

public interface IUserService
{
    Task<User> GetUserByIdAsync(int id);
    Task<SignupUapResponse> SignupByUaPAsync(SignUpByUapRequest request); 
    Task<SignUpEmailVerificationResponse> EmailVerificationAsync(VerifyEmailRequest request); 
    Task<UserCreatedResponse> CreateUserAsync(NameReuqest request); 
}