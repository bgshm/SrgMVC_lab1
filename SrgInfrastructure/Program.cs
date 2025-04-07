using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SrgDomain.Model;
using SrgInfrastructure;
using SrgInfrastructure.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<MonthlyResetFilter>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<MonthlyResetFilter>();
});

// Configure the DbContext for Identity and your domain.
builder.Services.AddDbContext<SrgDatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add Identity services with custom ApplicationUser.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Configure password settings as desired.
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<SrgDatabaseContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();

// Optionally add Razor Pages for the Identity UI.
builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Call the seeder here.
        await RoleSeeder.SeedRolesAndAdminAsync(services);
        // You can add a log statement here to verify that seeding ran.
        Console.WriteLine("Role seeding completed.");
    }
    catch (Exception ex)
    {
        // Log the exception (or rethrow)
        Console.WriteLine($"Error during role seeding: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(@"D:\Bogdan\Шева\ІСТТП\Второй трай\Lab1_Shumeiko\img"),
    RequestPath = "/img"
});

app.UseRouting();

// Add authentication and authorization middleware.
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Departments}/{action=Index}/{id?}");

app.Run();
