using expense_tracker.Dto.Request.UserRequest;
using FluentValidation;

namespace expense_tracker.Validation;

public class NameRequestValidation : AbstractValidator<NameReuqest>
{
    public NameRequestValidation()
    {
        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Your firstname cannot be empty")
            .Must(firstName => !firstName.Contains(' ')).WithMessage("Firstname can't contain spaces")
            .Matches(@"^[a-zA-Z]{8,}$").WithMessage("Firstname can't contain any special character")
            .Length(8, 50).WithMessage("Firstname can only be between 8 to 50");
        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Your  lastname cannot be empty")
            .Must(lastName => !lastName.Contains(' ')).WithMessage("Lastname can't contain spaces")
            .Matches(@"^[a-zA-Z]{8,}$").WithMessage("Lastname can't contain any special character")
            .Length(8, 50).WithMessage("Lastname can only be between 8 to 50");
        RuleFor(x => x.MiddleName)
            .Cascade(CascadeMode.Stop)
            .Matches(@"^[a-zA-Z]{0,}$").WithMessage("Middle name can't contain any special character")
            .Length(0, 50).WithMessage("Middle name can only be between 8 to 50");
    }
}