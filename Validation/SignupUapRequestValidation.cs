using expense_tracker.Dto.Request.UserRequest;
using FluentValidation;

namespace expense_tracker.Validation;

public class SignupUapRequestValidation : AbstractValidator<SignUpByUapRequest>
{
    public SignupUapRequestValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email must not be empty")
            .EmailAddress().WithMessage("Must be a valid email.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password must not be empty")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,50}$")
            .WithMessage("Password must have at least one uppercase, one lowercase, and a number.")
            .Length(8, 50).WithMessage("Password must be between 8 and 50 characters long.");

        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("The password must match.");
    }
}