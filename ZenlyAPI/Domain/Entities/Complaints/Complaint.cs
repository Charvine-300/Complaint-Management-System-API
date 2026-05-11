using ZenlyAPI.Domain.Entities.Shared;
using ZenlyAPI.Domain.User.Students;

namespace ZenlyAPI.Domain.Entities.Complaints;

public class Complaint: BaseEntity
{
    public string Title { get; set; }
    public ComplaintType ComplaintType { get; set; }
    public string? Description { get; set; }
    public virtual Course Course { get; set; }
    public Guid CourseId { get; set; }
    public ComplaintStatus Status { get; set; }
    public Guid? StudentId { get; set; }
    public virtual Student? Student { get; set; }
    public virtual ICollection<ComplaintsTrail> History { get; set; } = new List<ComplaintsTrail>();
    public virtual ICollection<ComplaintUpload> Documents { get; set; } = new List<ComplaintUpload>();

    // TODO : Add Complaint Communication btw Student and Lecturer
}
