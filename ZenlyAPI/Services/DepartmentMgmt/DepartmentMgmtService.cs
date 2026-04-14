using Microsoft.EntityFrameworkCore;
using ZenlyAPI.Context;
using ZenlyAPI.Domain.Entities;
using ZenlyAPI.Domain.Utilities;

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

        throw new NotImplementedException();
    }
}
