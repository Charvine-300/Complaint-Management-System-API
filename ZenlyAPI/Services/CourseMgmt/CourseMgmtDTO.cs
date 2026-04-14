using Elasticsearch.Net;
using ZenlyAPI.Domain.Entities.Shared;
using ZenlyAPI.Domain.Utilities;

namespace ZenlyAPI.Services.CourseMgmt;

public record AllCoursesResponse(Guid Id, string Code, string Name, string Type);

public class CourseParameters: RequestParameters
{
    public Guid? DepartmentId { get; set; }
    public CourseType? Type { get; set; }
}