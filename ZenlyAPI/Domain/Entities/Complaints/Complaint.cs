using ZenlyAPI.Domain.Entities.Shared;

namespace ZenlyAPI.Domain.Entities.Complaints;

public class Complaint: BaseEntity
{
    public string Title { get; set; }
    public ComplaintType ComplaintType { get; set; }
    public string? Description { get; set; }
    public virtual Course Course { get; set; }
    public Guid CourseId { get; set; }
    public ComplaintStatus Status { get; set; }
    public virtual ICollection<ComplaintsTrail> History { get; set; } = new List<ComplaintsTrail>();

    // TODO : Add Course's nav property (M-to-1)
    // TODO : Add Student's nav property (M-to-1)
    // TODO : Add Document/File uploads property (TBD)
    // TODO : Add Complaint Communication btw Student and Lecturer
}
