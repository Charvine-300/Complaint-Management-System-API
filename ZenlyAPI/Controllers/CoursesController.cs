using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenlyAPI.Controllers.Shared;
using ZenlyAPI.Domain.Utilities;
using ZenlyAPI.Domain.Validators.Attributes;
using ZenlyAPI.Services.CourseMgmt;

namespace ZenlyAPI.Controllers;

[ApiController]
[Route("api/courses")]
//[Authorize]

public class CoursesController(ICourseMgmtService courseMgmtService) : BaseController
{
    /// <summary>
    /// Get all courses in a department
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("")]
    [HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse<PaginationResponse<AllCoursesResponse>>))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> GetCourses([FromQuery] CourseParameters parameters, CancellationToken cancellationToken)
    {
        ServiceResponse<PaginationResponse<AllCoursesResponse>> response = await courseMgmtService.GetCoursesAsync(parameters, cancellationToken);
        return ComputeResponse(response);
    }

};
