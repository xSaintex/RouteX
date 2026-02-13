using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RouteX.Data;
using RouteX.Models;

var builder = WebApplication.CreateBuilder(args);

// ================== SERVICES ==================

// Add DbContext (SQL Server) with improved error handling
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null
            );
            sqlOptions.CommandTimeout(60);
        }
    )
);

// Add ASP.NET Core Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/LoginPage";
});

// Add Session support for login
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// ================== DATABASE SEEDING ==================
// Seed default admin user with Identity (with better error handling)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        // Wait a moment for database to be ready
        await Task.Delay(2000);

        // Check if Identity admin user exists
        var identityAdmin = await userManager.FindByEmailAsync("admin@routex.com");
        if (identityAdmin == null)
        {
            var adminUser = new IdentityUser
            {
                UserName = "admin@routex.com",
                Email = "admin@routex.com",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "admin123");

            if (result.Succeeded)
            {
                identityAdmin = adminUser;
            }
        }

        if (identityAdmin != null)
        {
            var hasPassword = await userManager.HasPasswordAsync(identityAdmin);
            if (hasPassword)
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(identityAdmin);
                await userManager.ResetPasswordAsync(identityAdmin, resetToken, "admin123");
            }
            else
            {
                await userManager.AddPasswordAsync(identityAdmin, "admin123");
            }

            // Also add/update custom Users table for compatibility
            var customUser = context.Users.FirstOrDefault(u => u.Email == "admin@routex.com");
            if (customUser == null)
            {
                context.Users.Add(new User
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@routex.com",
                    Password = "admin123",
                    Role = "Administrator",
                    Status = "Active"
                });
            }
            else
            {
                customUser.Password = "admin123";
                customUser.Status = "Active";
            }

            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding database. Application will continue without seeding.");
        // Don't throw the exception - let the application start
    }
}

// ================== MIDDLEWARE ==================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add Session middleware
app.UseSession();

// Identity authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// ================== ROUTES ==================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=LoginPage}/{id?}"
);

// Required for Identity UI
app.MapRazorPages();

app.Run();
