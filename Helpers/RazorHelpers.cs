using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using CRbooru.Models;

namespace CRbooru.Helpers;

public static class RazorHelpers
{
    public static IHtmlContent UserLink(this IHtmlHelper html, User user)
    {
        return html.ActionLink(user.Name, "Show", "User", new { id = user.Id });
    }
}
