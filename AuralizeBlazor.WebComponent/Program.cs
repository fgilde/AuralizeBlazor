using System.Runtime.InteropServices.JavaScript;
using AuralizeBlazor;
using AuralizeBlazor.WebComponent;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.RegisterCustomElement<Auralizer>(Auralizer.SuggestedWebComponentName);

// auralizer.js publishes the url it was loaded from. All "./_content/..." module imports
// have to be resolved against that url and not against the page that embeds the component.
var baseUrl = JSHost.GlobalThis.GetPropertyAsJSObject("AuralizeWebComponent")?.GetPropertyAsString("baseUrl");
if (!string.IsNullOrEmpty(baseUrl))
{
    var jsRuntime = (IJSRuntime)builder.Services.Single(d => d.ServiceType == typeof(IJSRuntime)).ImplementationInstance!;
    builder.Services.AddSingleton<IJSRuntime>(new RebasingJSRuntime(jsRuntime, baseUrl));
}

await builder.Build().RunAsync();
