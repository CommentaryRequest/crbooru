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

using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<CRbooruContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    if (!db.Users.Any()) {
        var user = new User();
        user.Name = "CommentaryRequest";
        user.Role = UserRole.Admin;
        user.PasswordHash = passwordHasher.HashPassword(user, "12345");
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}

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
