using Microsoft.AspNetCore.Components;

namespace DaisyBlazor;

/// <summary>
/// Internal record describing a single shown dialog: the component type to render, its
/// parameters, options, title and the reference whose <see cref="IDialogReference.Result"/>
/// completes when the dialog closes.
/// </summary>
public sealed class DialogReference : IDialogReference, IMudDialogInstance
{
    private readonly TaskCompletionSource<DialogResult> _completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public DialogReference(
        Guid id,
        string title,
        Type componentType,
        DialogParameters parameters,
        DialogOptions options)
    {
        Id = id;
        Title = title;
        ComponentType = componentType;
        Parameters = parameters;
        Options = options;
    }

    /// <summary>Unique id of this dialog instance.</summary>
    public Guid Id { get; }

    /// <summary>DOM id of the title element, referenced by the modal's <c>aria-labelledby</c>.</summary>
    public string TitleId => $"dialog-{Id:N}-title";

    /// <inheritdoc cref="IMudDialogInstance.Title" />
    public string Title { get; }

    /// <summary>Component type rendered inside the modal box.</summary>
    public Type ComponentType { get; }

    /// <summary>Parameters forwarded to the hosted component.</summary>
    public DialogParameters Parameters { get; }

    /// <summary>Display options for the modal.</summary>
    public DialogOptions Options { get; }

    /// <summary>Raised when this dialog requests to be removed from the provider.</summary>
    public event Action<DialogReference>? OnClose;

    /// <inheritdoc />
    public Task<DialogResult> Result => _completion.Task;

    /// <inheritdoc />
    public void Close() => Close(DialogResult.Ok());

    /// <inheritdoc cref="IDialogReference.Close(DialogResult)" />
    public void Close(DialogResult result)
    {
        _completion.TrySetResult(result);
        OnClose?.Invoke(this);
    }

    /// <inheritdoc />
    public void Cancel() => Close(DialogResult.Cancel());
}
