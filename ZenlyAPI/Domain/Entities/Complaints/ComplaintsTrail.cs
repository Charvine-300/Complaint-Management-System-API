using ZenlyAPI.Domain.Entities.Shared;

namespace ZenlyAPI.Domain.Entities.Complaints;

public class ComplaintsTrail: BaseEntity
{
    public Guid ComplaintId { get; set; }
    public virtual Complaint Complaint { get; set; }
    public string Action { get; set; }
    public string? Comment { get; set; }
    public string? Actor { get; set; } // Name of user who performed the action
    public ComplaintActionType ActionType { get; set; }
}
