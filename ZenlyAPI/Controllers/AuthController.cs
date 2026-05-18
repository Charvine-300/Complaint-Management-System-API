using Microsoft.AspNetCore.Mvc;
using ZenlyAPI.Controllers.Shared;
using ZenlyAPI.Domain.Utilities;
using ZenlyAPI.Services.AuthMgmt;

namespace ZenlyAPI.Controllers;

[ApiController]
[Route("api/auth")]

public class AuthController(IAuthService authService) : BaseController
{
    /// <summary>
    /// Log in to student account
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("login")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse<LoginResponse>))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> Login([FromBody] LoginRequest parameters, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ServiceResponse<LoginResponse> response = await authService.LoginAsync(parameters, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Sign up to student account
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("signup/student")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> StudentSignup([FromBody] StudentSignupRequest parameters, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ServiceResponse response = await authService.StudentSignupAsync(parameters, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Sign up to lecturer account
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("signup/lecturer")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> LecturerSignup([FromBody] LecturerSignupRequest parameters, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ServiceResponse response = await authService.LecturerSignupAsync(parameters, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Change password of student/lecturer
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("change-password")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest parameters, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ServiceResponse response = await authService.ChangePasswordAsync(parameters, cancellationToken);
        return ComputeResponse(response);
    }
}
