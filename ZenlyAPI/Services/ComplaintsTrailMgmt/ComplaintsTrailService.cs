using Serilog;
using ZenlyAPI.Context;
using ZenlyAPI.Domain.Entities.Complaints;
using ZenlyAPI.Services.Shared.UserContextService;

namespace ZenlyAPI.Services.ComplaintsTrailMgmt;

public class ComplaintsTrailService(ZenlyDbContext database, IUserContextService userContextService) : IComplaintsTrailService
{
    public async Task<bool> RecordComplaintActionAsync(ComplaintsTrailRequest log, CancellationToken cancellationToken)
    {
        try
        {
            ComplaintsTrail trail = new()
            {
                ComplaintId = log.ComplaintId,
                Action = log.Action,
                Comment = log.Comment,
                Actor = log.Actor,
                ActionType = log.ActionType,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userContextService.User.Id,
            };

            await database.ComplaintsTrail.AddAsync(trail, cancellationToken);
            database.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, $"An error occurred while updating complaint history for complaintID: {log.ComplaintId}");
            return false;
        }
    }
}
