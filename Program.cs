using Microsoft.AspNetCore.Authentication;
using NCRManagementSystem.Data;
using NCRManagementSystem.Extensions;
using NCRManagementSystem.Middleware;
using NCRManagementSystem.Repositories.Implementations;
using NCRManagementSystem.Repositories.Interfaces;
using NCRManagementSystem.Services.Implementations;
using NCRManagementSystem.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add custom services
builder.Services.AddCustomServices(builder.Configuration);

// Add repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INCRRepository, NCRRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<INCRFileRepository, NCRFileRepository>();
builder.Services.AddScoped<INCRHistoryRepository, NCRHistoryRepository>();
builder.Services.AddScoped<INCRCommentRepository, NCRCommentRepository>();

// Add services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INCRService, NCRService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IQAService, QAService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IManagerService, ManagerService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Add database connection
builder.Services.AddScoped<DbConnection>();

// Add session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Add authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireManagerRole", policy => policy.RequireRole("Manager", "Admin"));
    options.AddPolicy("RequireQARole", policy => policy.RequireRole("QA", "Manager", "Admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User", "QA", "Manager", "Admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add security headers middleware
//app.UseMiddleware<SecurityHeadersMiddleware>();


// Add exception handling middleware
//app.UseMiddleware<ExceptionHandlingMiddleware>();.
//builder.Services.AddHttpContextAccessor();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Add authentication middleware
app.UseMiddleware<NCRManagementSystem.Middleware.AuthenticationMiddleware>();

// Add HTTP Context Accessor


// Add SessionManager
//builder.Services.AddScoped<SessionManager>();

// Use custom authentication middleware
app.UseCustomAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await dbInitializer.InitializeAsync();
}

app.Run();