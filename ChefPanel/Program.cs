using ChefPanel.Services;
using Application;
using Application.Common.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddApplicationServices();
// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("WarrensCateringCS")));
//for Identity
builder.Services.AddTransient<IidentityService, IdentityService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<IDateTime, DateTimeService>();
builder.Services.AddTransient<IApplicationDbContext, ApplicationDbContext>();
// to keep consistent with API program file, to avoid errors
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
//Register Token Provider service
var tokenProvider = new TokenProviderService(builder.Configuration["JWT:ValidIssuer"] ?? "", builder.Configuration["JWT:ValidAudience"] ?? "", builder.Configuration["JWT:Secret"] ?? "");
builder.Services.AddSingleton<ITokenProvider>(tokenProvider);
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// for redirecting to login view when unauthorized user tries to access an API
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/accounts/login";
});


var app = builder.Build();

/// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
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
    pattern: "{controller=Accounts}/{action=Login}/{id?}");

app.Run();
