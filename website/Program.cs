using DaisyBlazor;
using DaisyBlazor.Docs;
using DaisyBlazor.Docs.Docs;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddDaisyBlazor();
builder.Services.AddSingleton<XmlDocs>();
builder.Services.AddSingleton<SearchIndex>();
builder.Services.AddSingleton<DocsJs>();

await builder.Build().RunAsync();
