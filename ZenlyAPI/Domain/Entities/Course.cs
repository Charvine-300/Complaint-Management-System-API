using ZenlyAPI.Domain.Entities.Complaints;
using ZenlyAPI.Domain.Entities.Shared;
using ZenlyAPI.Domain.User.Students;

namespace ZenlyAPI.Domain.Entities;

public class Course: BaseEntity
{
    public string Name { get; set; }
    public string Code { get; set; }
    public CourseType Type { get; set; } 
    public Guid DepartmentId { get; set; }
    public virtual Department Department { get; set; }
    public bool IsActive { get; set; } = true;
    public int? Year { get; set; }
    public SemesterType? Semester { get; set; }
    public virtual ICollection<Complaint> Compalints { get; set; } = new List<Complaint>();
    public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

    // TODO : Add Lecturer's nav property (M-to-M) - Create Associative Property
}
