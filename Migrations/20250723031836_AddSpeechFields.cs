using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NGO_WebAPI_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeechFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    NewsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__News__954EBDF308AA3080", x => x.NewsId);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Token = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplyCategories",
                columns: table => new
                {
                    SupplyCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplyCategoryName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SupplyCa__5ED6BB7689233BBE", x => x.SupplyCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentityNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Password = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Phone = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CC4CA782D7E8", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Workers",
                columns: table => new
                {
                    WorkerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Password = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Role = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "staff")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Workers__077C88266D580019", x => x.WorkerId);
                });

            migrationBuilder.CreateTable(
                name: "Supplies",
                columns: table => new
                {
                    SupplyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplyName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    SupplyPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    SupplyCategoryId = table.Column<int>(type: "int", nullable: true),
                    SupplyQuantity = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    SupplyDescription = table.Column<string>(type: "text", nullable: true),
                    SupplyType = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Supplies__7CDD6CAE98A6C393", x => x.SupplyId);
                    table.ForeignKey(
                        name: "FK__Supplies__Supply__151B244E",
                        column: x => x.SupplyCategoryId,
                        principalTable: "SupplyCategories",
                        principalColumn: "SupplyCategoryId");
                });

            migrationBuilder.CreateTable(
                name: "UserOrders",
                columns: table => new
                {
                    UserOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    PaymentStatus = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    OrderNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderSource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyNeedId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserOrde__35D02767D337C2D3", x => x.UserOrderId);
                    table.ForeignKey(
                        name: "FK__UserOrder__UserI__19DFD96B",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    ActivityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Location = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    MaxParticipants = table.Column<int>(type: "int", nullable: true),
                    CurrentParticipants = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    SignupDeadline = table.Column<DateOnly>(type: "date", nullable: true),
                    WorkerId = table.Column<int>(type: "int", nullable: true),
                    TargetAudience = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "open"),
                    Category = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Activiti__45F4A7914643EE44", x => x.ActivityId);
                    table.ForeignKey(
                        name: "FK__Activitie__Worke__03F0984C",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "WorkerId");
                });

            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    CaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    IdentityNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Birthday = table.Column<DateOnly>(type: "date", nullable: true),
                    WorkerId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ProfileImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    District = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DetailAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SpeechToText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpeechToTextAudioUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Cases__6CAE524C4841F794", x => x.CaseId);
                    table.ForeignKey(
                        name: "FK__Cases__WorkerId__09A971A2",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "WorkerId");
                });

            migrationBuilder.CreateTable(
                name: "RegularDistributionBatch",
                columns: table => new
                {
                    DistributionBatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistributionDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CaseCount = table.Column<int>(type: "int", nullable: false),
                    TotalSupplyItems = table.Column<int>(type: "int", nullable: false),
                    CreatedByWorkerId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    ApprovedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    ApprovedByWorkerId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RegularD__548A1428EA2B5191", x => x.DistributionBatchId);
                    table.ForeignKey(
                        name: "FK__RegularDi__Appro__41EDCAC5",
                        column: x => x.ApprovedByWorkerId,
                        principalTable: "Workers",
                        principalColumn: "WorkerId");
                    table.ForeignKey(
                        name: "FK__RegularDi__Creat__40F9A68C",
                        column: x => x.CreatedByWorkerId,
                        principalTable: "Workers",
                        principalColumn: "WorkerId");
                });

            migrationBuilder.CreateTable(
                name: "UserOrderDetails",
                columns: table => new
                {
                    DetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserOrderId = table.Column<int>(type: "int", nullable: true),
                    SupplyId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    OrderSource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyNeedId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserOrde__135C316DFCE52535", x => x.DetailId);
                    table.ForeignKey(
                        name: "FK__UserOrder__Suppl__18EBB532",
                        column: x => x.SupplyId,
                        principalTable: "Supplies",
                        principalColumn: "SupplyId");
                    table.ForeignKey(
                        name: "FK__UserOrder__UserO__17F790F9",
                        column: x => x.UserOrderId,
                        principalTable: "UserOrders",
                        principalColumn: "UserOrderId");
                });

            migrationBuilder.CreateTable(
                name: "UserActivityRegistrations",
                columns: table => new
                {
                    RegistrationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ActivityId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    NumberOfCompanions = table.Column<int>(type: "int", nullable: true),
                    RegisterTime = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserActi__6EF58810AB33EBAD", x => x.RegistrationId);
                    table.ForeignKey(
                        name: "FK__UserActiv__Activ__17036CC0",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "ActivityId");
                    table.ForeignKey(
                        name: "FK__UserActiv__UserI__160F4887",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "CaseActivityRegistrations",
                columns: table => new
                {
                    RegistrationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseId = table.Column<int>(type: "int", nullable: true),
                    ActivityId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    RegisterTime = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CaseActi__6EF58810AFB8FC95", x => x.RegistrationId);
                    table.ForeignKey(
                        name: "FK__CaseActiv__Activ__05D8E0BE",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "ActivityId");
                    table.ForeignKey(
                        name: "FK__CaseActiv__CaseI__04E4BC85",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "CaseId");
                });

            migrationBuilder.CreateTable(
                name: "CaseLogins",
                columns: table => new
                {
                    CaseId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Password = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    LastLogin = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CaseLogi__6CAE524CB0B15E21", x => x.CaseId);
                    table.ForeignKey(
                        name: "FK__CaseLogin__CaseI__06CD04F7",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "CaseId");
                });

            migrationBuilder.CreateTable(
                name: "CaseOrders",
                columns: table => new
                {
                    CaseOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseId = table.Column<int>(type: "int", nullable: true),
                    SupplyId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    OrderTime = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CaseOrde__4CFB6563687E973E", x => x.CaseOrderId);
                    table.ForeignKey(
                        name: "FK__CaseOrder__CaseI__07C12930",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "CaseId");
                    table.ForeignKey(
                        name: "FK__CaseOrder__Suppl__08B54D69",
                        column: x => x.SupplyId,
                        principalTable: "Supplies",
                        principalColumn: "SupplyId");
                });

            migrationBuilder.CreateTable(
                name: "EmergencySupplyNeeds",
                columns: table => new
                {
                    EmergencyNeedId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseId = table.Column<int>(type: "int", nullable: true),
                    WorkerId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    CollectedQuantity = table.Column<int>(type: "int", nullable: true),
                    SupplyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Priority = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Emergenc__D9A4C6FA1D1F8990", x => x.EmergencyNeedId);
                    table.ForeignKey(
                        name: "FK__Emergency__CaseI__0C85DE4D",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "CaseId");
                    table.ForeignKey(
                        name: "FK__Emergency__Worke__0E6E26BF",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "WorkerId");
                });

            migrationBuilder.CreateTable(
                name: "RegularSuppliesNeeds",
                columns: table => new
                {
                    RegularNeedId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseId = table.Column<int>(type: "int", nullable: true),
                    SupplyId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    ApplyDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    PickupDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    BatchId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RegularS__12A5D64A7B60206E", x => x.RegularNeedId);
                    table.ForeignKey(
                        name: "FK__RegularSu__CaseI__0F624AF8",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "CaseId");
                    table.ForeignKey(
                        name: "FK__RegularSu__Suppl__10566F31",
                        column: x => x.SupplyId,
                        principalTable: "Supplies",
                        principalColumn: "SupplyId");
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkerId = table.Column<int>(type: "int", nullable: true),
                    CaseId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    Priority = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    EventType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "個案訪問"),
                    EventName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "行程")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Schedule__9C8A5B49AB904B95", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK__Schedules__CaseI__14270015",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "CaseId");
                    table.ForeignKey(
                        name: "FK__Schedules__Worke__1332DBDC",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "WorkerId");
                });

            migrationBuilder.CreateTable(
                name: "EmergencySupplyMatches",
                columns: table => new
                {
                    EmergencyMatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmergencyNeedId = table.Column<int>(type: "int", nullable: true),
                    MatchedQuantity = table.Column<int>(type: "int", nullable: true),
                    MatchDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    MatchedByWorkerId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Emergenc__9E89AB71B11CAC01", x => x.EmergencyMatchId);
                    table.ForeignKey(
                        name: "FK__Emergency__Emerg__0A9D95DB",
                        column: x => x.EmergencyNeedId,
                        principalTable: "EmergencySupplyNeeds",
                        principalColumn: "EmergencyNeedId");
                    table.ForeignKey(
                        name: "FK__Emergency__Match__0B91BA14",
                        column: x => x.MatchedByWorkerId,
                        principalTable: "Workers",
                        principalColumn: "WorkerId");
                });

            migrationBuilder.CreateTable(
                name: "RegularSupplyMatches",
                columns: table => new
                {
                    RegularMatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegularNeedId = table.Column<int>(type: "int", nullable: true),
                    MatchedQuantity = table.Column<int>(type: "int", nullable: true),
                    MatchDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    MatchedByWorkerId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RegularS__98CD5428C2462B4D", x => x.RegularMatchId);
                    table.ForeignKey(
                        name: "FK__RegularSu__Match__123EB7A3",
                        column: x => x.MatchedByWorkerId,
                        principalTable: "Workers",
                        principalColumn: "WorkerId");
                    table.ForeignKey(
                        name: "FK__RegularSu__Regul__114A936A",
                        column: x => x.RegularNeedId,
                        principalTable: "RegularSuppliesNeeds",
                        principalColumn: "RegularNeedId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_WorkerId",
                table: "Activities",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseActivityRegistrations_ActivityId",
                table: "CaseActivityRegistrations",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseActivityRegistrations_CaseId",
                table: "CaseActivityRegistrations",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "UQ__CaseLogi__A9D105346FD037AD",
                table: "CaseLogins",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CaseOrders_CaseId",
                table: "CaseOrders",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseOrders_SupplyId",
                table: "CaseOrders",
                column: "SupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_WorkerId",
                table: "Cases",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "UQ__Cases__6354A73F80F4F619",
                table: "Cases",
                column: "IdentityNumber",
                unique: true,
                filter: "[IdentityNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencySupplyMatches_EmergencyNeedId",
                table: "EmergencySupplyMatches",
                column: "EmergencyNeedId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencySupplyMatches_MatchedByWorkerId",
                table: "EmergencySupplyMatches",
                column: "MatchedByWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencySupplyNeeds_CaseId",
                table: "EmergencySupplyNeeds",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencySupplyNeeds_WorkerId",
                table: "EmergencySupplyNeeds",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_RegularDistributionBatch_ApprovedByWorkerId",
                table: "RegularDistributionBatch",
                column: "ApprovedByWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_RegularDistributionBatch_CreatedByWorkerId",
                table: "RegularDistributionBatch",
                column: "CreatedByWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_RegularSuppliesNeeds_CaseId",
                table: "RegularSuppliesNeeds",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_RegularSuppliesNeeds_SupplyId",
                table: "RegularSuppliesNeeds",
                column: "SupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_RegularSupplyMatches_MatchedByWorkerId",
                table: "RegularSupplyMatches",
                column: "MatchedByWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_RegularSupplyMatches_RegularNeedId",
                table: "RegularSupplyMatches",
                column: "RegularNeedId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_CaseId",
                table: "Schedules",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_WorkerId",
                table: "Schedules",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplies_SupplyCategoryId",
                table: "Supplies",
                column: "SupplyCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityRegistrations_ActivityId",
                table: "UserActivityRegistrations",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityRegistrations_UserId",
                table: "UserActivityRegistrations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrderDetails_SupplyId",
                table: "UserOrderDetails",
                column: "SupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrderDetails_UserOrderId",
                table: "UserOrderDetails",
                column: "UserOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrders_UserId",
                table: "UserOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UK_UserOrders_OrderNumber",
                table: "UserOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Users__6354A73FE3EC02B5",
                table: "Users",
                column: "IdentityNumber",
                unique: true,
                filter: "[IdentityNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D105343C86E0BF",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__Workers__A9D10534AB50B5C5",
                table: "Workers",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseActivityRegistrations");

            migrationBuilder.DropTable(
                name: "CaseLogins");

            migrationBuilder.DropTable(
                name: "CaseOrders");

            migrationBuilder.DropTable(
                name: "EmergencySupplyMatches");

            migrationBuilder.DropTable(
                name: "News");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "RegularDistributionBatch");

            migrationBuilder.DropTable(
                name: "RegularSupplyMatches");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "UserActivityRegistrations");

            migrationBuilder.DropTable(
                name: "UserOrderDetails");

            migrationBuilder.DropTable(
                name: "EmergencySupplyNeeds");

            migrationBuilder.DropTable(
                name: "RegularSuppliesNeeds");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "UserOrders");

            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.DropTable(
                name: "Supplies");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Workers");

            migrationBuilder.DropTable(
                name: "SupplyCategories");
        }
    }
}
