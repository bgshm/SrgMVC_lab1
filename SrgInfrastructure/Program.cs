using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SrgDomain.Model;
using SrgInfrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1) DbContext
builder.Services.AddDbContext<SrgDatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 2) Identity (with Roles, EF stores, Default UI, Token Providers)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // Sign‑in settings
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<SrgDatabaseContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// 3) MVC + Global Filter
builder.Services.AddScoped<MonthlyResetFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<MonthlyResetFilter>();
});

// 4) Razor Pages (for Identity UI)
builder.Services.AddRazorPages();

var app = builder.Build();

// 5) Seed Roles & Admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await RoleSeeder.SeedRolesAndAdminAsync(services);
        Console.WriteLine("Role seeding completed.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during role seeding: {ex.Message}");
    }
}

// 6) Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Serve your external img folder under /img
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        @"D:\Bogdan\Шева\ІСТТП\Второй трай\Lab1_Shumeiko\img"),
    RequestPath = "/img"
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 7) Endpoints
app.MapRazorPages();  // Identity UI
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Departments}/{action=Index}/{id?}");

app.Run();
