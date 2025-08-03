using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;
using NGO_WebAPI_Backend.Models.Shared;
using NGO_WebAPI_Backend.Services;
using NGO_WebAPI_Backend.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using NGO_WebAPI_Backend.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 添加服務到容器中
// 了解更多關於配置 OpenAPI 的資訊：https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 添加控制器支援
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        // 設定屬性名稱為 camelCase（前端期望的格式）
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 配置 Entity Framework 和資料庫連線
builder.Services.AddDbContext<NgoplatformDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

# region 依賴注入服務註冊

// === 資料存取層服務 ===
builder.Services.AddScoped<ICaseRepository, CaseRepository>();

// === 業務邏輯層服務 ===
builder.Services.AddScoped<ICaseService, CaseService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

// === 外部服務整合 ===
builder.Services.AddScoped<AzureOpenAIService>();

// TODO: 未來可加入的服務
// builder.Services.AddScoped<IAzureStorageService, AzureStorageService>();
// builder.Services.AddScoped<IEmailService, EmailService>();
// builder.Services.AddScoped<INotificationService, NotificationService>();

# endregion

// 註冊 FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCaseDtoValidator>();

// 配置 JWT 驗證
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "NGO_Platform_Super_Secret_Key_For_Development_Only_2024";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 添加 CORS 支援（根據環境決定允許的來源）
if (builder.Environment.IsDevelopment())
{
    // 開發環境：允許所有來源（方便開發）
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });
}
else
{
    // 生產環境：只允許特定前端網址
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.WithOrigins(
                    "https://happy-wave-01bfc3a00.2.azurestaticapps.net",     // 生產環境前端網址
                    "https://www.happy-wave-01bfc3a00.2.azurestaticapps.net"  // 如果有 www 版本
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials(); // 如果需要傳送 cookies
        });
    });
}

var app = builder.Build();

// 配置 HTTP 請求管道
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 啟用 CORS
app.UseCors("AllowAll");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 啟用認證和授權
app.UseAuthentication();
app.UseAuthorization();

// 添加預設路由－檢查用
app.MapGet("/", () => "NGO API 運作正常 - " + DateTime.Now.ToString());

// 密碼加密測試端點（僅開發環境）
if (app.Environment.IsDevelopment())
{
    app.MapGet("/test-password", () =>
    {
        return "密碼加密測試端點已移除";
    });

    // 密碼遷移端點已移除
}

// 控制器路由啟動
app.MapControllers();

app.Run();
