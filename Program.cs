using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<CRbooruContext>(options => options.UseSqlite("Data Source=crbooru.db"));

// Model services
builder.Services.AddScoped<MediaAssetService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TagService>();

var app = builder.Build();

// Create test items for testing
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<CRbooruContext>();

    if (!db.MediaAssets.Any()) {
        db.MediaAssets.Add(new MediaAsset("32bb5f07a3c4b8cf75c93bb60c9f8082", "jpg"));
        await db.SaveChangesAsync();
    }

    if (!db.Users.Any()) {
        db.Users.Add(new User("CommentaryRequest"));
        await db.SaveChangesAsync();
    }

    if (!db.Tags.Any()) {
        db.Tags.Add(new Tag("1girl", 0, TagCategory.General, false));
        db.Tags.Add(new Tag("solo", 0, TagCategory.General, false));
        db.Tags.Add(new Tag("touhou", 0, TagCategory.Copyright, false));
        db.Tags.Add(new Tag("konpaku_youmu", 0, TagCategory.Character, false));
        db.Tags.Add(new Tag("kashuu", 0, TagCategory.Artist, false));
        db.Tags.Add(new Tag("commentary_request", 0, TagCategory.Meta, false));
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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
