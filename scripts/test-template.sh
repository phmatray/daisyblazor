#!/usr/bin/env bash
# Builds the `dotnet new daisyblazor` app against freshly packed local packages, the way a consumer
# would: NuGet only, CSS compiled by the package's Tailwind standalone CLI target (no Node/npm).
#   ./scripts/test-template.sh
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
WORK="$ROOT/obj/template-test"
VERSION="0.0.0-local"
rm -rf "$WORK" && mkdir -p "$WORK/feed"

for proj in src/DaisyBlazor.Components/DaisyBlazor.Components.csproj src/DaisyBlazor.Charts/DaisyBlazor.Charts.csproj src/DaisyBlazor.Charts.Server/DaisyBlazor.Charts.Server.csproj; do
  dotnet pack "$ROOT/$proj" -c Release -o "$WORK/feed" -p:Version="$VERSION" > "$WORK/pack.log" 2>&1 \
    || { cat "$WORK/pack.log"; exit 1; }
done

cp -r "$ROOT/templates/DaisyBlazor.Templates/templates/daisyblazor" "$WORK/App"
rm -rf "$WORK/App/.template.config"
sed -i.bak -E "s/(Include=\"DaisyBlazor\.(Components|Charts|Charts\.Server)\" Version=)\"[^\"]+\"/\1\"$VERSION\"/" "$WORK/App/DaisyBlazor.App.csproj"
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

# A fresh clone has no app.css at evaluation time: the build must still make it a static web asset,
# so a publish ships it (#107).
rm -rf "$WORK/App/wwwroot/css" "$WORK/App/obj" "$WORK/App/bin"
NUGET_PACKAGES="$WORK/packages" dotnet publish "$WORK/App" -c Release -o "$WORK/publish"
test -s "$WORK/publish/wwwroot/css/app.css"
grep -q '"Route":"css/app.css"' "$WORK/publish/DaisyBlazor.App.staticwebassets.endpoints.json" \
  || { echo "app.css is not a static web asset (no endpoint)"; exit 1; }
test -s "$WORK/publish/wwwroot/css/app.css.gz"
echo "publish OK: app.css shipped from a clean tree"

# Server rendering: the published app's first HTML must already hold the finished chart SVG (laid out
# by DaisyBlazor.Charts.Server), not the placeholder a chart shows while the browser lays it out.
PORT=5199
(cd "$WORK/publish" && ASPNETCORE_URLS="http://127.0.0.1:$PORT" exec dotnet DaisyBlazor.App.dll > "$WORK/run.log" 2>&1) &
APP=$!
trap 'kill $APP 2>/dev/null || true' EXIT
for _ in $(seq 1 60); do curl -sf "http://127.0.0.1:$PORT/" -o "$WORK/home.html" && break; sleep 1; done
test -s "$WORK/home.html" || { cat "$WORK/run.log"; echo "the published app did not answer"; exit 1; }
grep -q '<svg[^>]*role="img"' "$WORK/home.html" || { echo "no chart SVG in the server-rendered HTML"; exit 1; }
if grep -q 'aria-busy="true"' "$WORK/home.html"; then echo "a chart is still a placeholder in the server-rendered HTML"; exit 1; fi
echo "server rendering OK: $(grep -o '<svg[^>]*role="img"' "$WORK/home.html" | wc -l) chart(s) in the first HTML"
