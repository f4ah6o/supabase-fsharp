# GitHub Actions Workflow Manual Updates Required

Due to GitHub App permission restrictions, the following changes to `.github/workflows/test.yaml` need to be applied manually:

## Changes Required

Replace the section starting from `- name: Start Supabase` to the end of the file with:

```yaml
      - name: Start Supabase
        run: supabase start

      - name: Get Supabase credentials
        id: supabase-creds
        run: |
          echo "SUPABASE_TEST_URL=$(supabase status -o env | grep API_URL | cut -d '=' -f 2)" >> $GITHUB_ENV
          echo "SUPABASE_TEST_SERVICE_ROLE_KEY=$(supabase status -o env | grep SERVICE_ROLE_KEY | cut -d '=' -f 2)" >> $GITHUB_ENV

      - name: Setup test database schema
        run: |
          # Create test_items table for integration tests
          supabase db execute "
            CREATE TABLE IF NOT EXISTS test_items (
              id SERIAL PRIMARY KEY,
              name TEXT NOT NULL,
              value INTEGER NOT NULL DEFAULT 0,
              is_active BOOLEAN NOT NULL DEFAULT false,
              created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            -- Enable RLS but allow all operations for testing
            ALTER TABLE test_items ENABLE ROW LEVEL SECURITY;
            CREATE POLICY IF NOT EXISTS \"Allow all operations for service role\"
              ON test_items FOR ALL
              USING (true)
              WITH CHECK (true);
          "

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: |
          dotnet build Supabase.FSharp/Supabase.FSharp.fsproj --configuration Release --no-restore
          dotnet build Supabase.FSharp.Tests/Supabase.FSharp.Tests.fsproj --configuration Release --no-restore

      - name: Test
        run: dotnet test Supabase.FSharp.Tests/Supabase.FSharp.Tests.fsproj --configuration Release --no-restore
        env:
          SUPABASE_TEST_URL: ${{ env.SUPABASE_TEST_URL }}
          SUPABASE_TEST_SERVICE_ROLE_KEY: ${{ env.SUPABASE_TEST_SERVICE_ROLE_KEY }}
```

## What These Changes Do

1. **Get Supabase credentials**: Extracts the API URL and service role key from the local Supabase instance
2. **Setup test database schema**: Creates the `test_items` table required for integration tests
3. **Pass environment variables to tests**: Ensures the test runner has access to Supabase credentials

## Alternative: Use the Migration File

Instead of creating the table in the workflow, you can rely on the migration file (`supabase/migrations/1751173700_test_items_table.sql`). In that case, you only need to add the credential extraction and environment variable passing:

```yaml
      - name: Start Supabase
        run: supabase start

      - name: Get Supabase credentials
        id: supabase-creds
        run: |
          echo "SUPABASE_TEST_URL=$(supabase status -o env | grep API_URL | cut -d '=' -f 2)" >> $GITHUB_ENV
          echo "SUPABASE_TEST_SERVICE_ROLE_KEY=$(supabase status -o env | grep SERVICE_ROLE_KEY | cut -d '=' -f 2)" >> $GITHUB_ENV

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: |
          dotnet build Supabase.FSharp/Supabase.FSharp.fsproj --configuration Release --no-restore
          dotnet build Supabase.FSharp.Tests/Supabase.FSharp.Tests.fsproj --configuration Release --no-restore

      - name: Test
        run: dotnet test Supabase.FSharp.Tests/Supabase.FSharp.Tests.fsproj --configuration Release --no-restore
        env:
          SUPABASE_TEST_URL: ${{ env.SUPABASE_TEST_URL }}
          SUPABASE_TEST_SERVICE_ROLE_KEY: ${{ env.SUPABASE_TEST_SERVICE_ROLE_KEY }}
```

The migration will be automatically applied when `supabase start` is run.

## How to Apply

1. Edit `.github/workflows/test.yaml` manually in your repository
2. Commit the changes
3. The integration tests will run automatically on the next push or PR

You can also view the current unstaged changes in your local repository:
```bash
git diff .github/workflows/test.yaml
```
