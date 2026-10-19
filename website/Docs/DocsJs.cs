using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DaisyBlazor.Docs.Docs;

/// <summary>Thin wrapper over <c>wwwroot/js/docs.js</c>.</summary>
public sealed class DocsJs(IJSRuntime js) : IAsyncDisposable
{
    private Task<IJSObjectReference>? _module;

    private Task<IJSObjectReference> Module => _module ??= js.InvokeAsync<IJSObjectReference>("import", "./js/docs.js").AsTask();

    public async Task<IJSObjectReference?> RegisterShortcuts<T>(DotNetObjectReference<T> self) where T : class =>
        await (await Module).InvokeAsync<IJSObjectReference?>("registerShortcuts", self);

    public async Task<IJSObjectReference?> ObserveSections<T>(DotNetObjectReference<T> self, IEnumerable<string> ids) where T : class =>
        await (await Module).InvokeAsync<IJSObjectReference?>("observeSections", self, ids);

    public async Task BeginThemeTransition(double x, double y) =>
        await (await Module).InvokeVoidAsync("beginThemeTransition", x, y);

    public async Task<bool> Copy(string text) => await (await Module).InvokeAsync<bool>("copy", text);

    public async Task ShowModal(ElementReference el) => await (await Module).InvokeVoidAsync("showModal", el);

    public async Task CloseModal(ElementReference el) => await (await Module).InvokeVoidAsync("closeModal", el);

    public async Task Focus(ElementReference el) => await (await Module).InvokeVoidAsync("focus", el);

    public async Task ScrollIntoView(ElementReference el) => await (await Module).InvokeVoidAsync("scrollIntoView", el);

    public async Task RevealActiveLink(ElementReference container) => await (await Module).InvokeVoidAsync("revealActiveLink", container);

    public async Task ScrollToTop() => await (await Module).InvokeVoidAsync("scrollToTop");

    public async ValueTask DisposeAsync()
    {
        if (_module is { IsCompletedSuccessfully: true }) await _module.Result.DisposeAsync();
    }
}
