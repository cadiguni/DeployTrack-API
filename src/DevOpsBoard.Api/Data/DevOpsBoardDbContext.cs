using DevOpsBoard.Api.Models;
using Microsoft.EntityFrameworkCore;
using Environment = DevOpsBoard.Api.Models.Environment;

namespace DevOpsBoard.Api.Data;

public sealed class DevOpsBoardDbContext(DbContextOptions<DevOpsBoardDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<Environment> Environments => Set<Environment>();
    public DbSet<Deployment> Deployments => Set<Deployment>();
    public DbSet<ApplicationHealthCheck> HealthChecks => Set<ApplicationHealthCheck>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Email).HasMaxLength(256);
            entity.Property(user => user.Name).HasMaxLength(120);
            entity.Property(user => user.PasswordHash).HasMaxLength(512);
            entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(32);
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasIndex(application => application.Name).IsUnique();
            entity.Property(application => application.Name).HasMaxLength(120);
            entity.Property(application => application.Description).HasMaxLength(500);
            entity.Property(application => application.RepositoryUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<Environment>(entity =>
        {
            entity.HasIndex(environment => environment.Name).IsUnique();
            entity.Property(environment => environment.Name).HasMaxLength(64);
            entity.HasData(
                new Environment { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "dev" },
                new Environment { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "staging" },
                new Environment { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "production" });
        });

        modelBuilder.Entity<Deployment>(entity =>
        {
            entity.Property(deployment => deployment.Version).HasMaxLength(80);
            entity.Property(deployment => deployment.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(deployment => deployment.DeployedBy).HasMaxLength(160);
            entity.Property(deployment => deployment.CommitSha).HasMaxLength(80);
            entity.Property(deployment => deployment.PipelineUrl).HasMaxLength(700);

            entity.HasOne(deployment => deployment.Application)
                .WithMany(application => application.Deployments)
                .HasForeignKey(deployment => deployment.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(deployment => deployment.Environment)
                .WithMany(environment => environment.Deployments)
                .HasForeignKey(deployment => deployment.EnvironmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ApplicationHealthCheck>(entity =>
        {
            entity.Property(healthCheck => healthCheck.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(healthCheck => healthCheck.Details).HasMaxLength(1000);
            entity.Property(healthCheck => healthCheck.CheckedBy).HasMaxLength(160);

            entity.HasOne(healthCheck => healthCheck.Application)
                .WithMany(application => application.HealthChecks)
                .HasForeignKey(healthCheck => healthCheck.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(healthCheck => healthCheck.Environment)
                .WithMany(environment => environment.HealthChecks)
                .HasForeignKey(healthCheck => healthCheck.EnvironmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
