using ZenlyAPI.Domain.Entities.Shared;
using ZenlyAPI.Domain.Utilities;

namespace ZenlyAPI.Services.CourseMgmt;

public record AllCoursesResponse(Guid Id, string Code, string Name, string Type, bool IsActive);
public record CourseDetailsResponse(Guid Id, string Code, string Name, string Type, bool IsActive, string Department, string Faculty);

public class CourseMgmtRequest
{
    public string Name { get; set; }
    public string Code { get; set; }
    public CourseType Type { get; set; }
    public Guid DepartmentId { get; set; }
}

public class ChangeCourseTypeRequest
{
    public CourseType Type { get; set; }
}   

public class CourseParameters: RequestParameters
{
    public Guid? DepartmentId { get; set; }
    public CourseType? Type { get; set; }
}