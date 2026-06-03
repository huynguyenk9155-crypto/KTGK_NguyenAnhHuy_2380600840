using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KTGK_LapTrinhWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Cấu hình ASP.NET Core Identity hỗ trợ Roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Cấu hình mật khẩu đơn giản để dễ chấm điểm/test
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    
    // Cấu hình đăng nhập
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cấu hình Cookie cho việc chuyển hướng Auth tới AccountController tùy biến
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Cấu hình Authentication & Đăng nhập bằng Google
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        // Đọc cấu hình từ appsettings.json. Nếu không có, dùng dummy key để nút hiển thị
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "dummy-google-client-id.apps.googleusercontent.com";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "dummy-google-client-secret";
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed database khi ứng dụng bắt đầu
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi xảy ra trong quá trình seed database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Cấu hình phục vụ file tĩnh (CSS, JS, Images)
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
