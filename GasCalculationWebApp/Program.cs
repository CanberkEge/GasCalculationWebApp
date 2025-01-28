using Microsoft.EntityFrameworkCore; // DbContext için gerekli
using GasCalculationWebApp.Data;     // ApplicationDbContext'e eriþim için gerekli
using Microsoft.AspNetCore.Authentication.Cookies; // Cookie tabanlý kimlik doðrulama için gerekli





var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();  // MVC'yi ekleyin

// Veritabaný baðlantýsý (ConnectionString'i appsettings.json'dan alýyor)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Kullanýcý kimlik doðrulama ayarlarý (Cookie tabanlý kimlik doðrulama)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Giriþ yapmayan kullanýcýyý yönlendirme
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisiz eriþim için yönlendirme
    });






var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Kimlik doðrulama ve yetkilendirme iþlemleri

app.UseAuthentication(); // Kullanýcý oturum açma iþlemleri için gerekli
app.UseAuthorization(); // Yetki kontrolü

// MVC yapýlandýrmasý
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");  // MVC için rota

app.Run();


