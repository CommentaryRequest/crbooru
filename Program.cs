using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using CRbooru.Models;
using CRbooru.Data;
using CRbooru.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Configure database
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<CRbooruContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddSingleton(new StorageOptions(Path.Combine(builder.Environment.ContentRootPath, "Storage", "Images")));

// Model services
builder.Services.AddScoped<MediaAssetService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<UploadService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<PostUpdateService>();
builder.Services.AddScoped<PostVersionService>();
builder.Services.AddScoped<PoolService>();
builder.Services.AddScoped<PoolUpdateService>();
builder.Services.AddScoped<PoolPostService>();
builder.Services.AddScoped<PoolVersionService>();

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

app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "Assets")
    ),
    RequestPath = ""
});

app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "Storage", "Images")
    ),
    RequestPath = "/images"
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
