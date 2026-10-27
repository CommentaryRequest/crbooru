using CRbooru;
using CRbooru.Data;
using Microsoft.EntityFrameworkCore;

namespace CRbooru.Tests;

public abstract class DatabaseLifetime(CustomWebApplicationFactory<Program> factory) : IAsyncLifetime
{
    protected CustomWebApplicationFactory<Program> Factory { get; } = factory;

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    public async Task InitializeAsync()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CRbooruContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected T GetService<T>(IServiceScope scope) where T : notnull => scope.ServiceProvider.GetRequiredService<T>();
}
