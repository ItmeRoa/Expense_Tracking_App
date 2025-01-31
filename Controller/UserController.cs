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
    public async Task<ActionResult<UserCheckedByIdResponse>> GetUserById([FromRoute] int id)
    {
        UserCheckedByIdResponse user = await _userService.GetUserByIdAsync(id);
        return Ok(user);
    }

    [HttpPost("signup/")]
    public async Task<ActionResult<SignupUapResponse>> SignupUap([FromBody] SignUpByUapRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        SignupUapResponse response = await _userService.SignupByUaPAsync(request);
        return Accepted(response);
    }

    [HttpPost("signup/email-verification/")]
    public async Task<ActionResult<SignUpEmailVerificationResponse>> EmailVerification(
        [FromHeader(Name = "Auth-Session-Token")]
        Guid session,
        [FromBody] VerifyEmailRequest request)
    {
        SignUpEmailVerificationResponse response =
            await _userService.EmailVerificationAsync(session, request);

        return Ok(response);
    }

    [HttpPost("signup/user-creation/")]
    public async Task<ActionResult<UserCreatedResponse>> UserCreationAsync(
        [FromHeader(Name = "Auth-Session-Token")]
        Guid session,
        [FromBody] NameReuqest request)
    {
        UserCreatedResponse response = await _userService.CreateUserAsync(session, request);

        HttpContext.Response.Cookies.Append("AuthToken", response.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        });

        return CreatedAtAction(nameof(GetUserById), new { id = response.MetaData.UserId }, response);
    }

    [HttpPost("signup/signin")]
    public async Task<ActionResult<LoginResponse>> SignIn([FromBody] UserLoginRequest request)
    {
        LoginResponse response = await _userService.SignInViaUap(request);
        HttpContext.Response.Cookies.Append("AuthToken", response.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        });

        return Ok(response);
    }
}