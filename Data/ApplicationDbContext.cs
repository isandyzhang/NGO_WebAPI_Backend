using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models;

namespace NGO_WebAPI_Backend.Data
{
    /// <summary>
    /// 应用程序数据库上下文
    /// 这是Entity Framework Core的核心类，负责与数据库的交互
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// 构造函数 - 接收数据库配置选项
        /// </summary>
        /// <param name="options">数据库连接选项</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// 工作人員資料表
        /// </summary>
        public DbSet<Worker> Workers { get; set; }

        /// <summary>
        /// 個案資料表
        /// </summary>
        public DbSet<Case> Cases { get; set; }

        /// <summary>
        /// 使用者資料表
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// 活動資料表
        /// </summary>
        public DbSet<Activity> Activities { get; set; }
        
        /// <summary>
        /// 個案活動登記表
        /// </summary>
        public DbSet<CaseActivityRegistration> CaseActivityRegistrations { get; set; }

        /// <summary>
        /// 用戶活動登記表
        /// </summary>
        public DbSet<UserActivityRegistration> UserActivityRegistrations { get; set; }
        
        /// <summary>
        /// 配置数据库模型
        /// </summary>
        /// <param name="modelBuilder">模型构建器</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // 配置Worker實體
            modelBuilder.Entity<Worker>(entity =>
            {
                // 設定主鍵
                entity.HasKey(w => w.WorkerId);
                
                // 設定Email為唯一
                entity.HasIndex(w => w.Email).IsUnique();
                
                // 設定欄位長度限制
                entity.Property(w => w.Email).HasMaxLength(100);
                entity.Property(w => w.Password).HasMaxLength(255);
                entity.Property(w => w.Name).HasMaxLength(50);
            });

            // 配置Case實體
            modelBuilder.Entity<Case>(entity =>
            {
                // 設定主鍵
                entity.HasKey(c => c.CaseId);
                
                // 設定身分證字號為唯一
                entity.HasIndex(c => c.IdentityNumber).IsUnique();
                
                // 設定欄位長度限制
                entity.Property(c => c.Name).HasMaxLength(50);
                entity.Property(c => c.Phone).HasMaxLength(20);
                entity.Property(c => c.IdentityNumber).HasMaxLength(10);
                entity.Property(c => c.Address).HasMaxLength(200);
                entity.Property(c => c.Description).HasMaxLength(500);
                entity.Property(c => c.Status).HasMaxLength(20);
                entity.Property(c => c.Email).HasMaxLength(100);
                entity.Property(c => c.Gender).HasMaxLength(10);
                entity.Property(c => c.ProfileImage).HasMaxLength(1000);
                entity.Property(c => c.City).HasMaxLength(50);
                entity.Property(c => c.District).HasMaxLength(50);
                entity.Property(c => c.DetailAddress).HasMaxLength(200);
                
                // 設定與Worker的關聯
                entity.HasOne(c => c.Worker)
                      .WithMany()
                      .HasForeignKey(c => c.WorkerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置Activity實體
            modelBuilder.Entity<Activity>(entity =>
            {
                // 設定主鍵
                entity.HasKey(a => a.ActivityId);
                
                // 設定欄位長度限制
                entity.Property(a => a.ActivityName).HasMaxLength(200);
                entity.Property(a => a.Description).HasMaxLength(1000);
                entity.Property(a => a.ImageUrl).HasMaxLength(500);
                entity.Property(a => a.Location).HasMaxLength(200);
                entity.Property(a => a.TargetAudience).HasMaxLength(100);
                entity.Property(a => a.Status).HasMaxLength(50);
                
                // 設定與Worker的關聯
                entity.HasOne(a => a.Worker)
                      .WithMany()
                      .HasForeignKey(a => a.WorkerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置CaseActivityRegistration實體
            modelBuilder.Entity<CaseActivityRegistration>(entity =>
            {
                // 設定主鍵
                entity.HasKey(r => r.Id);
                
                // 設定與Case的關聯
                entity.HasOne(r => r.Case)
                      .WithMany(c => c.CaseActivityRegistrations)
                      .HasForeignKey(r => r.CaseId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // 設定與Activity的關聯
                entity.HasOne(r => r.Activity)
                      .WithMany(a => a.CaseActivityRegistrations)
                      .HasForeignKey(r => r.ActivityId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置UserActivityRegistration實體
            modelBuilder.Entity<UserActivityRegistration>(entity =>
            {
                // 設定主鍵
                entity.HasKey(r => r.Id);
                
                // 設定與Activity的關聯
                entity.HasOne(r => r.Activity)
                      .WithMany(a => a.UserActivityRegistrations)
                      .HasForeignKey(r => r.ActivityId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // 設定與User的關聯
                entity.HasOne(r => r.User)
                      .WithMany(u => u.UserActivityRegistrations)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
} 