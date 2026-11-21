using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Timekeeping.Api.Models;

namespace Timekeeping.Api.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<TaskItem> Tasks { get; set; }

    public virtual DbSet<TimeEntry> TimeEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasIndex(e => e.Code, "UQ_Projects_Code").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasIndex(e => e.ProjectId, "IX_Tasks_ProjectId");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.Project).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.HasIndex(e => e.Date, "IX_TimeEntries_Date");

            entity.HasIndex(e => e.TaskId, "IX_TimeEntries_TaskId");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.Hours).HasPrecision(4, 2);

            entity.HasOne(d => d.Task).WithMany(p => p.TimeEntries)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
