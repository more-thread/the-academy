using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Security.Permissions;
using TRS.Models;

namespace TRS.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options)
            : base(options)
        {
        }
        public DbSet<TrainingProgram> mTrainingProgram {get;set;}
        public DbSet<TrainingCourse> mTrainingCourse {get;set;}
        public DbSet<JobClass> mJobClass {get;set;}
        public DbSet<TrainingSchedule> tTrainingSchedule {get;set;}        
        public DbSet<TrainingRegistration> tTrainingRegistration {get;set;}
        public DbSet<UserLogs> tUserLogs {get;set;}
        public DbSet<TrainingCoordinator> mTrainingCoordinator {get;set;}
        public DbSet<TrainingFeedback> tTrainingFeedback {get;set;}
        public DbSet<TrainingFeedbackQuestions> mTrainingFeedbackQuestions {get;set;}
        public virtual DbSet<VwHrEmployeeInfo> VwHrEmployeeInfos { get; set; }
        public virtual DbSet<VwHrRegion> VwHrRegions { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=devCon",o => o.CommandTimeout(300)); //5minutes
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FormAccess>(entity =>
            {
                entity.HasNoKey();
            });
            
            modelBuilder.Entity<UserInfo>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<VwHrRegion>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vw_HR_Regions", "TRS");

                entity.Property(e => e.Branch)
                    .IsRequired()
                    .HasMaxLength(4)
                    .IsUnicode(false);
                entity.Property(e => e.Region)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);
            });
            
            modelBuilder.Entity<VwHrEmployeeInfo>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vw_HR_EmployeeInfo", "TRS");

                entity.Property(e => e.Branch)
                    .HasMaxLength(20)
                    .IsUnicode(false);
                entity.Property(e => e.DepartmentHeadFullname).HasMaxLength(255);
                entity.Property(e => e.DepartmentHeadId).HasColumnName("DepartmentHeadID");
                entity.Property(e => e.DepartmentId)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("DepartmentID");
                entity.Property(e => e.DepartmentName)
                    .HasMaxLength(500)
                    .IsUnicode(false);
                entity.Property(e => e.DivisionHeadFullname)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(e => e.DivisionHeadId).HasColumnName("DivisionHeadID");
                entity.Property(e => e.DivisionId)
                    .HasMaxLength(50)
                    .HasColumnName("DivisionID");
                entity.Property(e => e.DivisionName).HasMaxLength(50);
                entity.Property(e => e.EmailAddress)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(e => e.EmployeeLevel)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.EmployeeName).HasMaxLength(60);
                entity.Property(e => e.EmployeeService)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.EmployeeStatus)
                    .HasMaxLength(1)
                    .IsUnicode(false);
                entity.Property(e => e.EmployeeType)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.EmployeeTypeDesc).HasMaxLength(60);
                entity.Property(e => e.FirstName).HasMaxLength(60);
                entity.Property(e => e.JobClassCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.JobClassDescription).HasMaxLength(60);
                entity.Property(e => e.LastName).HasMaxLength(60);
                entity.Property(e => e.MiddleName).HasMaxLength(60);
                entity.Property(e => e.PersonalPhoneNo)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);
                entity.Property(e => e.PositionCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.PositionName)
                    .HasMaxLength(500)
                    .IsUnicode(false);
                entity.Property(e => e.Satellite)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.SectionHeadFullname).HasMaxLength(255);
                entity.Property(e => e.SectionHeadId).HasColumnName("SectionHeadID");
                entity.Property(e => e.SectionId)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("SectionID");
                entity.Property(e => e.SectionName)
                    .HasMaxLength(500)
                    .IsUnicode(false);
                entity.Property(e => e.ServiceDescription).HasMaxLength(60);
                entity.Property(e => e.SuperiorFullname).HasMaxLength(255);
                entity.Property(e => e.SuperiorId).HasColumnName("SuperiorID");
            });
        }

    }    

}
