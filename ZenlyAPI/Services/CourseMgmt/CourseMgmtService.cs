using Microsoft.EntityFrameworkCore;
using ZenlyAPI.Context;
using ZenlyAPI.Domain.Entities;
using ZenlyAPI.Domain.Utilities;
using ZenlyAPI.Services.Shared;
using Serilog;


namespace ZenlyAPI.Services.CourseMgmt;

public class CourseMgmtService(ZenlyDbContext database) : ICourseMgmtService
{
    public async Task<ServiceResponse<PaginationResponse<AllCoursesResponse>>> GetCoursesAsync(CourseParameters parameters, CancellationToken cancellationToken)
    {
        try
        {
            IQueryable<Course> query = database.Courses.AsNoTracking();

            if(parameters.DepartmentId.HasValue)
            {
                query = query.Where(c => c.DepartmentId == parameters.DepartmentId);
            }

            if (!string.IsNullOrEmpty(parameters.Search))
            {
                query = query.Where(c => c.Name.ToLower().Contains(parameters.Search.ToLower())
                          || c.Code.ToLower().Contains(parameters.Search.ToLower()));
            }

            if (parameters.Type is not null)
            {
                {
                    query = query.Where(c => c.Type == parameters.Type);
                }
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

                var courses = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToListAsync(cancellationToken);

                List<AllCoursesResponse> data = courses.Select(c => new AllCoursesResponse
                (
                    c.Id,
                    c.Name,
                    c.Code,
                    c.Type.ToString(),
                    c.IsActive
                )).ToList();


                return Response.Success("Courses retrieved successfully", new PaginationResponse<AllCoursesResponse>
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
            Log.Logger.Error(ex, "An error occurred while fetching courses");
            return Response.SystemMalfunction<PaginationResponse<AllCoursesResponse>>("An error occurred while processing your request. Please try again later.", null!);
        }
    }

    public async Task<ServiceResponse<CourseDetailsResponse>> GetCourseDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            Course? course = await database.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (course == null)
            {
                return Response.NotFound<CourseDetailsResponse>("This course does not exist", null!);
            }

            CourseDetailsResponse courseDetails = new CourseDetailsResponse
            (
                Id: course.Id,
                Name: course.Name,
                Code: course.Code,
                Type: course.Type.ToString(),
                IsActive: course.IsActive,
                Department: course.Department?.Name ?? "N/A",
                Faculty: course.Department?.Faculty?.Name ?? "N/A"
            );

            return Response.Success("Course details retrieved successfully", courseDetails);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while fetching course details");
            return Response.SystemMalfunction<CourseDetailsResponse>("An error occurred while processing your request. Please try again later.", null!);
        }
    }

    public async Task<ServiceResponse> CreateCourseAsync(CourseMgmtRequest request, CancellationToken cancellationToken)
    {
        try
        {
            bool departmentExists = await database.Departments.AsNoTracking().AnyAsync(c => c.Id == request.DepartmentId, cancellationToken);

            if (!departmentExists)
            {
                return Response.NotFound("This department does not exist");
            }

            // Check for duplicate course code and name within the same department
            bool duplicateCode = await database.Courses.AsNoTracking().AnyAsync(c => c.Code.ToLower() == request.Code.ToLower() && c.DepartmentId == request.DepartmentId, cancellationToken);

            if (duplicateCode)
            {
                return Response.Conflict("A course with the same code already exists in this department");
            }

            bool duplicateName = await database.Courses.AsNoTracking().AnyAsync(c => c.Name.ToLower() == request.Name.ToLower() && c.DepartmentId == request.DepartmentId, cancellationToken);

            if (duplicateName)
            {
                return Response.Conflict("A course with the same name already exists in this department");
            }


            Course newCourse = new Course
            {
                Name = request.Name,
                Code = request.Code,
                Type = request.Type,
                DepartmentId = request.DepartmentId,
                CreatedAt = DateTimeOffset.UtcNow,
                //TODO - Add CreatedBy content here
            };


            await database.Courses.AddAsync(newCourse);
            database.SaveChangesAsync(cancellationToken);

            return Response.Created("Course created successfully");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while creating courses");
            return Response.SystemMalfunction("An error occurred while processing your request. Please try again later.");
        }
    }

    public async Task<ServiceResponse> UpdateCourseAsync(Guid id, CourseMgmtRequest request, CancellationToken cancellationToken)
    {
        try
        {
            Course? course = await database.Courses.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (course == null)
            {
                return Response.NotFound("This course does not exist");
            }

            bool departmentExists = await database.Departments.AsNoTracking().AnyAsync(c => c.Id == request.DepartmentId, cancellationToken);

            if (!departmentExists)
            {
                return Response.NotFound("This department does not exist");
            }

            // Check for duplicate course code and name within the same department
            bool duplicateCode = await database.Courses.AsNoTracking().AnyAsync(c => c.Code.ToLower() == request.Code.ToLower() && c.DepartmentId == request.DepartmentId && c.Id != course.Id, cancellationToken);

            if (duplicateCode)
            {
                return Response.Conflict("A course with the same code already exists in this department");
            }

            bool duplicateName = await database.Courses.AsNoTracking().AnyAsync(c => c.Name.ToLower() == request.Name.ToLower() && c.DepartmentId == request.DepartmentId && c.Id != course.Id, cancellationToken);

            if (duplicateName)
            {
                return Response.Conflict("A course with the same name already exists in this department");
            }


            course.Name = request.Name;
            course.DepartmentId = request.DepartmentId;
            course.Code = request.Code;
            course.Type = request.Type;
            course.ModifiedAt = DateTimeOffset.UtcNow;
            //TODO - Add ModifiedBy content here

            await database.SaveChangesAsync(cancellationToken);

            return Response.Success("Course updated successfully");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while updating courses");
            return Response.SystemMalfunction("An error occurred while processing your request. Please try again later.");
        }
    }

    public async Task<ServiceResponse> ChangeCourseTypeAsync(Guid id, ChangeCourseTypeRequest request, CancellationToken cancellationToken)
    {
        Course? course = await database.Courses.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (course == null)
        {
            return Response.NotFound("This course does not exist");
        }

        course.Type = request.Type;
        course.ModifiedAt = DateTimeOffset.UtcNow;
        //TODO - Add ModifiedBy content here
    
        await database.SaveChangesAsync(cancellationToken);
    
        return Response.Success("Course type updated successfully");
    }

    public async Task<ServiceResponse> DeleteCourseAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            Course? course = await database.Courses.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (course == null)
            {
                return Response.NotFound("This course does not exist");
            }

            database.Courses.Remove(course);
            await database.SaveChangesAsync(cancellationToken);

            return Response.Success("Course deleted successfully");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while deleting courses");
            return Response.SystemMalfunction("An error occurred while processing your request. Please try again later.");
        }
    }
}
