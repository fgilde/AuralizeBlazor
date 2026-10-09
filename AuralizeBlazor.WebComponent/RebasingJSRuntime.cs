using Microsoft.JSInterop;

namespace AuralizeBlazor.WebComponent;

/// <summary>
/// Rewrites relative module imports ("./_content/...") to absolute urls below <paramref name="baseUrl"/>,
/// because Blazor resolves them against document.baseURI, which is the embedding page.
/// </summary>
internal sealed class RebasingJSRuntime(IJSRuntime inner, string baseUrl) : IJSInProcessRuntime
{
    private object?[]? Rebase(string identifier, object?[]? args)
    {
        if (identifier == "import" && args is [string url] && url.StartsWith("./"))
            return [baseUrl + url[2..]];
        return args;
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        => inner.InvokeAsync<TValue>(identifier, Rebase(identifier, args));

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        => inner.InvokeAsync<TValue>(identifier, cancellationToken, Rebase(identifier, args));

    public TValue Invoke<TValue>(string identifier, params object?[]? args)
        => ((IJSInProcessRuntime)inner).Invoke<TValue>(identifier, Rebase(identifier, args));
}
