# DaisyBlazor.Charts.Server

Server rendering for [DaisyBlazor.Charts](https://www.nuget.org/packages/DaisyBlazor.Charts).

DaisyBlazor charts compute their geometry with D3 layout modules. In the browser they run in the
page; this package runs the same modules on the server with [Jint](https://github.com/sebastienros/jint),
a JavaScript interpreter written in .NET. No Node.js is needed.

```csharp
builder.Services.AddDaisyBlazorChartsServerRendering();
```

With it registered, charts render their finished SVG during prerendering and static server
rendering, instead of a placeholder. Interactive charts lay out again in the browser.

The first layout parses the D3 bundle (about half a second); later layouts take a few milliseconds.
Each concurrent request uses its own engine from a pool.
