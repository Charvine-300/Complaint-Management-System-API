using ZenlyAPI.Services.DepartmentMgmt;

namespace ZenlyAPI.Services.FacultyMgmt;

// TODO: Add property for Departments
public record FacultyResponse(Guid Id, string Name);

public record FacultyDetailsResponse(Guid Id, string Name, List<AllDepartmentsResponse> Departments);

public class  FacultyMgmtRequest 
{
    public string Name { get; set; }
}

