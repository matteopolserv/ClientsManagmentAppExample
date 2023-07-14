global using ClientsManagmentAppExample.Models;
using ClientsManagmentAppExample.Interfaces;
using ClientsManagmentAppExample.Repositories;
using ClientsManagmentAppExample.Data;
using ClientsManagmentAppExample.Authorization;
using ClientsManagmentAppExample.Helpers;
using ClientsManagmentAppExample.Services;
using MediatR.NotificationPublishers;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    });
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddScoped<IHelpers, Helpers>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICooperatorRepository, CooperatorRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.NotificationPublisher = new TaskWhenAllPublisher();
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 5120000000;
    options.MaxRequestBodyBufferSize = 2048000000;
});
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
});

builder.Services.AddDefaultIdentity<IdentityUser>(
    options => 
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 12;
        options.Lockout.MaxFailedAccessAttempts = 15;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(12);
     }).AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsClient", policy => policy.Requirements.Add(new CheckRole("client")));
});
//builder.Services.AddTransient<IEmailSender, AuthMessageSender>();
//builder.Services.AddTransient<IEmailSender, sendEmail>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
