using ZenlyAPI.Domain.Entities;
using ZenlyAPI.Domain.Entities.Shared;

namespace ZenlyAPI.Domain.User;

public abstract class BaseUser: BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public UserType Type { get; set; }
    public Guid DepartmentId { get; set; }
    public virtual Department Department { get; set; }
    public Guid FacultyId { get; set; }
    public virtual Faculty Faculty { get; set; }
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
