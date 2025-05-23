using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Hubs;
using TravelAgency.Shared.Data;
using TravelAgencyFrontendAPI.Helpers;
using Microsoft.Extensions.FileProviders;

using TravelAgencyFrontendAPI.ECPay.Models; // 引入 ECPayConfiguration
using TravelAgencyFrontendAPI.ECPay.Services; // 引入 ECPayService

//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
// 加入你自己的寄信服務
builder.Services.AddScoped<EmailService>();

//新增 Swagger 設定
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:3000", "https://localhost:7107", "https://localhost:7258", "https://192.168.1.122:3000", "https://172.18.132.158:3000", "https://9621-49-159-210-217.ngrok-free.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(7265, listenOptions =>
//    {
//        listenOptions.UseHttps("certs/travel-api.pfx", "1234");
//    });
//});

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 驗證服務
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options => // 添加 JWT Bearer 驗證處理器
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true, // 是否驗證發行者
//        ValidateAudience = true, // 是否驗證接收者
//        ValidateLifetime = true, // 是否驗證 Token 的有效期限
//        ValidateIssuerSigningKey = true, // 是否驗證簽名金鑰
//        ValidIssuer = builder.Configuration["Jwt:Issuer"], // 從 appsettings.json 讀取 Issuer
//        ValidAudience = builder.Configuration["Jwt:Audience"], // 從 appsettings.json 讀取 Audience
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) // 從 appsettings.json 讀取並轉換金鑰
//    };

//    options.Events = new JwtBearerEvents
//    {
//        OnMessageReceived = context =>
//        {
//            var accessToken = context.Request.Query["access_token"];
//            var path = context.HttpContext.Request.Path;
//            if (!string.IsNullOrEmpty(accessToken) &&
//                (path.StartsWithSegments("/chathub"))) // 只針對 chathub
//            {
//                context.Token = accessToken;
//            }
//            return Task.CompletedTask;
//        }
//    };
//});

builder.Services.AddAuthorization();


builder.Services.Configure<ECPayConfiguration>(builder.Configuration.GetSection("ECPaySettings"));
builder.Services.AddScoped<ECPayService>();



var app = builder.Build();
// 啟用 Swagger
app.UseSwagger();
app.UseSwaggerUI();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var seeder = new TestDataSeeder(context);
        await seeder.SeedAsync();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var mvcWwwroot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../TravelAgencyBackend/wwwroot"));
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mvcWwwroot),
    RequestPath = "/uploads"
});

app.UseRouting();               

app.UseCors("AllowFrontend");  

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");


app.Run();