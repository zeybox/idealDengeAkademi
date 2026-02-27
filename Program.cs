using HizliOgren.Data;
using HizliOgren.Filters;
using HizliOgren.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Tüm isteklerde dosya/body boyutu üst sınırı: 30 MB (hiçbir yükleme bu değeri aşamaz)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 30 * 1024 * 1024;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<PayTRService>();
builder.Services.AddScoped<CaptchaService>();
builder.Services.AddScoped<LoadSiteTextsFilter>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(30); });

// Giriş çerezinin IIS uygulama havuzu yenilense bile geçerli kalması için anahtarları diske yazıyoruz
var keysPath = Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys");
Directory.CreateDirectory(keysPath);
builder.Services.AddDataProtection()
    .SetApplicationName("HizliOgren")
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

var authBuilder = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

var googleClientId = builder.Configuration["Google:ClientId"];
if (!string.IsNullOrWhiteSpace(googleClientId))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = builder.Configuration["Google:ClientSecret"] ?? "";
    });
}

builder.Services.AddControllersWithViews(o => o.Filters.Add<LoadSiteTextsFilter>());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try { await db.Database.MigrateAsync(); } catch { /* DB may not exist yet */ }
    // DurationMinutes kolonu yoksa migration kaydını silip MigrateAsync ile tekrar uygulat (senkronizasyon)
    try
    {
        var columnExists = 1;
        try
        {
            columnExists = await db.Database.SqlQueryRaw<int>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'CurriculumItems' AND COLUMN_NAME = N'DurationMinutes') THEN 1 ELSE 0 END AS Value").FirstOrDefaultAsync();
        }
        catch { columnExists = 0; }
        if (columnExists == 0)
        {
            await db.Database.ExecuteSqlRawAsync(
                "DELETE FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260202000001_AddDurationMinutesToCurriculumItem'");
            await db.Database.MigrateAsync();
        }
    }
    catch { /* Tablo yok veya yetki yok; devam et */ }
    // Users tablosunda eğitmen profil kolonları yoksa ekle
    try
    {
        var hasTitle = await db.Database.SqlQueryRaw<int>(
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'Users' AND COLUMN_NAME = N'Title') THEN 1 ELSE 0 END AS Value").FirstOrDefaultAsync();
        if (hasTitle == 0)
        {
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE Users ADD Title nvarchar(150) NULL");
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE Users ADD Bio nvarchar(2000) NULL");
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE Users ADD AvatarUrl nvarchar(500) NULL");
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE Users ADD Phone nvarchar(50) NULL");
        }
    }
    catch { /* Yetki veya tablo yok; devam et */ }
    // Courses tablosunda Highlights ve ContentSummary yoksa ekle (tek SQL ile)
    try
    {
        await db.Database.ExecuteSqlRawAsync(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Courses')
BEGIN
  IF NOT EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id WHERE t.name = 'Courses' AND c.name = 'Highlights')
    ALTER TABLE Courses ADD Highlights nvarchar(2000) NULL;
  IF NOT EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id WHERE t.name = 'Courses' AND c.name = 'ContentSummary')
    ALTER TABLE Courses ADD ContentSummary nvarchar(1000) NULL;
END
");
    }
    catch { /* Yetki veya tablo yok; devam et */ }
    // HeroSlides tablosu yoksa oluştur
    try
    {
        var tableExists = await db.Database.SqlQueryRaw<int>(
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.tables WHERE name = N'HeroSlides') THEN 1 ELSE 0 END AS Value").FirstOrDefaultAsync();
        if (tableExists == 0)
        {
            await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE HeroSlides (
  Id int IDENTITY(1,1) PRIMARY KEY,
  Title nvarchar(300) NOT NULL,
  Text nvarchar(1000) NULL,
  ImageUrl nvarchar(500) NULL,
  ImageAlt nvarchar(200) NULL,
  SortOrder int NOT NULL
)");
        }
    }
    catch { /* Yetki veya tablo yok; devam et */ }
    // InstructorApplications tablosu yoksa oluştur (Eğitmen Başvurusu)
    try
    {
        var tableExists = await db.Database.SqlQueryRaw<int>(
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.tables WHERE name = N'InstructorApplications') THEN 1 ELSE 0 END AS Value").FirstOrDefaultAsync();
        if (tableExists == 0)
        {
            await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE InstructorApplications (
  Id int IDENTITY(1,1) PRIMARY KEY,
  FullName nvarchar(200) NOT NULL,
  Email nvarchar(256) NOT NULL,
  Phone nvarchar(50) NULL,
  City nvarchar(100) NULL,
  Message nvarchar(2000) NULL,
  UserId int NULL,
  Status int NOT NULL DEFAULT 0,
  AdminNotes nvarchar(2000) NULL,
  CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
  CONSTRAINT FK_InstructorApplications_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
)");
        }
    }
    catch { /* Yetki veya tablo yok; devam et */ }
    await DbSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<AuthService>());
}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
