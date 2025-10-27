using Elrakhawy.DAL.Data;
using ELRakhawy.DAL.Implementaions;
using ELRakhawy.DAL.Persistence;
using ELRakhawy.DAL.Services;
using ELRakhawy.EL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Text.Json;
#region Builder Zone for configuration

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddRazorPages();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenaricRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWOrk>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession(builder =>
{
    builder.IdleTimeout = TimeSpan.FromMinutes(30);
    builder.Cookie.HttpOnly = true;
    builder.Cookie.IsEssential = true;
    builder.Cookie.Name = "ELRakhawy.Session";
});

// ✅ أضف نظام الـ Authentication بالكوكيز
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // لو المستخدم مش داخل هيتحول هنا
        options.AccessDeniedPath = "/Auth/Denied"; // لو معندوش صلاحية
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // مدة الجلسة
        options.SlidingExpiration = true;
    });


var app = builder.Build();
#endregion

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDBContext>();
    context.Database.Migrate(); // apply migrations if needed
    DbSeeder.SeedUsersAsync(context).Wait();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
// ✅ Correct Order:
app.UseSession();  // ⚠️ Must be BEFORE Authentication
app.UseAuthentication();
app.UseMiddleware<SingleSessionMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
