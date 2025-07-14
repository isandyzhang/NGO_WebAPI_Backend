using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NGO_WebAPI_Backend.Models;

public partial class NgoplatformDbContext : DbContext
{
    public NgoplatformDbContext()
    {
    }

    public NgoplatformDbContext(DbContextOptions<NgoplatformDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<Case> Cases { get; set; }

    public virtual DbSet<CaseActivityRegistration> CaseActivityRegistrations { get; set; }

    public virtual DbSet<CaseLogin> CaseLogins { get; set; }

    public virtual DbSet<CaseOrder> CaseOrders { get; set; }

    public virtual DbSet<EmergencySupplyMatch> EmergencySupplyMatches { get; set; }

    public virtual DbSet<EmergencySupplyNeed> EmergencySupplyNeeds { get; set; }

    public virtual DbSet<News> News { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<RegularSuppliesNeed> RegularSuppliesNeeds { get; set; }

    public virtual DbSet<RegularSupplyMatch> RegularSupplyMatches { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<Supply> Supplies { get; set; }

    public virtual DbSet<SupplyCategory> SupplyCategories { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserActivityRegistration> UserActivityRegistrations { get; set; }

    public virtual DbSet<UserOrder> UserOrders { get; set; }

    public virtual DbSet<UserOrderDetail> UserOrderDetails { get; set; }

    public virtual DbSet<VwActivitiesFrontend> VwActivitiesFrontends { get; set; }

    public virtual DbSet<VwActivitiesStatus> VwActivitiesStatuses { get; set; }

    public virtual DbSet<Worker> Workers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=ngosqlserver.database.windows.net;Database=NGOPlatformDB;User Id=ngoadmin;Password=r18031234!;TrustServerCertificate=True;Encrypt=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Chinese_Taiwan_Stroke_CI_AS");

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("PK__Activiti__45F4A7914643EE44");

            entity.ToTable(tb => tb.HasTrigger("tr_CheckFullOnRegistration"));

            entity.Property(e => e.ActivityName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Category).HasMaxLength(10);
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("open");
            entity.Property(e => e.TargetAudience)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Worker).WithMany(p => p.Activities)
                .HasForeignKey(d => d.WorkerId)
                .HasConstraintName("FK__Activitie__Worke__03F0984C");
        });

        modelBuilder.Entity<Case>(entity =>
        {
            entity.HasKey(e => e.CaseId).HasName("PK__Cases__6CAE524C4841F794");

            entity.HasIndex(e => e.IdentityNumber, "UQ__Cases__6354A73F80F4F619").IsUnique();


            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.DetailAddress).HasMaxLength(200);
            entity.Property(e => e.District).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.IdentityNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Worker).WithMany(p => p.Cases)
                .HasForeignKey(d => d.WorkerId)
                .HasConstraintName("FK__Cases__WorkerId__09A971A2");
        });

        modelBuilder.Entity<CaseActivityRegistration>(entity =>
        {
            entity.HasKey(e => e.RegistrationId).HasName("PK__CaseActi__6EF58810AFB8FC95");

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Activity).WithMany(p => p.CaseActivityRegistrations)
                .HasForeignKey(d => d.ActivityId)
                .HasConstraintName("FK__CaseActiv__Activ__05D8E0BE");

            entity.HasOne(d => d.Case).WithMany(p => p.CaseActivityRegistrations)
                .HasForeignKey(d => d.CaseId)
                .HasConstraintName("FK__CaseActiv__CaseI__04E4BC85");
        });

        modelBuilder.Entity<CaseLogin>(entity =>
        {
            entity.HasKey(e => e.CaseId).HasName("PK__CaseLogi__6CAE524CB0B15E21");

            entity.HasIndex(e => e.Email, "UQ__CaseLogi__A9D105346FD037AD").IsUnique();

            entity.Property(e => e.CaseId).ValueGeneratedNever();
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Case).WithOne(p => p.CaseLogin)
                .HasForeignKey<CaseLogin>(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CaseLogin__CaseI__06CD04F7");
        });

        modelBuilder.Entity<CaseOrder>(entity =>
        {
            entity.HasKey(e => e.CaseOrderId).HasName("PK__CaseOrde__4CFB6563687E973E");

            entity.Property(e => e.OrderTime).HasColumnType("datetime");

            entity.HasOne(d => d.Case).WithMany(p => p.CaseOrders)
                .HasForeignKey(d => d.CaseId)
                .HasConstraintName("FK__CaseOrder__CaseI__07C12930");

            entity.HasOne(d => d.Supply).WithMany(p => p.CaseOrders)
                .HasForeignKey(d => d.SupplyId)
                .HasConstraintName("FK__CaseOrder__Suppl__08B54D69");
        });

        modelBuilder.Entity<EmergencySupplyMatch>(entity =>
        {
            entity.HasKey(e => e.EmergencyMatchId).HasName("PK__Emergenc__9E89AB71B11CAC01");

            entity.Property(e => e.MatchDate).HasColumnType("datetime");
            entity.Property(e => e.Note).HasColumnType("text");

            entity.HasOne(d => d.EmergencyNeed).WithMany(p => p.EmergencySupplyMatches)
                .HasForeignKey(d => d.EmergencyNeedId)
                .HasConstraintName("FK__Emergency__Emerg__0A9D95DB");

            entity.HasOne(d => d.MatchedByWorker).WithMany(p => p.EmergencySupplyMatches)
                .HasForeignKey(d => d.MatchedByWorkerId)
                .HasConstraintName("FK__Emergency__Match__0B91BA14");
        });

        modelBuilder.Entity<EmergencySupplyNeed>(entity =>
        {
            entity.HasKey(e => e.EmergencyNeedId).HasName("PK__Emergenc__D9A4C6FA1D1F8990");

            entity.Property(e => e.PickupDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VisitDate).HasColumnType("datetime");

            entity.HasOne(d => d.Case).WithMany(p => p.EmergencySupplyNeeds)
                .HasForeignKey(d => d.CaseId)
                .HasConstraintName("FK__Emergency__CaseI__0C85DE4D");

            entity.HasOne(d => d.Supply).WithMany(p => p.EmergencySupplyNeeds)
                .HasForeignKey(d => d.SupplyId)
                .HasConstraintName("FK__Emergency__Suppl__0D7A0286");

            entity.HasOne(d => d.Worker).WithMany(p => p.EmergencySupplyNeeds)
                .HasForeignKey(d => d.WorkerId)
                .HasConstraintName("FK__Emergency__Worke__0E6E26BF");
        });

        modelBuilder.Entity<News>(entity =>
        {
            entity.HasKey(e => e.NewsId).HasName("PK__News__954EBDF308AA3080");

            entity.Property(e => e.Content).HasColumnType("text");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Token).HasMaxLength(255);
            entity.Property(e => e.UserType).HasMaxLength(10);
        });

        modelBuilder.Entity<RegularSuppliesNeed>(entity =>
        {
            entity.HasKey(e => e.RegularNeedId).HasName("PK__RegularS__12A5D64A7B60206E");

            entity.Property(e => e.ApplyDate).HasColumnType("datetime");
            entity.Property(e => e.PickupDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Case).WithMany(p => p.RegularSuppliesNeeds)
                .HasForeignKey(d => d.CaseId)
                .HasConstraintName("FK__RegularSu__CaseI__0F624AF8");

            entity.HasOne(d => d.Supply).WithMany(p => p.RegularSuppliesNeeds)
                .HasForeignKey(d => d.SupplyId)
                .HasConstraintName("FK__RegularSu__Suppl__10566F31");
        });

        modelBuilder.Entity<RegularSupplyMatch>(entity =>
        {
            entity.HasKey(e => e.RegularMatchId).HasName("PK__RegularS__98CD5428C2462B4D");

            entity.Property(e => e.MatchDate).HasColumnType("datetime");
            entity.Property(e => e.Note).HasColumnType("text");

            entity.HasOne(d => d.MatchedByWorker).WithMany(p => p.RegularSupplyMatches)
                .HasForeignKey(d => d.MatchedByWorkerId)
                .HasConstraintName("FK__RegularSu__Match__123EB7A3");

            entity.HasOne(d => d.RegularNeed).WithMany(p => p.RegularSupplyMatches)
                .HasForeignKey(d => d.RegularNeedId)
                .HasConstraintName("FK__RegularSu__Regul__114A936A");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__Schedule__9C8A5B49AB904B95");

            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.EventName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("行程");
            entity.Property(e => e.EventType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("個案訪問");
            entity.Property(e => e.Priority)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Case).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.CaseId)
                .HasConstraintName("FK__Schedules__CaseI__14270015");

            entity.HasOne(d => d.Worker).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.WorkerId)
                .HasConstraintName("FK__Schedules__Worke__1332DBDC");
        });

        modelBuilder.Entity<Supply>(entity =>
        {
            entity.HasKey(e => e.SupplyId).HasName("PK__Supplies__7CDD6CAE98A6C393");

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SupplyDescription).HasColumnType("text");
            entity.Property(e => e.SupplyName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SupplyPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SupplyType)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.SupplyCategory).WithMany(p => p.Supplies)
                .HasForeignKey(d => d.SupplyCategoryId)
                .HasConstraintName("FK__Supplies__Supply__151B244E");
        });

        modelBuilder.Entity<SupplyCategory>(entity =>
        {
            entity.HasKey(e => e.SupplyCategoryId).HasName("PK__SupplyCa__5ED6BB7689233BBE");

            entity.Property(e => e.SupplyCategoryName)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CA782D7E8");

            entity.HasIndex(e => e.IdentityNumber, "UQ__Users__6354A73FE3EC02B5").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105343C86E0BF").IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IdentityNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UserActivityRegistration>(entity =>
        {
            entity.HasKey(e => e.RegistrationId).HasName("PK__UserActi__6EF58810AB33EBAD");

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Activity).WithMany(p => p.UserActivityRegistrations)
                .HasForeignKey(d => d.ActivityId)
                .HasConstraintName("FK__UserActiv__Activ__17036CC0");

            entity.HasOne(d => d.User).WithMany(p => p.UserActivityRegistrations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserActiv__UserI__160F4887");
        });

        modelBuilder.Entity<UserOrder>(entity =>
        {
            entity.HasKey(e => e.UserOrderId).HasName("PK__UserOrde__35D02767D337C2D3");

            entity.HasIndex(e => e.OrderNumber, "UK_UserOrders_OrderNumber").IsUnique();

            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.OrderNumber).HasMaxLength(20);
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.UserOrders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserOrder__UserI__19DFD96B");
        });

        modelBuilder.Entity<UserOrderDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PK__UserOrde__135C316DFCE52535");

            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Supply).WithMany(p => p.UserOrderDetails)
                .HasForeignKey(d => d.SupplyId)
                .HasConstraintName("FK__UserOrder__Suppl__18EBB532");

            entity.HasOne(d => d.UserOrder).WithMany(p => p.UserOrderDetails)
                .HasForeignKey(d => d.UserOrderId)
                .HasConstraintName("FK__UserOrder__UserO__17F790F9");
        });

        modelBuilder.Entity<VwActivitiesFrontend>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_Activities_Frontend");

            entity.Property(e => e.ActivityId).ValueGeneratedOnAdd();
            entity.Property(e => e.ActivityName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.DisplayStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VwActivitiesStatus>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_Activities_Status");

            entity.Property(e => e.ActivityId).ValueGeneratedOnAdd();
            entity.Property(e => e.ActivityName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Category).HasMaxLength(10);
            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TargetAudience)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TimeBasedStatus)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.中文狀態)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.原始狀態)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.狀態檢查)
                .HasMaxLength(14)
                .IsUnicode(false);
            entity.Property(e => e.系統建議狀態)
                .HasMaxLength(9)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Worker>(entity =>
        {
            entity.HasKey(e => e.WorkerId).HasName("PK__Workers__077C88266D580019");

            entity.HasIndex(e => e.Email, "UQ__Workers__A9D10534AB50B5C5").IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
