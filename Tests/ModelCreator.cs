using System.Security.Cryptography;
using CRbooru.Models;

namespace CRbooru.Tests;

public static class ModelCreator
{
    public static User CreateUser(string name, UserRole role)
    {
        return new User {
            Name = name,
            Role = role
        };
    }

    public static MediaAsset CreateDummyMediaAsset(string md5 = "")
    {
        if (string.IsNullOrWhiteSpace(md5)) {
            md5 = Convert.ToHexString(MD5.HashData(RandomNumberGenerator.GetBytes(32))).ToLowerInvariant();
        }

        return new MediaAsset(
            md5,
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
