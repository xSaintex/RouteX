using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using RouteX.Data;

using RouteX.Models;

using RouteX.Services;



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

// Add Audit Service
builder.Services.AddScoped<IAuditService, AuditService>();

// Add Fuel Price Service
builder.Services.AddScoped<IFuelPriceService, FuelPriceService>();
builder.Services.AddScoped<IRouteDistanceService, TomTomService>();
builder.Services.AddMemoryCache(); // Required for fuel price caching
builder.Services.AddHttpClient(); // Required for FuelPriceService



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

        await SeedUser(userManager, context, "admin@routex.com", "admin123", "Admin", "User", "Administrator");



        // Seed operations staff user

        await SeedUser(userManager, context, "operationstaff@routex.com", "opstaff123", "Operations", "Staff", "OperationsStaff");



        // Seed super admin user

        await SeedUser(userManager, context, "superadmin@routex.com", "supadmin123", "Super", "Admin", "SuperAdmin");



        // Seed finance user

                await SeedUser(userManager, context, "finance@routex.com", "finance123", "Finance", "User", "Finance");

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



    // Ensure password is set

    var hasPassword = await userManager.HasPasswordAsync(identityUser);

    if (!hasPassword)

    {

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

        Console.WriteLine($"Warning: Could not update custom Users table for {email}: {ex.Message}");

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

