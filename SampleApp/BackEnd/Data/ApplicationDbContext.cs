using Microsoft.EntityFrameworkCore;
using BackEnd.Models;

namespace BackEnd.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Student> Students { get; set; }

    public DbSet<Course> Courses { get; set; }

    public DbSet<Organization> Organizations { get; set; }

    public DbSet<Enrollment> Enrollments { get; set; }

    public DbSet<Payment> Payments { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // =========================
        // Student -> Organization
        // =========================

        modelBuilder.Entity<Student>()
            .HasOne(s => s.Organization)
            .WithMany()
            .HasForeignKey(s => s.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Student -> Marketing User
        // =========================

        modelBuilder.Entity<Student>()
            .HasOne(s => s.MarketingUser)
            .WithMany()
            .HasForeignKey(s => s.MarketingUserId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Student -> Created By User
        // =========================

        modelBuilder.Entity<Student>()
            .HasOne(s => s.CreatedByUser)
            .WithMany()
            .HasForeignKey(s => s.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Student -> Support User
        // =========================

        modelBuilder.Entity<Student>()
            .HasOne(s => s.SupportUser)
            .WithMany()
            .HasForeignKey(s => s.SupportUserId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Course -> Instructor
        // =========================

        modelBuilder.Entity<Course>()
            .HasOne(c => c.Instructor)
            .WithMany()
            .HasForeignKey(c => c.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> Student
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany()
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> Course
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> Support User
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.SupportUser)
            .WithMany()
            .HasForeignKey(e => e.SupportUserId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> Instructor User
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Instructor)
            .WithMany()
            .HasForeignKey(e => e.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Payment -> Enrollment
        // =========================

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Enrollment)
            .WithMany()
            .HasForeignKey(p => p.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}