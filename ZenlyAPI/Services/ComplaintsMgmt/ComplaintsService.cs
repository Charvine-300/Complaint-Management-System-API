using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ZenlyAPI.Context;
using ZenlyAPI.Domain.Config;
using ZenlyAPI.Domain.Entities;
using ZenlyAPI.Domain.Entities.Complaints;
using ZenlyAPI.Domain.Entities.Shared;
using ZenlyAPI.Domain.Utilities;
using ZenlyAPI.Services.ComplaintsTrailMgmt;
using ZenlyAPI.Services.Shared;

namespace ZenlyAPI.Services.ComplaintsMgmt;

public class ComplaintsService(ZenlyDbContext database, IComplaintsTrailService trailService, ZenlyConfig zenlyConfig, IConfiguration configuration): IComplaintsService
{
    private readonly Cloudinary _cloudinary =
      new Cloudinary(
          new Account(
              zenlyConfig.CloudinaryConfig.CloudName,
              zenlyConfig.CloudinaryConfig.ApiKey,
              zenlyConfig.CloudinaryConfig.ApiSecret
          )
      );


    public async Task<ServiceResponse<PaginationResponse<AllComplaintsResponse>>> GetAllComplaintsAsync(ComplaintsParameters parameters, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Add student ID for fetching current user's complaints
            IQueryable<Complaint> query = database.Complaints.Include(c => c.Course).AsNoTracking();

            //if (parameters.CreatedBy.HasValue)
            //{
            //    query = query.Where(c => c.StudentId == parameters.CreatedBy);
            //}   

            if (parameters.CourseId.HasValue)
            {
                query = query.Where(c => c.CourseId == parameters.CourseId);
            }

            if (parameters.Status.HasValue)
            {
                query = query.Where(c => c.Status == parameters.Status);
            }

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                string searchTerm = parameters.Search.Trim().ToLower();
                query = query.Where(c => c.Title.ToLower().Contains(searchTerm)
                                         || c.Description.ToLower().Contains(searchTerm)
                );
            }

