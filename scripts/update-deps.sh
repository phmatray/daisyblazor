#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# DaisyBlazor — update front-end (Tailwind/daisyUI) and .NET dependencies.
#
#   ./scripts/update-deps.sh            # update npm deps to latest, report .NET outdated
#   ./scripts/update-deps.sh --dotnet   # also bump .NET PackageVersion entries (interactive)
#
# The daisyUI version vendored into the NuGet preset is the one in the root package.json;
# after bumping it, run `node scripts/gen-styles.mjs` to re-vendor the bundles + safelist.
# The Tailwind CLI version the package downloads is DaisyBlazorTailwindVersion in
#   src/DaisyBlazor.Components/buildTransitive/DaisyBlazor.Components.targets
# ---------------------------------------------------------------------------
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

# Packages we manage on the front-end side.
NPM_DEV_PKGS=(tailwindcss @tailwindcss/cli daisyui)

bold() { printf '\033[1m%s\033[0m\n' "$*"; }

update_npm_project() {
  local dir="$1"
  [ -f "$dir/package.json" ] || return 0
  bold ">> npm: updating ${NPM_DEV_PKGS[*]} in $dir"
  ( cd "$dir" && npm install --save-dev "${NPM_DEV_PKGS[@]/%/@latest}" )
}

# 1. Front-end deps in every project that has a package.json.
while IFS= read -r pkg; do
  update_npm_project "$(dirname "$pkg")"
done < <(find . -name package.json -not -path '*/node_modules/*')

# 2. Re-vendor the daisyUI bundles at the new version and regenerate the safelist.
bold ">> regenerating the NuGet preset (scripts/gen-styles.mjs)"
node scripts/gen-styles.mjs

# 3. .NET dependencies (Central Package Management).
bold ">> dotnet: outdated NuGet packages"
dotnet restore DaisyBlazor.slnx >/dev/null 2>&1 || true
dotnet list DaisyBlazor.slnx package --outdated || true

if [[ "${1:-}" == "--dotnet" ]]; then
  bold ">> Edit Directory.Packages.props to bump the versions listed above, then re-run 'dotnet restore'."
fi

bold "Done. Review changes, run ./scripts/build-css.sh, then 'dotnet build'."
