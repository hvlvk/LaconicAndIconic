using LaconicAndIconic.BLL;
using LaconicAndIconic.DAL;
using LaconicAndIconic.Web.Middleware;
using LaconicAndIconic.Web.Seeding;
using LaconicAndIconic.Web.Services;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.Web.Models;
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
builder.Services.Configure<TheMealDbOptions>(builder.Configuration.GetSection(TheMealDbOptions.SectionName));
builder.Services.AddHttpClient<IExternalRecipeClient, TheMealDbClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<TheMealDbOptions>>().Value;
    client.BaseAddress = options.BaseUrl;
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

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

app.UseMiddleware<ExecutionTimeMiddleware>();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await TestUserSeeder.SeedAsync(app.Services);

await app.RunAsync();
