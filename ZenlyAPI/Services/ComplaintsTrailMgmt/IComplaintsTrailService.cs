namespace ZenlyAPI.Services.ComplaintsTrailMgmt;

public interface IComplaintsTrailService
{
    Task<bool> RecordComplaintActionAsync(ComplaintsTrailRequest log, CancellationToken cancellationToken);


}
