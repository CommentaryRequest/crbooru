using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<CRbooruContext>(options => options.UseSqlite("Data Source=crbooru.db"));

var app = builder.Build();

// Create a new media asset for testing
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<CRbooruContext>();

    if (!db.MediaAssets.Any()) {
        db.MediaAssets.Add(new MediaAsset {
            Md5 = "32bb5f07a3c4b8cf75c93bb60c9f8082",
            FileType = "jpg"
        });

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
