namespace CRbooru.Models;

public class PostFormModel
{
    public int MediaAssetId { get; set; }
    public PostRating Rating { get; set; }
    public string TagString { get; set; }
}
