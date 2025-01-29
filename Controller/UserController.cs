using expense_tracker.Dto.Request.UserRequest;
using expense_tracker.Dto.Response;
using expense_tracker.Service.IService;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace expense_tracker.Controller;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<SignUpByUapRequest> _validator;

    public UserController(IUserService userService, IValidator<SignUpByUapRequest> validator)
    {
        _userService = userService;
        _validator = validator;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById([FromRoute] int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(user);
    }

    [HttpPost("signup/")]
    public async Task<IActionResult> SignupUap([FromBody] SignUpByUapRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        SignupUapResponse response = await _userService.SignupByUaPAsync(request);
        return Accepted(response);
    }

    [HttpPost("signup/email-verification/{session:guid}")]
    public async Task<IActionResult> EmailVerification(Guid session, VerifyEmailRequest request)
    {
        SignUpEmailVerificationResponse response =
            await _userService.EmailVerificationAsync(session, request);

        return Ok(response);
    }
}