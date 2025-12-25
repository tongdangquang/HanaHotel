using FluentValidation;
using FluentValidation.AspNetCore;
using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.BusinessLayer.Concrete;
using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.EntityLayer.Concrete;
using HanaHotel.WebUI.DTOs.GuestDTO;
using HanaHotel.WebUI.Models;
using HanaHotel.WebUI.ValidationRules.AdminGuestValidationRules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using VNPAY.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Load config
var urlApi = builder.Configuration["AppSettings:urlAPI"];

// -------------------------
// SERVICE CONFIGURATION
// -------------------------

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// DbContext
builder.Services.AddDbContext<DataContext>();

// Identity + Role
builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

// AutoMapper
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Global authorization (require login)
builder.Services.AddMvc(config =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    config.Filters.Add(new AuthorizeFilter(policy));
})
.AddRazorRuntimeCompilation();

// HttpClient
builder.Services.AddHttpClient();

// Register named HttpClient using config value
builder.Services.AddHttpClient("momoClient", client =>
{
    var endpoint = builder.Configuration["MoMo:Endpoint"];
    if (!string.IsNullOrWhiteSpace(endpoint) && Uri.TryCreate(endpoint, UriKind.Absolute, out var u))
    {
        // set base to scheme+host (optional) or leave BaseAddress and call PostAsync(endpoint,...) later
        client.BaseAddress = new Uri(u.GetLeftPart(UriPartial.Authority));
    }
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Cookie của Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Login/Index";
    options.AccessDeniedPath = "/Login/AccessDenied";
    options.SlidingExpiration = true;
});

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// HttpContext
builder.Services.AddHttpContextAccessor();

// AppSettings
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddScoped<IValidator<CreateGuestDTO>, CreateGuestValidator>();

// Business services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IPromotionDetailService, PromotionDetailService>();
//builder.Services.AddScoped<IServiceDal, ServiceDal>();
//builder.Services.AddScoped<IServiceService, ServiceService>();

var vnpayConfig = builder.Configuration.GetSection("VNPAY");
builder.Services.AddVnpayClient(config =>
{
	config.TmnCode = vnpayConfig["TmnCode"]!;
	config.HashSecret = vnpayConfig["HashSecret"]!;
	config.CallbackUrl = vnpayConfig["CallbackUrl"]!;
});

var app = builder.Build();

// -------------------------
// PIPELINE CONFIGURATION
// -------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// ROLE + USER INITIALIZATION
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    await EnsureBasicRolesCreatedAsync(roleManager);
    await AssignCustomerRoleToAllUsersAsync(userManager);
}

//await DataInitializer.TestDataAsync(app);

app.UseStatusCodePagesWithRedirects("/Error/Error404/?code={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Default}/{action=Index}/{id?}");

app.Run();

// -------------------------
// HELPER METHODS
// -------------------------

static async Task EnsureBasicRolesCreatedAsync(RoleManager<Role> roleManager)
{
    var roleNames = new[] { "Admin", "User", "Manager", "Customer", "Staff" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new Role
            {
                Name = roleName,
                NormalizedName = roleName.ToUpper()
            });
        }
    }
}

static async Task AssignCustomerRoleToAllUsersAsync(UserManager<User> userManager)
{
    var allUsers = userManager.Users.ToList();

    foreach (var user in allUsers)
    {
        var roles = await userManager.GetRolesAsync(user);

        if (roles.Count == 0)
        {
            await userManager.AddToRoleAsync(user, "Customer");
        }
    }
}
