using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CRbooru.Models;
using CRbooru.Data;
using CRbooru.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<CRbooruContext>(options => options.UseSqlite("Data Source=crbooru.db"));

// Model services
builder.Services.AddScoped<MediaAssetService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<PostService>();

// Authentication
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddAuthentication("CRbooruSession")
    .AddCookie("CRbooruSession", options =>
    {
        options.LoginPath = "/users/login";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
