using CRbooru.Logical;
using CRbooru.Models;

namespace CRbooru.Tests.Unit.Logical;

public class PoolVersionMergerTests
{
    private static PoolVersion createVersion(List<int>? newState = null, string? newName = null, string? newDescription = null, bool descriptionChanged = false)
    {
        return new PoolVersion {
            UpdaterId = 1,
            PoolId = 1,
            NewState = newState ?? [],
            NewName = newName,
            NewDescription = newDescription,
            DescriptionChanged = descriptionChanged
        };
    }

    private static List<PoolVersion> createHistory(params PoolVersion[] versions)
    {
        return versions.ToList();
    }

    [Fact]
    public void Merge_StatesOnly_OverridesState()
    {
        var oldVersion = createVersion(newState: [1, 2, 3]);
        var newVersion = createVersion(newState: [2, 4, 5]);

        var result = PoolVersionMerger.Merge(oldVersion, newVersion, createHistory(oldVersion));

        Assert.Equal(newVersion.NewState, result.NewState);
    }

    [Fact]
    public void Merge_UnchangedState_CancelsChange()
    {
        var oldVersion = createVersion(newState: [1, 2, 3]);
        var newVersion = createVersion(newState: [1, 2, 3]);

        var result = PoolVersionMerger.Merge(oldVersion, newVersion, createHistory(oldVersion));
        
        Assert.Null(result.NewState);
    }

    [Fact]
    public void Merge_NameOnly_OverridesName()
    {
        var oldVersion = createVersion(newName: "new pool");
        var newVersion = createVersion(newName: "pool name");

        var result = PoolVersionMerger.Merge(oldVersion, newVersion, createHistory(oldVersion));

        Assert.NotNull(result.NewName);
        Assert.Equal("pool name", result.NewName);
    }

    [Fact]
    public void Merge_UnchangedName_CancelsChange()
    {
        var oldVersion = createVersion(newName: "new pool");
        var newVersion = createVersion(newName: "new pool");

        var result = PoolVersionMerger.Merge(oldVersion, newVersion, createHistory(oldVersion));
        
        Assert.Null(result.NewName);
    }
    
    [Fact]
    public void Merge_DescriptionOnly_OverridesDescription()
    {
        var oldVersion = createVersion(newDescription: "this is a pool", descriptionChanged: true);
        var newVersion = createVersion(newDescription: "new description", descriptionChanged: true);

        var result = PoolVersionMerger.Merge(oldVersion, newVersion, createHistory(oldVersion));
        
        Assert.True(result.DescriptionChanged);
        Assert.Equal("new description", result.NewDescription);
    }

    [Fact]
    public void Merge_UnchangedDescription_CancelsChange()
    {
        var oldVersion = createVersion(newDescription: "this is a pool", descriptionChanged: true);
        var newVersion = createVersion(newDescription: "this is a pool", descriptionChanged: true);

        var result = PoolVersionMerger.Merge(oldVersion, newVersion, createHistory(oldVersion));
        
        Assert.False(result.DescriptionChanged);
        Assert.Null(result.NewDescription);
    }
}