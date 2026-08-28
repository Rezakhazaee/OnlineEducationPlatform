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


        // Enrollment -> Student
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany()
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);


        // Enrollment -> Course
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // Enrollment -> Support User
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.SupportUser)
            .WithMany()
            .HasForeignKey(e => e.SupportUserId)
            .OnDelete(DeleteBehavior.Restrict);


        // Enrollment -> Instructor User
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Instructor)
            .WithMany()
            .HasForeignKey(e => e.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}