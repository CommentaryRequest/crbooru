using Microsoft.AspNetCore.Http;

namespace CRbooru.Models;

public class UploadFormModel
{
    public required List<IFormFile> Files { get; set; }
}
