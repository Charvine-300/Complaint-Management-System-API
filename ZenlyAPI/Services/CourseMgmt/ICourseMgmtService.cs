using ZenlyAPI.Domain.Utilities;

namespace ZenlyAPI.Services.CourseMgmt;

public interface ICourseMgmtService
{
    Task<ServiceResponse<PaginationResponse<AllCoursesResponse>>> GetCoursesAsync(CourseParameters parameters, CancellationToken cancellationToken);
    Task<ServiceResponse<CourseDetailsResponse>> GetCourseDetailsAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResponse> CreateCourseAsync(CourseMgmtRequest request, CancellationToken cancellationToken);
     Task<ServiceResponse> UpdateCourseAsync(Guid id, CourseMgmtRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse> ChangeCourseTypeAsync(Guid id, ChangeCourseTypeRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse> DeleteCourseAsync(Guid id, CancellationToken cancellationToken);
}
