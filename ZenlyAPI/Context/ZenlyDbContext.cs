using Microsoft.EntityFrameworkCore;
using ZenlyAPI.Domain.Config;
using ZenlyAPI.Domain.Entities;
using ZenlyAPI.Domain.Entities.Complaints;
using ZenlyAPI.Domain.Entities.Shared;
using ZenlyAPI.Domain.User;
using ZenlyAPI.Domain.User.Lecturers;
using ZenlyAPI.Domain.User.Students;

namespace ZenlyAPI.Context;

public class ZenlyDbContext(DbContextOptions<ZenlyDbContext> options, ZenlyConfig config)
: DbContext(options)
{
    public DbSet<Course> Courses { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Faculty> Faculties { get; set; }
    public DbSet<Complaint> Complaints { get; set; }    
    public DbSet<ComplaintsTrail> ComplaintsTrail { get; set; }
    public DbSet<ComplaintUpload> ComplaintUploads { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<StudentCourse> Students_Courses { get; set; }
    public DbSet<Lecturer> Lecturers { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Student>().ToTable("Students");
        //modelBuilder.Entity<Lecturer>().ToTable("Lecturers");

        modelBuilder.Entity<Department>()
            .HasMany(v => v.Courses)
            .WithOne(p => p.Department)
            .HasForeignKey(p => p.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Department>()
            .HasOne(d => d.Faculty)
            .WithMany(f => f.Departments)
            .HasForeignKey(d => d.FacultyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

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

        modelBuilder.Entity<Complaint>()
            .HasMany(u => u.Documents)
            .WithOne(h => h.Complaint)
            .HasForeignKey(h => h.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ComplaintsTrail>()
            .Property(x => x.ActionType)
            .HasDefaultValue(ComplaintActionType.Other);

        modelBuilder.Entity<StudentCourse>()
            .HasOne(sc => sc.Student)
            .WithMany(s => s.StudentCourses)
            .HasForeignKey(sc => sc.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StudentCourse>()
            .HasOne(sc => sc.Course)
            .WithMany(c => c.StudentCourses)
            .HasForeignKey(sc => sc.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RefreshToken>()
     .HasOne(rt => rt.Student)
     .WithMany(s => s.RefreshTokens)
     .HasForeignKey(rt => rt.StudentId)
     .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.Lecturer)
            .WithMany(l => l.RefreshTokens)
            .HasForeignKey(rt => rt.LecturerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Student>()
             .HasOne(u => u.Department)
             .WithMany()
             .HasForeignKey(u => u.DepartmentId)
             .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Student>()
            .HasOne(u => u.Faculty)
            .WithMany()
            .HasForeignKey(u => u.FacultyId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Lecturer>()
          .HasOne(u => u.Department)
          .WithMany()
          .HasForeignKey(u => u.DepartmentId)
          .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Lecturer>()
            .HasOne(u => u.Faculty)
            .WithMany()
            .HasForeignKey(u => u.FacultyId)
            .OnDelete(DeleteBehavior.NoAction);
    }

}
