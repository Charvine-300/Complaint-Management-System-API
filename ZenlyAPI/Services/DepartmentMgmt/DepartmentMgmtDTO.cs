using ZenlyAPI.Domain.Utilities;
using ZenlyAPI.Services.CourseMgmt;

namespace ZenlyAPI.Services.DepartmentMgmt;

public record AllDepartmentsResponse(
    Guid Id,
    string Name
);

public record DepartmentDetailsResponse(
    Guid Id,
    string Name,
    List<AllCoursesResponse> Courses
);

public class DepartmentParameters : RequestParameters {
    public Guid? FacultyId { get; set; }
}

public class DepartmentMgmtRequest
{
    [StartsWith("Department of")]
    public string Name { get; set; }
    public Guid FacultyId { get; set; }
}
