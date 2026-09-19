using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CRbooru.Models;

public class Post
{
    public int Id { get; private set; }
    public User Uploader { get; private set; }
    public MediaAsset MediaAsset { get; set; }
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();

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
