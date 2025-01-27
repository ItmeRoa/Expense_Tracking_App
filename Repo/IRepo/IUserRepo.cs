using expense_tracker.Dto.Request.UserRequest;
using expense_tracker.Dto.Response;

namespace expense_tracker.Repo.IRepo;

public interface IUserRepo
{
    Task SignupByUaPAsync(SignUpByUapRequest request); // 202 
    Task<SignUpEmailVerificationResponse> EmailVerificationAsync(VerifyEmailRequest request); // 200;
    Task<UserCreatedResponse> CreateUserAsync(NameReuqest request); // 201
}