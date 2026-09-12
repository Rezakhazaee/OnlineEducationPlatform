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


    public DbSet<OrganizationSettings> OrganizationSettings { get; set; }
    public DbSet<PartnerOrganization> PartnerOrganizations { get; set; }
    public DbSet<CoursePartnerOrganization> CoursePartnerOrganizations { get; set; }

    public DbSet<Enrollment> Enrollments { get; set; }

    public DbSet<Payment> Payments { get; set; }

    public DbSet<StudentFollowUp> StudentFollowUps { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // User -> Student
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<Student>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Student -> Marketing User
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<Student>()
            .HasOne(s => s.MarketingUser)
            .WithMany()
            .HasForeignKey(s => s.MarketingUserId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Student -> Created By User
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<Student>()
            .HasOne(s => s.CreatedByUser)
            .WithMany()
            .HasForeignKey(s => s.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Student -> Support User
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<Student>()
            .HasOne(s => s.SupportUser)
            .WithMany()
            .HasForeignKey(s => s.SupportUserId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Course -> Instructor
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<Course>()
            .HasOne(c => c.Instructor)
            .WithMany()
            .HasForeignKey(c => c.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> Student
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany()
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> Support User
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.SupportUser)
            .WithMany()
            .HasForeignKey(e => e.SupportUserId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> Instructor User
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Instructor)
            .WithMany()
            .HasForeignKey(e => e.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // StudentFollowUp -> Student
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<StudentFollowUp>()
            .HasOne(f => f.Student)
            .WithMany()
            .HasForeignKey(f => f.StudentId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // StudentFollowUp -> Support User
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<StudentFollowUp>()
            .HasOne(f => f.SupportUser)
            .WithMany()
            .HasForeignKey(f => f.SupportUserId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Payment -> Enrollment
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Enrollment)
            .WithMany()
            .HasForeignKey(p => p.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Organization Settings
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Course
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.Course)
            .WithMany()
            .HasForeignKey(cpo => cpo.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // CoursePartnerOrganization -> Partner Organization
        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================

        modelBuilder.Entity<CoursePartnerOrganization>()
            .HasOne(cpo => cpo.PartnerOrganization)
            .WithMany()
            .HasForeignKey(cpo => cpo.PartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        // Enrollment -> CoursePartnerOrganization
        // =========================

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CoursePartnerOrganization)
            .WithMany()
            .HasForeignKey(e => e.CoursePartnerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);


        // =========================
        //
        // هر نصب آموزش‌یار فقط یک تنظیمات سازمانی دارد.
        // در این مرحله محدودیت تک‌رکوردی را در دیتابیس
        // اعمال نمی‌کنیم؛ بعداً Migration آن را مدیریت می‌کند.
        //

        modelBuilder.Entity<OrganizationSettings>()
            .Property(s => s.Name)
            .IsRequired();

        modelBuilder.Entity<OrganizationSettings>()
            .Property(s => s.PrimaryColor)
            .HasDefaultValue("#568fa8");

        modelBuilder.Entity<OrganizationSettings>()
            .Property(s => s.SecondaryColor)
            .HasDefaultValue("#263d4a");

        modelBuilder.Entity<OrganizationSettings>()
            .Property(s => s.IsActive)
            .HasDefaultValue(true);

        modelBuilder.Entity<OrganizationSettings>()
            .Property(s => s.ThemeIsActive)
            .HasDefaultValue(true);
    }
}