            if (parameters.StartDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= parameters.StartDate.Value);
            }

            if (parameters.EndDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= parameters.EndDate.Value);
            }


            int totalCount = await query.CountAsync(cancellationToken);
            int totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);

            var complaints = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync(cancellationToken);

            List<AllComplaintsResponse> data = complaints.Select(c => new AllComplaintsResponse
            (
                c.Id,
                c.Title,
                c.ComplaintType.ToString(),
                c.Status.ToString(),
                c.Course.Code + " - " + c.Course.Name,
                c.CreatedAt
            )).ToList();


            return Response.Success("Complaints retrieved successfully", new PaginationResponse<AllComplaintsResponse>
            (
                Records: data,
                TotalRecords: totalCount,
                TotalPages: totalPages,
                CurrentPage: parameters.PageNumber,
                PageSize: parameters.PageSize
            ));
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, $"An error occurred while fetching complaints");
            return Response.SystemMalfunction<PaginationResponse<AllComplaintsResponse>>("An error occurred while processing your request. Please try again later.", null!);
        }
    }

    public async Task<ServiceResponse<ComplaintDetailsResponse>> GetComplaintDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            Complaint? complaintDetails = await database.Complaints.Include(c => c.Course)
                .Include(c => c.History)
                .Include(c => c.Documents)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaintDetails == null)
            {
                return Response.NotFound<ComplaintDetailsResponse>("Complaint not found", null!);
            }


            ComplaintDetailsResponse data = new ComplaintDetailsResponse
                (
                    complaintDetails.Id,
                    complaintDetails.Title,
                    complaintDetails.ComplaintType.ToString(),
                    complaintDetails.Description,
                    complaintDetails.Status.ToString(),
                    complaintDetails.Course.Code + " - " + complaintDetails.Course.Name,
                    complaintDetails.CreatedAt,
                    complaintDetails.Documents.OrderByDescending(d => d.CreatedAt).Select(d => new ComplaintUploadsResponse
                    (
                        d.Id,
                        d.ImageUrl
                    )).ToList(),
                    complaintDetails.History.OrderBy(t => t.CreatedAt).Select(h => new ComplaintsTrailResponse
                    (
                        h.Id,
                        h.Action,
                        h.Comment,
                        h.Actor,
                        h.ActionType,
                        h.CreatedAt
                    )).ToList()
                );

            return Response.Success("Complaint details retrieved successfully", data);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, $"An error occurred while fetching complaint details for complaintID: {id}");
            return Response.SystemMalfunction<ComplaintDetailsResponse>("An error occurred while processing your request. Please try again later.", null!);
        }
    }

    public async Task<ServiceResponse> LogComplaintAsync(ComplaintMgmtRequest request, CancellationToken cancellationToken)
    {
        try
        {
            Course? courseDetails = await database.Courses.FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

            if (courseDetails == null)
            {
                return Response.NotFound("This course does not exist");
            }

            //TODO - Add checker for user creating duplicate complaints (enforcing Idempotency)

            Complaint logComplaint = new()
            {
                Title = request.Title,
                ComplaintType = request.ComplaintType,
                Description = request.Description,
                CourseId = request.CourseId,
                Status = ComplaintStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow
                //TODO - Add CreatedBy content here
            };


            // Uploading probable images related to this complaint and getting their URLs
            if (request.Uploads != null && request.Uploads.Count > 0)
            {
                List<ComplaintUpload> uploadedPhotos = await UploadPhotoAsync(new ImageCreationDTO
                {
                    ComplaintId = logComplaint.Id,
                    CourseCode = courseDetails.Code,
                    Uploads = request.Uploads
                });

                logComplaint.Documents = uploadedPhotos;
            }


            await database.Complaints.AddAsync(logComplaint, cancellationToken);
            await database.ComplaintUploads.AddRangeAsync(logComplaint.Documents, cancellationToken);
            await database.SaveChangesAsync(cancellationToken);

            // Record complaint hsitory
            ComplaintsTrailRequest complaintHistory = new()
            {
                ComplaintId = logComplaint.Id,
                Action = "Create Complaint",
                Comment = $"Complaint logged for course {courseDetails.Code} - {courseDetails.Name}",
                Actor = "Student", //TODO - Add actual actor content here
                ActionType = ComplaintActionType.Create
            };

            await trailService.RecordComplaintActionAsync(complaintHistory, cancellationToken);


            return Response.Created("Complaint logged successfully");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, $"An error occurred while logging complaint");
            return Response.SystemMalfunction("An error occurred while processing your request. Please try again later.");
        }
    }

    public async Task<ServiceResponse> UpdateComplaintAsync(Guid id, ComplaintMgmtRequest request, CancellationToken cancellationToken)
    {
        try
        {
            Complaint? complaintDetails = await database.Complaints.FirstOrDefaultAsync(c => c.Id == id);

            if (complaintDetails == null)
            {
                return Response.NotFound("This Complaint does not exist");
            }

            Course? courseDetails = await database.Courses.FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

            if (courseDetails == null)
            {
                return Response.NotFound("This course does not exist");
            }


            complaintDetails.Title = request.Title;
            complaintDetails.ComplaintType = request.ComplaintType;
            complaintDetails.Description = request.Description;
            complaintDetails.CourseId = request.CourseId;
            complaintDetails.ModifiedAt = DateTimeOffset.UtcNow;
            //TODO - Add ModifiedBy content here

            await database.SaveChangesAsync(cancellationToken);


            ComplaintsTrailRequest complaintHistory = new()
            {
                ComplaintId = id,
                Action = "Update Complaint",
                Comment = $"Details of this complaint were edited",
                Actor = "Student", //TODO - Add actual actor content here
                ActionType = ComplaintActionType.Update
            };

            await trailService.RecordComplaintActionAsync(complaintHistory, cancellationToken);
            return Response.Success("Complaint updated successfully");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, $"An error occurred while updating complaint details for complaintID: {id}");
            return Response.SystemMalfunction("An error occurred while processing your request. Please try again later.");
        }
    }

    public async Task<ServiceResponse> UpdateComplaintStatusAsync(Guid id, ComplaintStatusMgmtRequest request, CancellationToken cancellationToken)
    {
        Complaint? complaintDetails = await database.Complaints.FirstOrDefaultAsync(c => c.Id == id);

        if (complaintDetails == null)
        {
            return Response.NotFound("This Complaint does not exist");
        }

        ComplaintStatus oldStatus = complaintDetails.Status;


        complaintDetails.Status = request.Status;
        complaintDetails.ModifiedAt = DateTimeOffset.UtcNow;
        //TODO - Add ModifiedBy content here

        await database.SaveChangesAsync(cancellationToken);

        ComplaintActionInfo action = request.Status switch
        {
            ComplaintStatus.Pending => new()
            {
                Title = "Complaint Submitted",
                Description = "The complaint has been received and is awaiting review."
            },

            ComplaintStatus.Ongoing => new()
            {
                Title = "Complaint In Progress",
                Description = "The complaint is currently being investigated."
            },

            ComplaintStatus.Resolved => new()
            {
                Title = "Complaint Resolved",
                Description = "The complaint has been successfully resolved."
            },

            ComplaintStatus.Canceled => new()
            {
                Title = "Complaint Canceled",
                Description = $"The complaint has been canceled due to - {request.Reason}."
            },

            _ => new()
            {
                Title = "Complaint Status Updated",
                Description = $"The status of this complaint was updated from {oldStatus.ToString()} -> {request.Status.ToString()}."
            }
        };


        ComplaintsTrailRequest complaintHistory = new()
        {
            ComplaintId = id,
            Action = $"{action.Title}",
            Comment = $"{action.Description}",
            Actor = "Student/Lecturer", //TODO - Add actual actor content here
            ActionType = ComplaintActionType.Update
        };

        await trailService.RecordComplaintActionAsync(complaintHistory, cancellationToken);
        return Response.Success("Complaint status updated successfully");
    }

    public async Task<ServiceResponse> DeleteComplaintAsync(Guid id, CancellationToken cancellationToken)
    {
        Complaint? complaintDetails = await database.Complaints.FirstOrDefaultAsync(c => c.Id == id);

        if (complaintDetails == null)
        {
            return Response.NotFound("This Complaint does not exist");
        }


        database.Complaints.Remove(complaintDetails);
        await database.SaveChangesAsync(cancellationToken);

        return Response.Success("Complaint deleted successfully");
    }

    private async Task<List<ComplaintUpload>> UploadPhotoAsync(ImageCreationDTO imgDetails)
    {
        var uploads = new List<ComplaintUpload>();

        foreach (var file in imgDetails.Uploads)
        {
            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Folder = $"Zenly/{imgDetails.CourseCode}"
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            var Url = uploadResult.Url.ToString();
            var PublicId = uploadResult.PublicId;

            uploads.Add(new ComplaintUpload
            {
                ImageUrl = Url,
                PublicId = PublicId,
                CreatedAt = DateTimeOffset.UtcNow,
                ComplaintId = imgDetails.ComplaintId,
            });

        };

        return uploads;
    }
}
