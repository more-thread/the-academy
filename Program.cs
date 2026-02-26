using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using TRS.Data;
using TRS.DTOs;
using TRS.Global;
using TRS.Interfaces;
using TRS.Middlewares;
using TRS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IJobClassService, JobClassService>();
builder.Services.AddScoped<ITrainingProgramService, TrainingProgramService>();
builder.Services.AddScoped<ITrainingScheduleService, TrainingScheduleService>();
builder.Services.AddScoped<ITrainingCourseService, TrainingCourseService>();
builder.Services.AddScoped<ITrainingCoordinatorService, TrainingCoordinatorService>();
builder.Services.AddScoped<ITrainingRegistrationService, TrainingRegistrationService>();
builder.Services.AddScoped<ITrainingFeedbackService, TrainingFeedbackService>();
builder.Services.AddScoped<IEmployeeAuthenticationService, EmployeeAuthenticationService>();
builder.Services.AddScoped<GlobalService>();
builder.Services.AddScoped<ISessionService, SessionService>();

builder.Services.AddRazorPages();
builder.Services.AddKendo();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddMvc().AddSessionStateTempDataProvider();
builder.Services.AddDbContext<AppDBContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("devCon")));

builder.Services.Configure<FtpSettingDto>(builder.Configuration.GetSection("FTPSetting"));
builder.Services.Configure<LdapSettingDto>(builder.Configuration.GetSection("LDAPSetting"));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

//app.UseMiddleware<ImpersonationMiddleware>();
app.UseSession();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Crypto}/{id?}");

app.Run();
