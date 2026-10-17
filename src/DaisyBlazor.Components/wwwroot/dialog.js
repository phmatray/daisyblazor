// DialogProvider interop: promote the server-rendered <dialog open> to a real modal
// (top layer, inert background, focus moved in, native Escape) and restore focus on close.

export function open(dialog) {
    if (!dialog || typeof dialog.showModal !== "function") return;
    dialog.__daisyReturnFocus = document.activeElement;
    // Escape is handled by the component's keydown handler (it honours CloseOnEscapeKey). Cancel the
    // native close request: Chrome ignores preventDefault on `cancel` without fresh user activation,
    // so a second Escape would otherwise close the <dialog> behind Blazor's back.
    if (!dialog.__daisyEscape) {
        dialog.__daisyEscape = true;
        dialog.addEventListener("keydown", e => { if (e.key === "Escape") e.preventDefault(); });
    }
    if (dialog.open) dialog.close();
    dialog.showModal();
}

export function close(dialog) {
    if (!dialog) return;
    const target = dialog.__daisyReturnFocus;
    dialog.__daisyReturnFocus = null;
    if (dialog.open) dialog.close();
    if (target && typeof target.focus === "function" && target.isConnected) target.focus();
}
