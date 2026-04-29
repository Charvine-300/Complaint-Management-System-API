using ZenlyAPI.Domain.Entities.Shared;

namespace ZenlyAPI.Domain.Entities.Complaints
{
    public class ComplaintUpload: BaseEntity
    {
            public Guid ComplaintId { get; set; }
            public virtual Complaint? Complaint { get; set; }
            public required string ImageUrl { get; set; }
            public string PublicId { get; set; }
    }
}
