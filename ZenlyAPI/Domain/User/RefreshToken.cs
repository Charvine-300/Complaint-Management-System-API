using System.ComponentModel.DataAnnotations;
using ZenlyAPI.Domain.Entities.Shared;
using ZenlyAPI.Domain.User.Lecturers;
using ZenlyAPI.Domain.User.Students;

namespace ZenlyAPI.Domain.User;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    [Timestamp]
    public byte[] TimeStamp { get; set; }
    public Guid? StudentId { get; set; }
    public virtual Student? Student { get; set; }

    public Guid? LecturerId { get; set; }
    public virtual Lecturer? Lecturer { get; set; }
    public string Token { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime ExpiresAt { get; set; }
    public string? Roles { get; set; }
}
