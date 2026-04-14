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
}
