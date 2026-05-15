using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using RouteX.Data;

using RouteX.Models;

using RouteX.Services;

using System.Threading.RateLimiting;

using Microsoft.AspNetCore.RateLimiting;



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

    options.Password.RequireDigit = true;

    options.Password.RequireLowercase = true;

    options.Password.RequireUppercase = true;

    options.Password.RequireNonAlphanumeric = true;

    options.Password.RequiredLength = 8;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    options.Lockout.MaxFailedAccessAttempts = 5;

    options.Lockout.AllowedForNewUsers = true;

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
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});



// Add MVC

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

// Add Audit Service
builder.Services.AddScoped<IAuditService, AuditService>();

// Add Fuel Price Service
builder.Services.AddScoped<IFuelPriceService, FuelPriceService>();
builder.Services.AddScoped<IRouteDistanceService, TomTomService>();
builder.Services.AddMemoryCache(); // Required for fuel price caching
builder.Services.AddHttpClient(); // Required for FuelPriceService

// Add Rate Limiting (Issue 8)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});



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

        await Task.Delay(3000);



        // Ensure database is created

        context.Database.EnsureCreated();



        // Seed admin user

        await SeedUser(userManager, context, "admin@routex.com", "Admin@1234", "Admin", "User", "Administrator");



        // Seed operations staff user

        await SeedUser(userManager, context, "operationstaff@routex.com", "OpStaff@1234", "Operations", "Staff", "OperationsStaff");



        // Seed super admin user

        await SeedUser(userManager, context, "superadmin@routex.com", "SupAdmin@1234", "Super", "Admin", "SuperAdmin");



        // Seed finance user

                await SeedUser(userManager, context, "finance@routex.com", "Finance@1234", "Finance", "User", "Finance");

                // Seed branch data
                BranchSeedData.SeedBranches(context);

            }

            catch (Exception ex)

    {

        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogError(ex, "An error occurred while seeding database. Application will continue without seeding.");

        // Don't throw the exception - let the application start

    }

}



async Task SeedUser(UserManager<IdentityUser> userManager, ApplicationDbContext context, string email, string password, string firstName, string lastName, string role)

{

    // Check if Identity user exists

    var identityUser = await userManager.FindByEmailAsync(email);

    if (identityUser == null)

    {

        identityUser = new IdentityUser

        {

            UserName = email,

            Email = email,

            EmailConfirmed = true

        };



        var result = await userManager.CreateAsync(identityUser, password);

        if (!result.Succeeded)

        {

            throw new Exception($"Failed to create Identity user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        }

    }



    // Ensure password is set and meets current policy (reset if user already exists with a weak password)

    var hasPassword = await userManager.HasPasswordAsync(identityUser);

    if (!hasPassword)

    {

        await userManager.AddPasswordAsync(identityUser, password);

    }

    else

    {

        // Reset password to ensure it meets the current strong policy

        await userManager.RemovePasswordAsync(identityUser);

        await userManager.AddPasswordAsync(identityUser, password);

    }



    // Add/update custom Users table entry

    try

    {

        var customUser = context.Users.FirstOrDefault(u => u.Email == email);

        if (customUser == null)

        {

            context.Users.Add(new User

            {

                FirstName = firstName,

                LastName = lastName,

                Email = email,

                Password = identityUser.PasswordHash ?? string.Empty,

                Role = role,

                Status = "Active"

            });

        }

        else

        {

            customUser.Password = identityUser.PasswordHash ?? string.Empty;

            customUser.Role = role;

            customUser.Status = "Active";

        }



        await context.SaveChangesAsync();

    }

    catch (Exception ex)

    {

        // Log but don't fail - Identity user is the important part
        var logger = userManager.Logger;
        logger.LogWarning(ex, "Could not update custom Users table for {Email}", email);

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

// Rate limiting middleware (after UseRouting, before UseAuthentication)
app.UseRateLimiter();

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

