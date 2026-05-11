using ZenlyAPI.Domain.Entities.Shared;
using ZenlyAPI.Domain.User.Students;

namespace ZenlyAPI.Domain.Entities;

public class Faculty: BaseEntity
{
    public string Name { get; set; }
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
}
