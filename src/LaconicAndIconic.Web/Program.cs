using LaconicAndIconic.BLL;
using LaconicAndIconic.DAL;
using LaconicAndIconic.Web.Middleware;
using LaconicAndIconic.Web.Seeding;
using LaconicAndIconic.Web.Services;
using LaconicAndIconic.BLL.Interfaces;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddControllersWithViews();
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer();
builder.Services.AddScoped<IFileService, FileService>();

// Add MemoryCache
builder.Services.AddMemoryCache();

// Configure CachingOptions from appsettings.json
builder.Services.Configure<CachingOptions>(builder.Configuration.GetSection("Caching"));
// Реєстрація AppSettings через IOptions
builder.Services.Configure<LaconicAndIconic.Web.Models.AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await TestUserSeeder.SeedAsync(app.Services);

await app.RunAsync();
