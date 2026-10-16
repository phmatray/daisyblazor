#!/usr/bin/env bash
# Builds the `dotnet new daisyblazor` app against freshly packed local packages, the way a consumer
# would: NuGet only, CSS compiled by the package's Tailwind standalone CLI target (no Node/npm).
#   ./scripts/test-template.sh
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
WORK="$ROOT/obj/template-test"
VERSION="0.0.0-local"
rm -rf "$WORK" && mkdir -p "$WORK/feed"

for proj in src/DaisyBlazor.Components/DaisyBlazor.Components.csproj src/DaisyBlazor.Charts/DaisyBlazor.Charts.csproj; do
  dotnet pack "$ROOT/$proj" -c Release -o "$WORK/feed" -p:Version="$VERSION" > "$WORK/pack.log" 2>&1 \
    || { cat "$WORK/pack.log"; exit 1; }
done

cp -r "$ROOT/templates/DaisyBlazor.Templates/templates/daisyblazor" "$WORK/App"
rm -rf "$WORK/App/.template.config"
sed -i.bak -E "s/(Include=\"DaisyBlazor\.(Components|Charts)\" Version=)\"[^\"]+\"/\1\"$VERSION\"/" "$WORK/App/DaisyBlazor.App.csproj"
# Isolate from the repo's Directory.*.props (central package management, versioning).
for f in Directory.Build.props Directory.Build.targets Directory.Packages.props; do echo '<Project />' > "$WORK/$f"; done
cat > "$WORK/nuget.config" <<EOF
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="$WORK/feed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
EOF

NUGET_PACKAGES="$WORK/packages" dotnet build "$WORK/App" -c Release
test -s "$WORK/App/wwwroot/css/app.css"
grep -q 'navbar' "$WORK/App/wwwroot/css/app.css"
echo "template OK: $(wc -c < "$WORK/App/wwwroot/css/app.css") bytes of CSS"
