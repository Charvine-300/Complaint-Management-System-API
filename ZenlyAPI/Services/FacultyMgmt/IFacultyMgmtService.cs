using ZenlyAPI.Domain.Utilities;

namespace ZenlyAPI.Services.FacultyMgmt;

public interface IFacultyMgmtService
{
    Task<ServiceResponse<PaginationResponse<FacultyResponse>>> GetAllFacultiesAsync(RequestParameters parameters, CancellationToken cancellationToken);
    Task<ServiceResponse<FacultyDetailsResponse>> GetFacultyDetailsAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResponse> CreateFacultyAsync(FacultyMgmtRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse> UpdateFacultyAsync(Guid id, FacultyMgmtRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse> DeleteFacultyAsync(Guid id, CancellationToken cancellationToken);
}
