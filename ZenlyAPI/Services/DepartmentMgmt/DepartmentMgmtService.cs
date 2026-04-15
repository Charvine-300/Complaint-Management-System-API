using Microsoft.EntityFrameworkCore;
using Serilog;
using ZenlyAPI.Context;
using ZenlyAPI.Domain.Entities;
using ZenlyAPI.Domain.Utilities;
using ZenlyAPI.Services.CourseMgmt;
using ZenlyAPI.Services.FacultyMgmt;
using ZenlyAPI.Services.Shared;

namespace ZenlyAPI.Services.DepartmentMgmt;

public class DepartmentMgmtService(ZenlyDbContext database) : IDepartmentMgmtService
{
    public async Task<ServiceResponse<PaginationResponse<AllDepartmentsResponse>>> GetDepartmentsAsync(DepartmentParameters parameters, CancellationToken cancellationToken)
    {
        IQueryable<Department> query = database.Departments.AsNoTracking();
        
        if (parameters.FacultyId.HasValue)
        {
            query = query.Where(d => d.FacultyId == parameters.FacultyId);
        }

        if (!string.IsNullOrEmpty(parameters.Search))
        {
            query = query.Where(d => d.Name.ToLower().Contains(parameters.Search.ToLower()));
        }


        if (parameters.StartDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt >= parameters.StartDate.Value);
        }

        if (parameters.EndDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt <= parameters.EndDate.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);
        int totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);

        var faculties = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        List<AllDepartmentsResponse> data = faculties.Select(f => new AllDepartmentsResponse
        (
            f.Id,
            f.Name
        )).ToList();

        return Response.Success("Departments retrieved successfully", new PaginationResponse<AllDepartmentsResponse>
        (
            Records: data,
            PageSize: parameters.PageSize,
            CurrentPage: parameters.PageNumber,
            TotalPages: totalPages,
            TotalRecords: totalCount
        ));
    }

    public async Task<ServiceResponse<DepartmentDetailsResponse>> GetDepartmentDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            Department? department = await database.Departments.AsNoTracking().Include(d => d.Courses).FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return Response.NotFound<DepartmentDetailsResponse>("This department does not exist", null!);
            }

            DepartmentDetailsResponse data = new DepartmentDetailsResponse
        (
            department.Id,
            department.Name,
            department.Courses.Select(c => new AllCoursesResponse
            (
                c.Id,
                c.Code,
                c.Name,
                c.Type.ToString()
            )).ToList()
        );

            return Response.Success("Department details retrieved successfully", data);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, $"An error occurred while fetching department details for departmentID: {id}");
            return Response.SystemMalfunction<DepartmentDetailsResponse>("An error occurred while processing your request. Please try again later.", null!);
        }
    }

    public async Task<ServiceResponse> CreateDepartmentAsync(DepartmentMgmtRequest request, CancellationToken cancellationToken)
    {

        try
        {
            if (request.FacultyId == Guid.Empty)
            {
                return Response.BadRequest("Please select a faculty");
            }

            // Check if department already exists
            bool departmentExists = await database.Departments.AnyAsync(d => d.Name.ToLower() == request.Name.ToLower() && d.FacultyId == request.FacultyId, cancellationToken);

            if (departmentExists)
            {
                return Response.Conflict("A department with this name already exists in this faculty");
            }

            // Check if faculty exists
            bool facultyExists = await database.Faculties.AnyAsync(f => f.Id == request.FacultyId, cancellationToken);

            if (!facultyExists) {
                return Response.NotFound("The selected faculty does not exist");
            }

            Department newDepartment = new Department
            {
                Name = request.Name,
                FacultyId = request.FacultyId,
                CreatedAt = DateTimeOffset.UtcNow
                //TODO - Add CreatedBy content here
            };

            database.Departments.Add(newDepartment);
            await database.SaveChangesAsync(cancellationToken);

            return Response.Created("Department created successfully");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while creating department");
            return Response.SystemMalfunction("An error occurred while processing your request. Please try again later.");
        }
    }

    public async Task<ServiceResponse> UpdateDepartmentAsync(Guid id, DepartmentMgmtRequest request, CancellationToken cancellationToken)
    {
        try
        {
            Department? department = await database.Departments.FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return Response.NotFound("This department does not exist");
            }

            // Check if faculty exists
            bool facultyExists = await database.Faculties.AnyAsync(f => f.Id == request.FacultyId, cancellationToken);

            if (!facultyExists)
            {
                return Response.NotFound("The selected faculty does not exist");
            }

            department.Name = request.Name;
            department.FacultyId = request.FacultyId;
            department.ModifiedAt = DateTimeOffset.UtcNow;
            //TODO - Add ModifiedBy content here

            await database.SaveChangesAsync(cancellationToken);

            return Response.Success("Department updated successfully");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while updating department");
            return Response.SystemMalfunction("An error occurred while processing your request. Please try again later.");
        }
    }

    public async Task<ServiceResponse> DeleteDepartmentAsync(Guid id, CancellationToken cancellationToken)
    {
       try
        {
            Department? department = await database.Departments.FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return Response.NotFound("This department does not exist");
            }

            database.Departments.Remove(department);
            await database.SaveChangesAsync(cancellationToken);

            return Response.Success("Department deleted successfully");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while deleting department");
            return Response.SystemMalfunction("An error occurred while processing your request. Please try again later.");
        }
    }
}
