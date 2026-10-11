using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
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
builder.Services.AddScoped<UploadService>();
builder.Services.AddScoped<PostVersionService>();

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
    var postVersions = scope.ServiceProvider.GetRequiredService<PostVersionService>();
    if (!db.PostVersions.Any()) {
        var posts = await db.Posts
            .Include(p => p.Tags.OrderBy(tag => tag.Name))
            .Include(p => p.Uploader)
            .ToListAsync();
        foreach (var post in posts) {
            postVersions.Add(new PostVersion(post.Id, post.Uploader.Id, post.Tags.Select(tag => tag.Name).ToArray(), null, post.Rating, post.Source, true));
        }
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
