using Microsoft.EntityFrameworkCore;
using Serilog;
using ZenlyAPI.Context;
using ZenlyAPI.Domain.Entities;
using ZenlyAPI.Domain.Utilities;
using ZenlyAPI.Services.DepartmentMgmt;
using ZenlyAPI.Services.Shared;

namespace ZenlyAPI.Services.FacultyMgmt;

public class FacultyMgmtService(ZenlyDbContext database) : IFacultyMgmtService
{
    public async Task<ServiceResponse<PaginationResponse<FacultyResponse>>> GetAllFacultiesAsync(RequestParameters parameters, CancellationToken cancellationToken)
    {
        try
        {
            IQueryable<Faculty> query = database.Faculties.AsNoTracking();

            if (!string.IsNullOrEmpty(parameters.Search))
            {
                query = query.Where(f => f.Name.Contains(parameters.Search));
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

                var faculties = await query
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToListAsync(cancellationToken);

            List<FacultyResponse> data = faculties.Select(f => new FacultyResponse
            (
                f.Id,
                f.Name
            )).ToList();

            return Response.Success("Faculties retrieved successfully", new PaginationResponse<FacultyResponse>
            (
                Records: data,
                PageSize: parameters.PageSize,
                CurrentPage: parameters.PageNumber,
                TotalPages: totalPages,
                TotalRecords: totalCount
            ));
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while fetching faculties");
            return Response.SystemMalfunction<PaginationResponse<FacultyResponse>>("An error occurred while processing your request. Please try again later.", null!);
        }
    }

    public async Task<ServiceResponse<FacultyDetailsResponse>> GetFacultyDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            Faculty? faculty = await database.Faculties.AsNoTracking()
                .Include(f => f.Departments)
                .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

            if (faculty == null)
            {
                return Response.NotFound<FacultyDetailsResponse>("This faculty does not exist", default!);
            }

            FacultyDetailsResponse data = new FacultyDetailsResponse
            (
                Id: faculty.Id,
                Name: faculty.Name,
                Departments: faculty.Departments.Select(d => new AllDepartmentsResponse
                (
                    Id: d.Id,
                    Name: d.Name
                )).ToList()
            );

            return Response.Success("Faculty details retrieved successfully", data);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while fetching faculty details");
            return Response.SystemMalfunction<FacultyDetailsResponse>("An error occurred while processing your request. Please try again later.", null!);
        }
    }

    public async Task<ServiceResponse> CreateFacultyAsync(FacultyMgmtRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if faculty already exists
            bool facultyExists = await database.Faculties.AnyAsync(f => f.Name.ToLower() == request.Name.ToLower(), cancellationToken);

            if (facultyExists)
            {
                return Response.Conflict("A faculty with this name already exists");
            }

            Faculty newFaculty = new Faculty
            {
                    Name = request.Name,
                    CreatedAt = DateTimeOffset.UtcNow
                    //TODO - Add CreatedBy content here
            };

            database.Faculties.Add(newFaculty);
            await database.SaveChangesAsync(cancellationToken);

            return Response.Created("Faculty created successfully");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while creating faculty");
            return Response.SystemMalfunction("An error occurred while processing your request. Please try again later.");
        }
    }

    public async Task<ServiceResponse> UpdateFacultyAsync(Guid id, FacultyMgmtRequest request, CancellationToken cancellationToken)
    {
        try
        {
            Faculty? faculty = await database.Faculties.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

            if (faculty == null)
            {
                return Response.NotFound("This faculty does not exist");
            }


            faculty.Name = request.Name;
            faculty.ModifiedAt = DateTimeOffset.UtcNow;
            // TODO - Add ModifiedBy content here

            await database.SaveChangesAsync(cancellationToken);

            return Response.Success("Faculty updated successfully");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while updating faculty");
            return Response.SystemMalfunction("An error occurred while processing your request. Please try again later.");
        }
    }

    public async Task<ServiceResponse> DeleteFacultyAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            Faculty? faculty = await database.Faculties.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

            if (faculty == null)
            {
                return Response.NotFound("This faculty does not exist");
            }

            database.Faculties.Remove(faculty);
            await database.SaveChangesAsync(cancellationToken);

            return Response.Success("Faculty deleted successfully");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while deleting faculty");
            return Response.SystemMalfunction("An error occurred while processing your request. Please try again later.");
        }
    }
}
