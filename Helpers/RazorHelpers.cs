using Microsoft.AspNetCore.Mvc.Rendering;
using CRbooru.Models;

namespace CRbooru.Helpers;

public static class RazorHelpers
{
    public static string MediaAssetSize(this IHtmlHelper _, MediaAsset asset)
    {
        string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = asset.FileSize;

        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }

        return $"{asset.Width}x{asset.Height} {number:n2} {suffixes[counter]} .{asset.FileType}";
    }
}
