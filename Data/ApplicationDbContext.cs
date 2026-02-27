using Microsoft.EntityFrameworkCore;
using HizliOgren.Models;

namespace HizliOgren.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CurriculumItem> CurriculumItems => Set<CurriculumItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<UserCourse> UserCourses => Set<UserCourse>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<HeroSlide> HeroSlides => Set<HeroSlide>();
    public DbSet<InstructorApplication> InstructorApplications => Set<InstructorApplication>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<UserCourse>()
            .HasIndex(uc => new { uc.UserId, uc.CourseId })
            .IsUnique();

        builder.Entity<UserCourse>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.PurchasedCourses)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserCourse>()
            .HasOne(uc => uc.Course)
            .WithMany(c => c.UserCourses)
            .HasForeignKey(uc => uc.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CurriculumItem>()
            .HasIndex(c => new { c.CourseId, c.SortOrder });

        // SQL Server: avoid multiple cascade paths (Users -> Orders -> OrderItems)
        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Course)
            .WithMany(c => c.OrderItems)
            .HasForeignKey(oi => oi.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<InstructorApplication>()
            .HasOne(ia => ia.User)
            .WithMany()
            .HasForeignKey(ia => ia.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
