using ZenlyAPI.Domain.Utilities;

namespace ZenlyAPI.Services.DepartmentMgmt;

public record AllDepartmentsResponse(
    Guid Id,
    string Name
);

public class DepartmentParameters : RequestParameters {
    public Guid? FacultyId { get; set; }
}
