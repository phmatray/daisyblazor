# Contributing & releasing

## Branch strategy

**`main`** is the only long-lived branch: always green, and all work lands there via PR. The
[`ci`](.github/workflows/ci.yml) workflow builds + tests every push and PR, and the
[`release-please`](.github/workflows/release-please.yml) workflow turns merged commits into releases
(see below).

Feature work uses short-lived branches off `main`, named by intent:

```
feat/<thing>     # new feature
fix/<thing>      # bug fix
chore/<thing>    # tooling, docs, deps
```

Open a PR into `main` with a [Conventional Commits](https://www.conventionalcommits.org/) title
(`feat: …`, `fix(dialog): …`, `chore(deps): …`) and squash-merge once CI is green. The PR title
becomes the commit on `main`, and it is what decides the next version.

## Versioning & releases

Releases are automated with [release-please](https://github.com/googleapis/release-please). The
version is single-sourced in **`Directory.Build.props`** (`<Version>`), so all NuGet packages release
in lockstep; release-please owns it (and the template's package references) through the
`x-release-please-version` markers — don't bump it by hand.

1. Every push to `main` runs the **release-please** workflow, which keeps a release PR open with the
   next version and the `CHANGELOG.md` entry (pre-1.0: `feat` → minor, `fix` → patch).
2. Merging that release PR tags `v<version>` and creates the GitHub release. The same workflow then:
   - runs the tests,
   - packs `DaisyBlazor.Components`, `DaisyBlazor.Charts`, and `DaisyBlazor.Templates`,
   - pushes them to NuGet (`--skip-duplicate`, so re-runs are safe),
   - attaches the `.nupkg` files to the release.

## Required repository secrets

Set these under **Settings → Secrets and variables → Actions**:

| Secret | Used for | Notes |
|--------|----------|-------|
| `NUGET_API_KEY` | `dotnet nuget push` | nuget.org API key with **Push → "Push new packages and package versions"** and glob pattern `*` (or `DaisyBlazor.*`). |

## Documentation site

`website/` is a Blazor WebAssembly app built with DaisyBlazor itself, deployed to
GitHub Pages by the [`docs`](.github/workflows/docs.yml) workflow.

```bash
dotnet run --project website   # http://localhost:5000 — builds its CSS with the NuGet's Tailwind CLI
```

- A component page is one `ComponentDoc` entry in `website/Docs/Registry/<Category>.cs`
  plus its examples in `website/Examples/<Category>/`. Each example is a Razor component;
  the page shows it live and embeds its exact source, so the code shown is the code that runs.
- The **API tables** are reflected at runtime from the `[Parameter]` properties, with the
  summaries read from the libraries' XML doc comments: document parameters in C#, not in the site.
- The **playground** turns every simple parameter (enum, bool, string, number) into a control;
  opt in with a `PlaygroundSpec` on the `ComponentDoc`.

## Local checks before pushing

```bash
dotnet build           # builds everything incl. the docs site and its CSS
dotnet test            # bUnit component tests
./scripts/pack.sh 0.1.1 # pack locally into ./artifacts
```
