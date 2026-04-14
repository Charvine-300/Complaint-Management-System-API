using Microsoft.EntityFrameworkCore;
using ZenlyAPI.Context;
using ZenlyAPI.Domain.Entities;
using ZenlyAPI.Domain.Utilities;
using ZenlyAPI.Services.Shared;


namespace ZenlyAPI.Services.CourseMgmt;

public class CourseMgmtService(ZenlyDbContext database) : ICourseMgmtService
{
    public async Task<ServiceResponse<PaginationResponse<AllCoursesResponse>>> GetCoursesAsync(CourseParameters parameters, CancellationToken cancellationToken)
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
                c.Type.ToString()
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
}
