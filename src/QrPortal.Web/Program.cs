using Microsoft.AspNetCore.Authentication.Cookies;
using QrPortal.Application.Interfaces;
using QrPortal.Application.Services;
using QrPortal.DataAccess.Infrastructure;
using QrPortal.DataAccess.Interfaces;
using QrPortal.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/auth/login";
        o.AccessDeniedPath = "/auth/denied";
    });

builder.Services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
builder.Services.AddScoped<IClientSubscriptionRepository, ClientSubscriptionRepository>();
builder.Services.AddScoped<IQrTypeRepository, QrTypeRepository>();
builder.Services.AddScoped<IQrCategoryRepository, QrCategoryRepository>();
builder.Services.AddScoped<IQrCodeRepository, QrCodeRepository>();
builder.Services.AddScoped<IQrScanRepository, QrScanRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IQrCodeContentGenerator, QrCodeContentGenerator>();
builder.Services.AddScoped<IQrPayloadValidator, QrPayloadValidator>();
builder.Services.AddScoped<IRedirectResolver, RedirectResolver>();
builder.Services.AddScoped<IQrImageGenerator, QrImageGenerator>();

var app = builder.Build();
app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute(name: "redirect", pattern: "r/{shortCode}", defaults: new { controller = "Redirect", action = "Go" });
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
