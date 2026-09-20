namespace CRbooru.Models;

public class UploadViewModel
{
    public Upload Upload { get; set; }
    public MediaAsset MediaAsset { get; set; }

    public UploadViewModel(Upload upload, MediaAsset mediaAsset)
    {
        Upload = upload;
        MediaAsset = mediaAsset;
    }
}
