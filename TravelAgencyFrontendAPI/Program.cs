using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Hubs;
using TravelAgency.Shared.Data;
using Microsoft.Extensions.FileProviders;

//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;

var builder = WebApplication.CreateBuilder(args);
//新增 Swagger 設定
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:3000", "https://localhost:7107", "https://localhost:7258", "https://192.168.1.122:3000")
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

//builder.Services.AddAuthorization();


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
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "Uploads")),
    RequestPath = "/Uploads"
});

app.UseRouting();               

app.UseCors("AllowFrontend");  

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.Run();