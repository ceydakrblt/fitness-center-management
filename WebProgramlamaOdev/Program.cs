using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebProgramlamaOdev.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

// Connection String
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection")
    ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

// DbContext Ayarý
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity Ayarý
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    // Þifre gereksinimlerini gevþet (sau gibi basit þifre için)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// MVC Ekle
builder.Services.AddControllersWithViews();

// Razor Pages ekle (Identity için gerekli!)
builder.Services.AddRazorPages();

var app = builder.Build();

// Production Hata Yönetimi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Areas Route (Admin & User)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Razor Pages mapping (Identity sayfalarý için ZORUNLU!)
app.MapRazorPages();

// ----------------------
// ADMIN OLUÞTURMA - TEK SEFER
// ----------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    // 1) Rolleri oluþtur
    string[] roles = { "Admin", "Uye" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // 2) Admin kullanýcý oluþtur
    string adminEmail = "adminsau@gmail.com";
    string adminPassword = "sau123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(adminUser, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    else
    {
        // Kullanýcý varsa rolde olup olmadýðýný kontrol et
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

app.Run();