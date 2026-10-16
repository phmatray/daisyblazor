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

## API reference (generated)

The docs site's **API reference** pages (`website/src/content/docs/api/**`) are
generated from the .NET XML documentation comments, not hand-written. To regenerate
them locally:

```bash
dotnet build src/DaisyBlazor.Components/DaisyBlazor.Components.csproj -c Release
dotnet build src/DaisyBlazor.Charts/DaisyBlazor.Charts.csproj -c Release
node website/scripts/gen-api.mjs
```

`gen-api.mjs` requires a prior `dotnet build -c Release` (it reads the emitted
`*.xml` doc files under each project's `bin/Release/net10.0/`). It wipes and
recreates the `api/` directory on every run, so don't hand-edit those pages —
edit the C# XML doc comments instead. The [`docs`](.github/workflows/docs.yml)
workflow runs these same steps before building the site, so the published API
reference is always regenerated from source on deploy.

## Local checks before pushing

```bash
dotnet build           # builds everything incl. the gallery CSS
dotnet test            # bUnit component tests
./scripts/build-css.sh # (re)compile the gallery stylesheet
./scripts/pack.sh 0.1.1 # pack locally into ./artifacts
```
