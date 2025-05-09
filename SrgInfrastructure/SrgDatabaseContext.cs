﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SrgDomain.Model;
using SrgInfrastructure.Models; // for ApplicationUser

namespace SrgInfrastructure
{
    public partial class SrgDatabaseContext : IdentityDbContext<ApplicationUser>
    {
        public SrgDatabaseContext()
        {
        }

        public SrgDatabaseContext(DbContextOptions<SrgDatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Manager> Managers { get; set; }
        public virtual DbSet<Member> Members { get; set; }
        public virtual DbSet<SrgDomain.Model.Task> Tasks { get; set; }
        public virtual DbSet<TaskHistory> TaskHistories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            #warning To protect potentially sensitive information in your connection string, you should move it out of source code.
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-P2UCR0C\\SQLEXPRESS; Database=SRG_Database; Trusted_Connection=True; TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IMPORTANT: call the base method to set up Identity
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Departme__B2079BCD051293C7");
                entity.Property(e => e.Id).HasColumnName("DepartmentID");
                entity.Property(e => e.ContactEmail).HasMaxLength(255);
                entity.Property(e => e.CreatedDate)
                      .HasDefaultValueSql("(getdate())")
                      .HasColumnType("datetime");
                entity.Property(e => e.DepartmentName).HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.PhotoPath)
                      .HasMaxLength(500)
                      .IsUnicode(false)
                      .IsRequired(false);
            });

            modelBuilder.Entity<Manager>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Heads__EB3F22F00A8106E9");
                entity.Property(e => e.Id).HasColumnName("ManagerID");
                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.PhotoPath)
                      .HasMaxLength(500)
                      .IsUnicode(false)
                      .IsRequired(false);

                entity.HasOne(d => d.Department).WithOne(p => p.Manager)
                      .HasForeignKey<Manager>(d => d.DepartmentId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_Managers_Departments");
            });

            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Members__0CF04B387B1871F1");
                entity.Property(e => e.Id).HasColumnName("MemberID");
                entity.Property(e => e.LastTaskDate).HasColumnType("datetime");
                entity.Property(e => e.ManagerId).HasColumnName("ManagerID");
                entity.Property(e => e.Name).HasMaxLength(255);
                entity.Property(e => e.Role).HasMaxLength(100);
                entity.Property(e => e.TasksPerMonth).HasDefaultValue(0);
                entity.Property(e => e.TasksTotal).HasDefaultValue(0);

                entity.Property(e => e.PhotoPath)
                      .HasMaxLength(500)
                      .IsUnicode(false)
                      .IsRequired(false);

                entity.HasOne(d => d.Manager).WithMany(p => p.Members)
                      .HasForeignKey(d => d.ManagerId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_Members_Managers");
            });

            modelBuilder.Entity<SrgDomain.Model.Task>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Tasks__7C6949D12A5A069F");
                entity.ToTable(tb => tb.HasTrigger("trg_TaskComplete"));
                entity.Property(e => e.Id).HasColumnName("TaskID");
                entity.Property(e => e.CreationDate)
                      .HasDefaultValueSql("(getdate())")
                      .HasColumnType("datetime");
                entity.Property(e => e.Deadline).HasColumnType("datetime");
                entity.Property(e => e.ManagerId).HasColumnName("ManagerID");
                entity.Property(e => e.Status)
                      .HasMaxLength(50)
                      .HasDefaultValue("In Progress");
                entity.Property(e => e.Title).HasMaxLength(255);

                entity.HasOne(d => d.Manager).WithMany(p => p.Tasks)
                      .HasForeignKey(d => d.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Tasks_Managers");

                entity.HasMany(d => d.Members).WithMany(p => p.Tasks)
                      .UsingEntity<Dictionary<string, object>>(
                          "TaskExecutor",
                          r => r.HasOne<Member>().WithMany()
                                .HasForeignKey("MemberId")
                                .OnDelete(DeleteBehavior.Cascade)
                                .HasConstraintName("FK_TaskExecutors_Members"),
                          l => l.HasOne<SrgDomain.Model.Task>().WithMany()
                                .HasForeignKey("TaskId")
                                .OnDelete(DeleteBehavior.Cascade)
                                .HasConstraintName("FK_TaskExecutors_Tasks"),
                          j =>
                          {
                              j.HasKey("TaskId", "MemberId").HasName("PK__TaskExec__ECA64D62A91DD690");
                              j.ToTable("TaskExecutors");
                              j.IndexerProperty<int>("TaskId").HasColumnName("TaskID");
                              j.IndexerProperty<int>("MemberId").HasColumnName("MemberID");
                          });
            });

            modelBuilder.Entity<TaskHistory>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__TaskHist__4D7B4ADDE9228EA3");
                entity.ToTable("TaskHistory");
                entity.Property(e => e.Id).HasColumnName("HistoryID");
                entity.Property(e => e.Action).HasMaxLength(50);
                entity.Property(e => e.TaskId).HasColumnName("TaskID");
                entity.Property(e => e.Timestamp)
                      .HasDefaultValueSql("(getdate())")
                      .HasColumnType("datetime");

                entity.HasOne(d => d.Task).WithMany(p => p.TaskHistories)
                      .HasForeignKey(d => d.TaskId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .HasConstraintName("FK__TaskHisto__TaskI__03F0984C");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
        public DbSet<SrgInfrastructure.ViewModels.AdminUserViewModel> AdminUserViewModel { get; set; } = default!;
    }
}
