---
title: "Drawer"
description: "API reference for Drawer."
---

## Properties

### `ChildContent`

The main content rendered inside the drawer-content region.

### `End`

When true, the drawer slides in from the right end instead of the left.

### `Id`

Unique id for the hidden checkbox used to drive the CSS toggle. Defaults to a generated value.

### `Open`

Whether the drawer is currently open (two-way bindable).

### `OpenChanged`

Raised when the open state changes (toggle, overlay click, or Escape).

### `SideContent`

The panel content rendered inside the drawer-side region.

### `SideId`

Element id of the side panel; point a custom toggle's `aria-controls` at it.

### `ToggleClass`

CSS classes of the built-in toggle button (e.g. add `lg:hidden` for a responsive drawer).

### `ToggleContent`

Content of an accessible built-in toggle button rendered at the top of the drawer-content region
(a real button exposing aria-expanded/aria-controls). When set, the hidden checkbox leaves the tab order.

## Methods

### `SetOpenAsync(bool)`

Opens or closes the drawer and raises `OpenChanged`.

### `ToggleAsync`

Toggles the drawer open state.
