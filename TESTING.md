# Testing Guide

This document describes how to run tests for the Supabase F# library.

## Test Types

### Unit Tests
- `AsyncExtensionsTests.fs` - Unit tests for async extensions
- `OptionExtensionsTests.fs` - Unit tests for option extensions
- `SupabaseBuilderTests.fs` - Unit tests for Supabase builder
- `SupabaseModuleTests.fs` - Unit tests for Supabase module

### Property-Based Tests
- `AsyncExtensionsPropertyTests.fs` - Property-based tests for async extensions using FsCheck
- `OptionExtensionsPropertyTests.fs` - Property-based tests for option extensions using FsCheck
- `SupabaseIntegrationPropertyTests.fs` - Property-based integration tests with live Supabase instance

## Running Tests Locally

### Prerequisites

1. **Install Supabase CLI**
   ```bash
   npm install -g supabase
   ```

2. **Install 1Password CLI** (if using `op run` for secure environment variable management)
   ```bash
   # macOS
   brew install --cask 1password-cli

   # Other platforms: https://developer.1password.com/docs/cli/get-started/
   ```

3. **Setup Local Environment**

   Create a `.env` file in the project root based on `.env.sample`:
   ```bash
   cp .env.sample .env
   ```

### Option 1: Running with Local Supabase (Recommended)

1. **Start Local Supabase**
   ```bash
   supabase start
   ```

   This will output credentials including:
   - API URL (typically `http://127.0.0.1:54321`)
   - Service Role Key

2. **Update .env file**
   ```env
   SUPABASE_TEST_URL=http://127.0.0.1:54321
   SUPABASE_TEST_SERVICE_ROLE_KEY=<your-service-role-key-from-supabase-start>
   ```

3. **Run Tests**

   - Supabase CLI (`supabase start` 済み) が使える環境では、単純に `dotnet test` を実行するだけで OK です。環境変数が未設定の場合はテストコード内で `supabase status -o env` と `supabase migration up --local --yes` を呼び出して補完します。

   - 既存の `.env` を使いたい場合は、従来どおり `op run --env-file=.env -- dotnet test` や `export ... && dotnet test` でも動きます。

4. **CI と同じ流れをまとめて実行したい場合**

   Supabase を `supabase start` で起動した後、以下のスクリプトでスキーマ作成～テスト実行までを自動化できます:

   ```bash
   ./scripts/run-local-supabase-tests.sh
   ```

   このスクリプトは GitHub Actions と同じように `supabase migration up --local --yes` を実行した後、`supabase status -o env` から取得した URL/Key を使って `dotnet test` を実行します。

### Option 2: Running with Remote Supabase

1. **Get Credentials from Supabase Dashboard**
   - Project URL: `https://<project-ref>.supabase.co`
   - Service Role Key: Available in Project Settings > API

2. **Update .env file**
   ```env
   SUPABASE_TEST_URL=https://<project-ref>.supabase.co
   SUPABASE_TEST_SERVICE_ROLE_KEY=<your-service-role-key>
   ```

3. **Run Tests**
   ```bash
   op run --env-file=.env -- dotnet test
   ```

## Running Specific Test Categories

### Run only unit tests (fast, no Supabase required)
```bash
dotnet test --filter "FullyQualifiedName!~Integration"
```

### Run only integration tests (requires Supabase)
```bash
op run --env-file=.env -- dotnet test --filter "FullyQualifiedName~Integration"
```

### Run only property-based tests
```bash
op run --env-file=.env -- dotnet test --filter "FullyQualifiedName~Property"
```

## GitHub Actions

The tests are automatically run on GitHub Actions when:
- Pushing to `master` branch
- Creating a pull request to `master` branch
- Manually triggering the workflow

The GitHub Actions workflow:
1. Starts a local Supabase instance
2. Extracts credentials from Supabase
3. Sets up the test database schema
4. Runs all tests including integration tests

### Required GitHub Secrets

No secrets are required for the default workflow as it uses a local Supabase instance. However, if you want to test against a remote Supabase instance:

1. Add these secrets to your repository:
   - `SUPABASE_TEST_URL`
   - `SUPABASE_TEST_SERVICE_ROLE_KEY`

2. Update `.github/workflows/test.yaml` to use these secrets instead of local Supabase.

## Test Database Schema

The integration tests use a `test_items` table with the following schema:

```sql
CREATE TABLE test_items (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    value INTEGER NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

This table is automatically created by:
- Supabase migrations in `supabase/migrations/1751173700_test_items_table.sql` (local)
- GitHub Actions workflow setup step (CI)

## Troubleshooting

### Integration tests are skipped
- **Cause**: Environment variables `SUPABASE_TEST_URL` or `SUPABASE_TEST_SERVICE_ROLE_KEY` are not set
- **Solution**: Ensure `.env` file is properly configured and using `op run --env-file=.env` or export variables manually

### Tests fail with connection errors
- **Local Supabase**: Ensure `supabase start` was run successfully
- **Remote Supabase**: Check your network connection and credentials

### Tests fail with "table does not exist"
- **Local Supabase**: Run `supabase db reset` to apply migrations
- **Remote Supabase**: Run migrations manually or use Supabase Dashboard

### Property-based tests timeout
- Property-based tests generate multiple test cases (default: 5-100 per property)
- If tests timeout, the Supabase instance might be slow
- Consider reducing `MaxTest` attribute value in test properties

## Test Coverage

To generate test coverage reports:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Best Practices

1. **Always clean up test data**: Integration tests clean up after themselves, but manual testing should too
2. **Use local Supabase for development**: Faster and doesn't consume production quota
3. **Run unit tests before integration tests**: Catch basic issues quickly
4. **Use `op run` for secure credential management**: Never commit `.env` file with real credentials
