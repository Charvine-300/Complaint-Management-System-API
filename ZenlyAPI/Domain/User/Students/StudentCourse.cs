using ZenlyAPI.Domain.Entities;
using ZenlyAPI.Domain.Entities.Shared;

namespace ZenlyAPI.Domain.User.Students;

public class StudentCourse: BaseEntity
{
    public Guid StudentId { get; set; }
    public virtual Student Student { get; set; }

    public Guid CourseId { get; set; }
    public virtual Course Course { get; set; }

    // optional extra fields (very useful in real systems)
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public string? Grade { get; set; }
}
