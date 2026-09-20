using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CRbooru.Models;

public enum PostRating : byte
{
    General,
    Sensitive,
    Questionable,
    Explicit
}

public class Post
{
    public int Id { get; private set; }
    public User Uploader { get; private set; }
    public MediaAsset MediaAsset { get; set; }
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public PostRating Rating { get; set; } = PostRating.General;

    private Post()
    {
    }

    public Post(User uploader, MediaAsset mediaAsset, ICollection<Tag> tags)
    {
        Uploader = uploader;
        MediaAsset = mediaAsset;
        Tags = tags;
    }
}
