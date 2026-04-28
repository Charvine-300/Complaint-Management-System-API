using System.ComponentModel.DataAnnotations;
using ZenlyAPI.Domain.Entities.Shared;

namespace ZenlyAPI.Services.ComplaintsTrailMgmt;

public record ComplaintsTrailResponse(
    Guid Id,
    string Action,
    string Comment,
    string Actor,
    ComplaintActionType? ActionType,
    DateTimeOffset CreatedAt
);

public class ComplaintsTrailRequest
{
    [Required]
    public Guid ComplaintId { get; set; }

    [Required]
    public string Action { get; set; }
    public string? Comment { get; set; }
    public string? Actor { get; set; }
    public ComplaintActionType ActionType { get; set; }
}
