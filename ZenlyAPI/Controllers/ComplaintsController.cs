using Microsoft.AspNetCore.Mvc;
using ZenlyAPI.Controllers.Shared;
using ZenlyAPI.Domain.Utilities;
using ZenlyAPI.Services.ComplaintsMgmt;

namespace ZenlyAPI.Controllers;

[ApiController]
[Route("api/complaints")]
//[Authorize]
public class ComplaintsController(IComplaintsService complaintsService) : BaseController
{
    /// <summary>
    /// Get all complaints
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse<PaginationResponse<AllComplaintsResponse>>))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> GetComplaints([FromQuery] ComplaintsParameters parameters, CancellationToken cancellationToken)
    {
        ServiceResponse<PaginationResponse<AllComplaintsResponse>> response = await complaintsService.GetAllComplaintsAsync(parameters, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Get details of a specific complaint
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse<ComplaintDetailsResponse>))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> GetComplaintDetails([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse<ComplaintDetailsResponse> response = await complaintsService.GetComplaintDetailsAsync(id, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Log a complaint
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("create")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> LogComplaint([FromBody] ComplaintMgmtRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ServiceResponse response = await complaintsService.LogComplaintAsync(request, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Update a complaint
    /// </summary>
    /// <param name="request"></param>
    /// <param name="id""></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("update/{id}")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> UpdateComplaint([FromRoute] Guid id, [FromBody] ComplaintMgmtRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ServiceResponse response = await complaintsService.UpdateComplaintAsync(id, request, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Update a complaint's status
    /// </summary>
    /// <param name="request"></param>
    /// <param name="id""></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("status/{id}/update")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> UpdateComplaintStatus([FromRoute] Guid id, [FromBody] ComplaintStatusMgmtRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ServiceResponse response = await complaintsService.UpdateComplaintStatusAsync(id, request, cancellationToken);
        return ComputeResponse(response);
    }

    /// <summary>
    /// Delete a complaint
    /// </summary>
    /// <param name="id""></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("delete/{id}")]
    //[HasPermission("trail.read")]

    [ProducesResponseType(200, Type = typeof(ApiResponse))]
    [ProducesResponseType(400, Type = typeof(ApiResponse))]
    [ProducesResponseType(404, Type = typeof(ApiResponse))]
    public async Task<IActionResult> DeleteComplaint([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse response = await complaintsService.DeleteComplaintAsync(id, cancellationToken);
        return ComputeResponse(response);
    }
}
