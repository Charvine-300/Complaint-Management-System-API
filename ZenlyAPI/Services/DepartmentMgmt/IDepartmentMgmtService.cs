using ZenlyAPI.Domain.Utilities;

namespace ZenlyAPI.Services.DepartmentMgmt;

public interface IDepartmentMgmtService
{
    Task<ServiceResponse<PaginationResponse<AllDepartmentsResponse>>> GetDepartmentsAsync(DepartmentParameters parameters, CancellationToken cancellationToken);
    Task<ServiceResponse<DepartmentDetailsResponse>> GetDepartmentDetailsAsync(Guid id,  CancellationToken cancellationToken);
    Task<ServiceResponse> CreateDepartmentAsync(DepartmentMgmtRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse> UpdateDepartmentAsync(Guid id, DepartmentMgmtRequest request, CancellationToken cancellationToken);
     Task<ServiceResponse> DeleteDepartmentAsync(Guid id, CancellationToken cancellationToken);
}
