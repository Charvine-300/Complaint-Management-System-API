using ZenlyAPI.Domain.Utilities;

namespace ZenlyAPI.Services.DepartmentMgmt;

public interface IDepartmentMgmtService
{
    Task<ServiceResponse<PaginationResponse<AllDepartmentsResponse>>> GetDepartmentsAsync(DepartmentParameters parameters, CancellationToken cancellationToken);
}
