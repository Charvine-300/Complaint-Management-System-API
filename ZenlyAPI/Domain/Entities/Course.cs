using ZenlyAPI.Domain.Entities.Shared;

namespace ZenlyAPI.Domain.Entities;

public class Course: BaseEntity
{
    public string Name { get; set; }
    public string Code { get; set; }
    public Guid DepartmentId { get; set; }
    public virtual Department Department { get; set; }

    // TODO : Add Complaint's nav property (O-to-M)
    // TODO : Add Lecturer's nav property (M-to-M) - Create Associative Property
    // TODO : Add Student's nav property (M-to-M) - Create Associative Property
}
