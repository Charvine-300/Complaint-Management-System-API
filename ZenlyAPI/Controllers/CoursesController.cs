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
    /// Get all courses
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse<PaginationResponse<AllCoursesResponse>>))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> GetCourses([FromQuery] CourseParameters parameters, CancellationToken cancellationToken)
    {
        ServiceResponse<PaginationResponse<AllCoursesResponse>> response = await courseMgmtService.GetCoursesAsync(parameters, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Get details of a course by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse<CourseDetailsResponse>))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> GetCourseDetails([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse<CourseDetailsResponse> response = await courseMgmtService.GetCourseDetailsAsync(id, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Create a course
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("create")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> CreateCourse([FromBody] CourseMgmtRequest request, CancellationToken cancellationToken)
    {
        ServiceResponse response = await courseMgmtService.CreateCourseAsync(request, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Update a course
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("update/{id}")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> UpdateCourse([FromRoute] Guid id, [FromBody] CourseMgmtRequest request, CancellationToken cancellationToken)
    {
        ServiceResponse response = await courseMgmtService.UpdateCourseAsync(id, request, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Change course type (Compulsory/Elective)
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("update/course-type/{id}")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> UpdateCourseType([FromRoute] Guid id, [FromBody] ChangeCourseTypeRequest request, CancellationToken cancellationToken)
    {
        ServiceResponse response = await courseMgmtService.ChangeCourseTypeAsync(id, request, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Delete a course
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("delete/{id}")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> DeleteCourse([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse response = await courseMgmtService.DeleteCourseAsync(id, cancellationToken);
        return ComputeResponse(response);
    }
};
