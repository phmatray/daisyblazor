#!/usr/bin/env bash
# Screenshot a page of the running docs site: scripts/shot.sh <port> <path> <out.png> [width] [height]
# Start the site first: dotnet run --project website --urls http://localhost:<port>
set -euo pipefail
port=$1 path=$2 out=$3 w=${4:-1440} h=${5:-2400}
timeout 90 chromium --headless=new --disable-gpu --hide-scrollbars --window-size="$w,$h" \
  --virtual-time-budget=15000 --screenshot="$out" "http://localhost:$port/${path#/}" >/dev/null 2>&1
echo "$out"
