using Microsoft.EntityFrameworkCore;
using System.Numerics;
using ZenlyAPI.Domain.Config;
using ZenlyAPI.Domain.Entities;
using ZenlyAPI.Domain.Entities.Complaints;
using ZenlyAPI.Domain.Entities.Shared;

namespace ZenlyAPI.Context;

public class ZenlyDbContext(DbContextOptions<ZenlyDbContext> options, ZenlyConfig config)
: DbContext(options)
{
    public DbSet<Course> Courses { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Faculty> Faculties { get; set; }
    public DbSet<Complaint> Complaints { get; set; }    
    public DbSet<ComplaintsTrail> ComplaintsTrail { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Department>()
            .HasMany(v => v.Courses)
            .WithOne(p => p.Department)
            .HasForeignKey(p => p.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Faculty>()
            .HasMany(f => f.Departments)
            .WithOne(d => d.Faculty)
            .HasForeignKey(d => d.FacultyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Complaint>()
            .HasMany(c => c.History)
            .WithOne(h => h.Complaint)
            .HasForeignKey(h => h.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ComplaintsTrail>()
            .Property(x => x.ActionType)
            .HasDefaultValue(ComplaintActionType.Other);
    }

}
