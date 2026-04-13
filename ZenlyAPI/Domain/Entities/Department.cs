using ZenlyAPI.Domain.Entities.Shared;

namespace ZenlyAPI.Domain.Entities;

public class Department: BaseEntity
{
    public string Name { get; set; }
    public int MyProperty { get; set; }
    public Guid FacultyId { get; set; }
    public virtual Faculty Faculty { get; set; }
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
