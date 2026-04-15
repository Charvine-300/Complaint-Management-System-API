using Microsoft.AspNetCore.Mvc;
using ZenlyAPI.Controllers.Shared;
using ZenlyAPI.Domain.Utilities;
using ZenlyAPI.Services.CourseMgmt;
using ZenlyAPI.Services.FacultyMgmt;

namespace ZenlyAPI.Controllers;

[ApiController]
[Route("api/faculties")]
//[Authorize]

public class FacultiesController(IFacultyMgmtService facultyMgmtService) : BaseController
{
    /// <summary>
    /// Get all faculties
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse<PaginationResponse<AllCoursesResponse>>))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> GetFaculties([FromQuery] RequestParameters parameters, CancellationToken cancellationToken)
    {
        ServiceResponse<PaginationResponse<FacultyResponse>> response = await facultyMgmtService.GetAllFacultiesAsync(parameters, cancellationToken);
        return ComputeResponse(response);
    }


    /// <summary>
    /// Get details of a faculty by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse<FacultyDetailsResponse>))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> GetFacultyDetails(Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse<FacultyDetailsResponse> response = await facultyMgmtService.GetFacultyDetailsAsync(id, cancellationToken);
        return ComputeResponse(response);
    }


    /// <summary>
    /// Create a faculty
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("create")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> CreateFaculty(FacultyMgmtRequest payload, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ServiceResponse response = await facultyMgmtService.CreateFacultyAsync(payload, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Update a faculty
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("update/{id}")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> UpdateFaculty(Guid id, FacultyMgmtRequest payload, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ServiceResponse response = await facultyMgmtService.UpdateFacultyAsync(id, payload, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Delete a faculty
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("delete/{id}")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> DeleteFaculty(Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse response = await facultyMgmtService.DeleteFacultyAsync(id, cancellationToken);
        return ComputeResponse(response);
    }
}