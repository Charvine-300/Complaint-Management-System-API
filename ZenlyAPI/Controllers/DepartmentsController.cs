using Microsoft.AspNetCore.Mvc;
using ZenlyAPI.Controllers.Shared;
using ZenlyAPI.Domain.Utilities;
using ZenlyAPI.Domain.Validators.Attributes;
using ZenlyAPI.Services.CourseMgmt;
using ZenlyAPI.Services.DepartmentMgmt;

namespace ZenlyAPI.Controllers;

[ApiController]
[Route("api/departments")]
//[Authorize]


public class DepartmentsController(IDepartmentMgmtService departmentMgmtService) : BaseController
{
    /// <summary>
    /// Get all departments
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("")]


    [ProducesResponseType(200, Type = typeof(ApiResponse<PaginationResponse<AllDepartmentsResponse>>))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> GetDepartments([FromQuery] DepartmentParameters parameters, CancellationToken cancellationToken)
    {
        ServiceResponse<PaginationResponse<AllDepartmentsResponse>> response = await departmentMgmtService.GetDepartmentsAsync(parameters, cancellationToken);
        return ComputeResponse(response);
    }


    /// <summary>
    /// Get details of a department
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id}")]


    [ProducesResponseType(200, Type = typeof(ApiResponse<DepartmentDetailsResponse>))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> GetDepartmentDetails(Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse<DepartmentDetailsResponse> response = await departmentMgmtService.GetDepartmentDetailsAsync(id, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Create a department under a faculty
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("create")]


    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> CreateDepartment([FromBody] DepartmentMgmtRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ServiceResponse response = await departmentMgmtService.CreateDepartmentAsync(request, cancellationToken);
        return ComputeResponse(response);
    }


    /// <summary>
    /// Create a department under a faculty
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("update/{id}")]


    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> UpdateDepartment([FromRoute] Guid id, [FromBody] DepartmentMgmtRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ServiceResponse response = await departmentMgmtService.UpdateDepartmentAsync(id, request, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Delete a department
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("delete/{id}")]


    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> DeleteDepartment([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse response = await departmentMgmtService.DeleteDepartmentAsync(id, cancellationToken);
        return ComputeResponse(response);
    }

}
