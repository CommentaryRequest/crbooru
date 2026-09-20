using Microsoft.AspNetCore.Http;

namespace CRbooru.Models;

public class UploadFormModel
{
    public List<IFormFile> Files { get; set; }
}
