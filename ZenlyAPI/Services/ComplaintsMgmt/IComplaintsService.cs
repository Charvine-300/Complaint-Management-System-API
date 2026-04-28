using ZenlyAPI.Domain.Utilities;

namespace ZenlyAPI.Services.ComplaintsMgmt;

public interface IComplaintsService
{
    Task<ServiceResponse<PaginationResponse<AllComplaintsResponse>>> GetAllComplaintsAsync(ComplaintsParameters parameters, CancellationToken cancellationToken);
    Task<ServiceResponse<ComplaintDetailsResponse>> GetComplaintDetailsAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResponse> LogComplaintAsync(ComplaintMgmtRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse> UpdateComplaintAsync(Guid id, ComplaintMgmtRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse> UpdateComplaintStatusAsync(Guid id, ComplaintStatusMgmtRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse> DeleteComplaintAsync(Guid id, CancellationToken cancellationToken);

}
