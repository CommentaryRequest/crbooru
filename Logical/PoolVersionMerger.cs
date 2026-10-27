using CRbooru.Models;

namespace CRbooru.Logical;

public static class PoolVersionMerger
{
    public static List<int>? GetLastState(List<PoolVersion> versions, int index)
    {
        if (index < 1) {
            return versions[0].NewState;
        }

        for (int i = index - 1; i >= 0; i--) {
            PoolVersion v = versions[i];
            if (v.NewState != null) {
                return v.NewState;
            }
        }

        return null;
    }

    public static string? GetLastName(List<PoolVersion> versions, int index)
    {
        if (index < 1) {
            return versions[0].NewName;
        }
        
        for (int i = index - 1; i >= 0; i--) {
            PoolVersion v = versions[i];
            if (v.NewName != null) {
                return v.NewName;
            }
        }

        return null;
    }
    
    public static string? GetLastDescription(List<PoolVersion> versions, int index)
    {
        if (index < 1) {
            return versions[0].NewDescription;
        }
        
        for (int i = index - 1; i >= 0; i--) {
            PoolVersion v = versions[i];
            if (v.DescriptionChanged) {
                return v.NewDescription;
            }
        }

        return null;
    }
    
    public static PoolVersion Merge(PoolVersion oldVersion, PoolVersion newVersion, List<PoolVersion> versions)
    {
        var index = versions.IndexOf(oldVersion);
        if (index == -1) { 
            throw new ArgumentException("Old version not present in versions list");
        }
        
        PoolVersion result = new(oldVersion);

        if (newVersion.NewState != null) {
            result.NewState = newVersion.NewState;

            var lastState = GetLastState(versions, index);
            if (lastState != null && result.NewState.SequenceEqual(lastState)) {
                result.NewState = null;
            }
        }

        if (newVersion.NewName != null) {
            result.NewName = newVersion.NewName;

            var lastName = GetLastName(versions, index);
            if (result.NewName == lastName) {
                result.NewName = null;
            }
        }

        if (newVersion.DescriptionChanged) {
            result.NewDescription = newVersion.NewDescription;
            result.DescriptionChanged = true;

            var lastDescription = GetLastDescription(versions, index);
            if (result.NewDescription == lastDescription) {
                result.NewDescription = null;
                result.DescriptionChanged = false;
            }
        }

        return result;
    }
}