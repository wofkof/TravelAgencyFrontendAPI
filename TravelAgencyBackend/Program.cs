using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgencyBackend.Services;
using TravelAgencyBackend.ViewComponent;

var builder = WebApplication.CreateBuilder(args);

//依賴注入
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PermissionCheckService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//----------------------------------------------

// 加入 Session 服務（必要）
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session 存活時間
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 啟用 Session（順序重要！要在 UseRouting 後、UseAuthorization 前）
app.UseSession();

app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");


app.Run();
