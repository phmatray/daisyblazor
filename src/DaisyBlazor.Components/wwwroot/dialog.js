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
    // showModal() focuses the first focusable descendant; in a content-only box that is the invisible
    // backdrop button (viewport-wide ring, "Close" announced). Focus the box so its title is read (#163).
    const box = dialog.querySelector(".modal-box");
    if (box && !box.contains(document.activeElement)) box.focus();
    // A dialog removed while open (e.g. an `@if` around a Modal) never gets close(): its ElementReference
    // no longer resolves, so watch for the removal and give focus back then (#101).
    dialog.__daisyRemoval?.disconnect();
    if (typeof MutationObserver === "function") {
        dialog.__daisyRemoval = new MutationObserver(() => { if (!dialog.isConnected) restoreFocus(dialog); });
        dialog.__daisyRemoval.observe(document.body, { childList: true, subtree: true });
    }
}

export function close(dialog) {
    if (!dialog) return;
    if (dialog.open) dialog.close();
    restoreFocus(dialog);
}

function restoreFocus(dialog) {
    dialog.__daisyRemoval?.disconnect();
    dialog.__daisyRemoval = null;
    const target = dialog.__daisyReturnFocus;
    dialog.__daisyReturnFocus = null;
    if (target && typeof target.focus === "function" && target.isConnected) target.focus();
}
