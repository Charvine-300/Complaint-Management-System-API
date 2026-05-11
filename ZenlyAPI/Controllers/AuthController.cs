using Microsoft.AspNetCore.Mvc;
using ZenlyAPI.Controllers.Shared;
using ZenlyAPI.Domain.Utilities;
using ZenlyAPI.Services.AuthMgmt;
using ZenlyAPI.Services.ComplaintsMgmt;

namespace ZenlyAPI.Controllers;

[ApiController]
[Route("api/auth")]

public class AuthController(IAuthService authService) : BaseController
{
    /// <summary>
    /// Log in to account
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("login/student")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse<LoginResponse>))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> Login([FromBody] StudentLoginRequest parameters, CancellationToken cancellationToken)
    {
        ServiceResponse<LoginResponse> response = await authService.StudentLoginAsync(parameters, cancellationToken);
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
        ServiceResponse response = await authService.StudentSignupAsync(parameters, cancellationToken);
        return ComputeResponse(response);
    }
}
