using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineAppointment.Data;
using OnlineAppointment.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using OnlineAppointment.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity + UI + Token Providers
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // e-posta onayı istemiyoruz

    // password rules – gevşetilmiş
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false; // özel karakter
    options.Password.RequiredUniqueChars = 1;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI(); // <<— hazır Login/Register sayfaları

// IEmailSender = No-op (mail göndermiyoruz)
builder.Services.AddTransient<IEmailSender, AppEmailSender>();


// MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");                      // 500 vb.
    app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}"); // 404 vb.
    app.UseHsts();
}


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Identity UI için şart

using (var scope = app.Services.CreateScope())
{
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // 1) Rolleri oluştur
    string[] roles = { "Admin", "Staff" };
    foreach (var r in roles)
    {
        if (!await roleMgr.RoleExistsAsync(r))
            await roleMgr.CreateAsync(new IdentityRole(r));
    }

    // 2) appsettings.json’dan admin email al
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var adminEmail = config["Seed:AdminEmail"];

    if (!string.IsNullOrWhiteSpace(adminEmail))
    {
        var adminUser = await userMgr.FindByEmailAsync(adminEmail);
        if (adminUser != null && !await userMgr.IsInRoleAsync(adminUser, "Admin"))
        {
            await userMgr.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

app.Run();
