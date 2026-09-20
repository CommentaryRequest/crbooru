namespace CRbooru.Models;

public enum UploadStatus : byte
{
    Pending,
    Success,
    Error
}

public class Upload
{
    public int Id { get; set; }
    public User Uploader { get; set; }
    public UploadStatus Status { get; set; } = UploadStatus.Pending;
    public List<MediaAsset> MediaAssets { get; set; }
    public string? StatusMessage { get; set; }
}
