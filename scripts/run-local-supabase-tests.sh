#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT_DIR"

require_cmd() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "[error] '$1' コマンドが見つかりません。インストールしてください。" >&2
    exit 1
  fi
}

require_cmd supabase
require_cmd dotnet

# Ensure Supabase is running locally
if ! supabase status >/dev/null 2>&1; then
  echo "[error] Supabase ローカル環境が起動していません。先に 'supabase start' を実行してください。" >&2
  exit 1
fi

SUPABASE_STATUS=$(supabase status -o env)
SUPABASE_TEST_URL=$(echo "$SUPABASE_STATUS" | awk -F '=' '/API_URL/ {gsub(/"/, "", $2); print $2}')
SUPABASE_TEST_SERVICE_ROLE_KEY=$(echo "$SUPABASE_STATUS" | awk -F '=' '/SERVICE_ROLE_KEY/ {gsub(/"/, "", $2); print $2}')

if [[ -z "$SUPABASE_TEST_URL" || -z "$SUPABASE_TEST_SERVICE_ROLE_KEY" ]]; then
  echo "[error] Supabase 環境変数を取得できませんでした。supabase status -o env の出力を確認してください。" >&2
  exit 1
fi

TEST_SCHEMA_SQL=$(cat <<'SQL'
CREATE TABLE IF NOT EXISTS test_items (
  id SERIAL PRIMARY KEY,
  name TEXT NOT NULL,
  value INTEGER NOT NULL DEFAULT 0,
  is_active BOOLEAN NOT NULL DEFAULT false,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE test_items ENABLE ROW LEVEL SECURITY;
CREATE POLICY IF NOT EXISTS "Allow all operations for service role"
  ON test_items FOR ALL
  USING (true)
  WITH CHECK (true);
SQL
)

supabase db execute "$TEST_SCHEMA_SQL" >/dev/null

dotnet restore

dotnet build Supabase.FSharp/Supabase.FSharp.fsproj --configuration Release --no-restore

dotnet build Supabase.FSharp.Tests/Supabase.FSharp.Tests.fsproj --configuration Release --no-restore

SUPABASE_TEST_URL="$SUPABASE_TEST_URL" \
SUPABASE_TEST_SERVICE_ROLE_KEY="$SUPABASE_TEST_SERVICE_ROLE_KEY" \
dotnet test Supabase.FSharp.Tests/Supabase.FSharp.Tests.fsproj --configuration Release --no-restore
