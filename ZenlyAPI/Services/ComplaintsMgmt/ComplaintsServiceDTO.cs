using System.ComponentModel.DataAnnotations;
using ZenlyAPI.Domain.Entities.Shared;
using ZenlyAPI.Domain.Utilities;
using ZenlyAPI.Services.ComplaintsTrailMgmt;
using static ZenlyAPI.Domain.Utilities.StartsWithAttribute;

namespace ZenlyAPI.Services.ComplaintsMgmt;

public record AllComplaintsResponse(Guid Id, string Title, string ComplaintType, string Status, string Course, DateTimeOffset CreatedAt);

public record ComplaintDetailsResponse(Guid Id, string Title, string ComplaintType, string? Description, string Status, string Course, DateTimeOffset CreatedAt, List<ComplaintUploadsResponse> Documents, List<ComplaintsTrailResponse> History);

public record ComplaintUploadsResponse(Guid Id, string Url);

public class ComplaintsParameters: RequestParameters
{
    public Guid? CreatedBy { get; set; }
    public ComplaintStatus? Status { get; set; }
    public ComplaintType? complaintType { get; set; }
    public Guid? CourseId { get; set; }
    public string? ComplaintType { get; set; }
}

public class ComplaintMgmtRequest {
    [Required]
    public string Title { get; set; }

    [Required]
    public ComplaintType ComplaintType { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    public string? Description { get; set; }

    [ValidImgTypeAndSize(5)]
    public List<IFormFile> Uploads { get; set; } = new();
}

public class ComplaintStatusMgmtRequest
{
    [Required]
    public ComplaintStatus Status { get; set; }
    public string? Reason { get; set; }
}

public class ComplaintActionInfo
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}

public class ImageCreationDTO
{
    public string CourseCode { get; set; }

    [Required]
    [ValidImgTypeAndSize(5)]
    public List<IFormFile> Uploads { get; set; } = new();

    [Required]
    public Guid ComplaintId { get; set; }
}