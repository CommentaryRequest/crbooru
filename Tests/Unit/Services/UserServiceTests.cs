using System.Security.Claims;
using CRbooru.Models;
using CRbooru.Services;

namespace CRbooru.Tests.Unit.Services;

public class UserServiceTests
    : DatabaseLifetime,
      IClassFixture<CustomWebApplicationFactory<Program>>
{
    public UserServiceTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Add_ValidUser_AddsUserToDatabase()
    {
        using var scope = CreateScope();
        var service = GetService<UserService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await service.Add(user);
    }

    [Fact]
    public async Task Get_ValidId_ReturnsCorrectUser()
    {
        using var scope = CreateScope();
        var service = GetService<UserService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Privileged);
        await service.Add(user);

        var dbUser = await service.Get(1);
        Assert.NotNull(dbUser);
        Assert.Equal("NewUser", dbUser.Name);
        Assert.Equal(UserRole.Privileged, dbUser.Role);
    }

    [Fact]
    public async Task Get_InvalidId_ReturnsNull()
    {
        using var scope = CreateScope();
        var service = GetService<UserService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Privileged);
        await service.Add(user);

        var dbUser = await service.Get(10);
        Assert.Null(dbUser);
    }

    [Fact]
    public async Task Get_ValidName_ReturnsCorrectUser()
    {
        using var scope = CreateScope();
        var service = GetService<UserService>(scope);

        var user1 = ModelCreator.CreateUser("NewUser1", UserRole.Member);
        var user2 = ModelCreator.CreateUser("NewUser2", UserRole.Builder);
        await service.Add(user1);
        await service.Add(user2);

        var dbUser = await service.ByName("NewUser2");
        Assert.NotNull(dbUser);
        Assert.Equal(2, dbUser.Id);
        Assert.Equal("NewUser2", dbUser.Name);
        Assert.Equal(UserRole.Builder, dbUser.Role);
    }

    [Fact]
    public async Task Get_InvalidName_ReturnsNull()
    {
        using var scope = CreateScope();
        var service = GetService<UserService>(scope);

        var user = ModelCreator.CreateUser("NewUser2", UserRole.Builder);
        await service.Add(user);

        var dbUser = await service.ByName("thisuserdoesnotexist");
        Assert.Null(dbUser);
    }

    [Fact]
    public async Task GetCurrentUser_ValidClaimsPrincipal_ReturnsCurrentUser()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };

        var identity = new ClaimsIdentity(claims, authenticationType: "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        using var scope = CreateScope();
        var service = GetService<UserService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await service.Add(user);
        
        var dbUser = await service.GetCurrentUser(claimsPrincipal);
        Assert.NotNull(dbUser);
        Assert.Equal(1, dbUser.Id);
        Assert.Equal("NewUser", dbUser.Name);
        Assert.Equal(UserRole.Member, dbUser.Role);
    }

    [Fact]
    public async Task GetCurrentUser_InvalidClaimsPrincipal_ReturnsNull()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "2")
        };

        var identity = new ClaimsIdentity(claims, authenticationType: "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        using var scope = CreateScope();
        var service = GetService<UserService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await service.Add(user);
        
        var dbUser = await service.GetCurrentUser(claimsPrincipal);
        Assert.Null(dbUser);
    }
}
