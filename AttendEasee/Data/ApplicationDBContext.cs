using AttendEase.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
namespace AttendEase.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Manager)
                .WithMany(m => m.Subordinates)
                .HasForeignKey(u => u.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Designation)
                .WithMany(m => m.Users)
                .HasForeignKey(u => u.DesignationId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Project)
                .WithMany(m => m.Users)
                .HasForeignKey(u => u.ProjectId);

            modelBuilder.Entity<Leave>()
                .HasOne(l => l.User)
                .WithMany(m => m.Leaves)
                .HasForeignKey(l => l.UserId);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.User)
                .WithMany(u => u.Attendances)
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<Project>()
            .Property(p => p.ProjectId)
            .ValueGeneratedNever();

            modelBuilder.Entity<Designation>()
            .Property(p => p.DesignationId)
            .ValueGeneratedNever();
            
           
        }
    }
}