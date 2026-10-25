using CRbooru.Models;

namespace CRbooru.Tests;

public static class TestUtils
{
    public static User CreateUser(string name, UserRole role)
    {
        return new User {
            Name = name,
            Role = role
        };
    }

    public static MediaAsset CreateDummyMediaAsset()
    {
        return new MediaAsset(
            "abcdefabcdefabcdefabcdefabcdefab",
            "abcdefabcdefabcdefabcdefabcdefab",
            "jpg",
            1024,
            800,
            600);
    }

    public static Post CreatePost(User uploader, PostRating rating, string? source, int? parentId)
    {
        return new Post(uploader, CreateDummyMediaAsset(), rating, source, parentId);
    }
}
