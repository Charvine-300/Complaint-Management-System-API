using ZenlyAPI.Domain.Utilities;

namespace ZenlyAPI.Services.CourseMgmt;

public interface ICourseMgmtService
{
    Task<ServiceResponse<PaginationResponse<AllCoursesResponse>>> GetCoursesAsync(CourseParameters parameters, CancellationToken cancellationToken);

}
