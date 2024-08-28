using Microsoft.AspNetCore.Components;

namespace AuralizeBlazor.Sample.Helper;

internal static class NavigationManagerExt
{
    public static bool FullPaged(this NavigationManager nav) => nav.Uri.Contains("?full") || nav.Uri.Contains("eyirish");
}