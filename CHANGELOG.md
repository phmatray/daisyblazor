# Changelog

## [0.5.1](https://github.com/phmatray/daisyblazor/compare/v0.5.0...v0.5.1) (2026-10-17)


### Bug Fixes

* **dialog:** keep a closing service dialog from being re-opened before it leaves the DOM ([#98](https://github.com/phmatray/daisyblazor/issues/98)) ([8581435](https://github.com/phmatray/daisyblazor/commit/85814358429021543d2c8ad11b89700526130946)), closes [#97](https://github.com/phmatray/daisyblazor/issues/97)

## [0.5.0](https://github.com/phmatray/daisyblazor/compare/v0.4.0...v0.5.0) (2026-10-17)


### ⚠ BREAKING CHANGES

* **build:** `DaisyBlazorBuildCss` is now incremental; files outside the project scanned by an `@source` line must be added to `DaisyBlazorCssSource` or editing them no longer rebuilds the CSS (closes #75)
* **form:** `Form.Validate()` and `IsValid` now check `Required` and `Validation` on `TextField`, `NumericField` and `Select`; forms that relied on the old no-op can now report invalid (closes #77)
* **theming:** `ThemeProvider` now falls back to the OS light/dark default (not always the light theme) when the stored theme is unknown
* **navmenulink:** `NavMenuLink` now renders its own `<li>`; remove any `<li>` you wrapped around it, otherwise `<li>` ends up nested in `<li>` (closes #78)
* **buttongroup:** `ButtonGroup.OverrideStyles` now defaults to `true` (as in MudBlazor), so a non-default group `Variant`, `Color` or `Size` is applied to its child buttons
* **colors:** `Color.Surface` no longer maps to the daisyUI `neutral` modifier; components keep their base (`base-100`/`base-200`) look
* **typography:** `Typography` h1–h6 no longer emit `font-heading`; add the class yourself if you relied on it

### Features

* **charts:** stacked AreaChart for part-of-whole series ([#79](https://github.com/phmatray/daisyblazor/issues/79)) ([#84](https://github.com/phmatray/daisyblazor/issues/84)) ([4019c9c](https://github.com/phmatray/daisyblazor/commit/4019c9cf2a2bd2cd5120b9f41ec2c6d5373d7b24))
* **dialogs:** add declarative Modal with [@bind](https://github.com/bind)-Open ([#96](https://github.com/phmatray/daisyblazor/issues/96)) ([eaaea55](https://github.com/phmatray/daisyblazor/commit/eaaea557a8fb13c70d34198a68aa501632b65701)), closes [#67](https://github.com/phmatray/daisyblazor/issues/67)
* **docs:** Blazor WebAssembly docs site + 255 visual bug fixes ([b2bc05a](https://github.com/phmatray/daisyblazor/commit/b2bc05ade27f09b6b88bda4261c0e5ab007bb281))
* **form:** `Form.Validate()` and `IsValid` now check `Required` and `Validation` on `TextField`, `NumericField` and `Select`; forms that relied on the old no-op can now report invalid (closes [#77](https://github.com/phmatray/daisyblazor/issues/77)) ([c4f3bd0](https://github.com/phmatray/daisyblazor/commit/c4f3bd0b7a821ef0db180205e08e46bcc44f6bea))
* **pageheader:** add SubtitleColor parameter ([#90](https://github.com/phmatray/daisyblazor/issues/90)) ([b2521d1](https://github.com/phmatray/daisyblazor/commit/b2521d1d3e5c7ad7e399b7c08eba551602dc7f50)), closes [#70](https://github.com/phmatray/daisyblazor/issues/70)
* **snackbar:** add Variant and class hooks to SnackbarProvider and SnackbarOptions ([#86](https://github.com/phmatray/daisyblazor/issues/86)) ([f22fa93](https://github.com/phmatray/daisyblazor/commit/f22fa93c95da970c30ae0f0b5a694c825aff7350)), closes [#68](https://github.com/phmatray/daisyblazor/issues/68)
* **tabs:** add Tab.TitleContent for icon and count labels ([#88](https://github.com/phmatray/daisyblazor/issues/88)) ([d57391f](https://github.com/phmatray/daisyblazor/commit/d57391f90f2fdc806ce05b61126616058a6b1838)), closes [#66](https://github.com/phmatray/daisyblazor/issues/66)


### Bug Fixes

* **build:** `DaisyBlazorBuildCss` is now incremental; files outside the project scanned by an `[@source](https://github.com/source)` line must be added to `DaisyBlazorCssSource` or editing them no longer rebuilds the CSS (closes [#75](https://github.com/phmatray/daisyblazor/issues/75)) ([aa9145b](https://github.com/phmatray/daisyblazor/commit/aa9145b0e38f6ca051a7f8b900e05ce98cb009c9))
* **build:** download fallback and `DaisyBlazorDaisyUIVersion` pin for the CSS build (closes [#72](https://github.com/phmatray/daisyblazor/issues/72), closes [#73](https://github.com/phmatray/daisyblazor/issues/73)) ([aa9145b](https://github.com/phmatray/daisyblazor/commit/aa9145b0e38f6ca051a7f8b900e05ce98cb009c9))
* **buttongroup:** `ButtonGroup.OverrideStyles` now defaults to `true` (as in MudBlazor), so a non-default group `Variant`, `Color` or `Size` is applied to its child buttons ([b2bc05a](https://github.com/phmatray/daisyblazor/commit/b2bc05ade27f09b6b88bda4261c0e5ab007bb281))
* **colors:** `Color.Surface` no longer maps to the daisyUI `neutral` modifier; components keep their base (`base-100`/`base-200`) look ([b2bc05a](https://github.com/phmatray/daisyblazor/commit/b2bc05ade27f09b6b88bda4261c0e5ab007bb281))
* **docs:** embed library XML docs on a clean build ([#95](https://github.com/phmatray/daisyblazor/issues/95)) ([669f6e0](https://github.com/phmatray/daisyblazor/commit/669f6e0f0e8fd6ff8224828e746904342ff8398e))
* **drawer:** JS fallback for static SSR and localisable overlay label ([#92](https://github.com/phmatray/daisyblazor/issues/92)) ([10aefe2](https://github.com/phmatray/daisyblazor/commit/10aefe23828c89f3525e5ca1cf7e0ccf02ae2773)), closes [#69](https://github.com/phmatray/daisyblazor/issues/69)
* **navmenulink:** `NavMenuLink` now renders its own `<li>`; remove any `<li>` you wrapped around it, otherwise `<li>` ends up nested in `<li>` (closes [#78](https://github.com/phmatray/daisyblazor/issues/78)) ([bb2cd0f](https://github.com/phmatray/daisyblazor/commit/bb2cd0fbb30333dcad2533f5579c06bdf75be533))
* **preset:** safelist font-heading and ship a default [@utility](https://github.com/utility) for it ([#89](https://github.com/phmatray/daisyblazor/issues/89)) ([cd48c39](https://github.com/phmatray/daisyblazor/commit/cd48c39280d930feff5b646f64541e2b0db546c2)), closes [#65](https://github.com/phmatray/daisyblazor/issues/65)
* **theming:** `data-themes` allow-list for `theme-init.js` (closes [#71](https://github.com/phmatray/daisyblazor/issues/71)) ([7bb4dca](https://github.com/phmatray/daisyblazor/commit/7bb4dca7dd90d64d2cfbe29ed22b807d6ed97db6))
* **theming:** `ThemeProvider` now falls back to the OS light/dark default (not always the light theme) when the stored theme is unknown ([7bb4dca](https://github.com/phmatray/daisyblazor/commit/7bb4dca7dd90d64d2cfbe29ed22b807d6ed97db6))
* **typography:** `Typography` h1–h6 no longer emit `font-heading`; add the class yourself if you relied on it ([b2bc05a](https://github.com/phmatray/daisyblazor/commit/b2bc05ade27f09b6b88bda4261c0e5ab007bb281))

## [0.4.0](https://github.com/phmatray/daisyblazor/compare/v0.3.0...v0.4.0) (2026-10-16)


### Features

* **preset:** ship the CSS preset and Tailwind build through NuGet only ([#62](https://github.com/phmatray/daisyblazor/issues/62)) ([592d029](https://github.com/phmatray/daisyblazor/commit/592d029edb668f5e4af3347e7dd1c1ff3d5c223d)), closes [#44](https://github.com/phmatray/daisyblazor/issues/44)
