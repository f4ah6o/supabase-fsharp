# Supabase F# Example

This project provides comprehensive usage examples of the Supabase.FSharp library, demonstrating various Supabase operations using F#-specific features.

## Overview

This sample demonstrates how to use the following Supabase features:

1. **Client Creation and Initialization** - Configuration using Computation Expressions
2. **Authentication** - Sign up, sign out, and user management
3. **Database Operations** - CRUD operations using PostgREST
4. **Realtime** - Real-time communication via WebSockets
5. **Storage** - File upload and download
6. **RPC (Remote Procedure Call)** - Calling database functions
7. **Computation Expressions** - Workflows using F# computation expressions
8. **Pipeline Style** - Operations leveraging F# pipeline operators

## Requirements

- .NET 9.0 or later
- F# 9.0 or later
- Active Supabase project

## Supabase Project Setup

Before running this sample, you need to configure your Supabase project as follows:

### 1. Create a Supabase Project

1. Go to [Supabase](https://supabase.com/) and create an account
2. Create a new project
3. Get your Project URL and API Key
   - Navigate to Dashboard > Settings > API and note:
     - `Project URL` (e.g., `https://xxxxx.supabase.co`)
     - `anon/public` key (API key for anonymous access)

### 2. Create Database Tables

Execute the following SQL in the Supabase SQL Editor:

#### Movies Table

```sql
-- Create movies table
CREATE TABLE movies (
    id BIGSERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Enable Row Level Security (RLS)
ALTER TABLE movies ENABLE ROW LEVEL SECURITY;

-- Grant read access to all users (authenticated and anonymous)
CREATE POLICY "Allow read access to all users"
ON movies
FOR SELECT
TO authenticated, anon
USING (true);

-- Insert sample data
INSERT INTO movies (name) VALUES
    ('The Matrix'),
    ('Inception'),
    ('Interstellar');
```

#### Users Table

```sql
-- Create users table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username TEXT UNIQUE NOT NULL,
    email TEXT UNIQUE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Enable Row Level Security (RLS)
ALTER TABLE users ENABLE ROW LEVEL SECURITY;

-- Users can read their own data
CREATE POLICY "Users can read own data"
ON users
FOR SELECT
TO authenticated
USING (auth.uid() = id);

-- Users can update their own data
CREATE POLICY "Users can update own data"
ON users
FOR UPDATE
TO authenticated
USING (auth.uid() = id);
```

### 3. Create Storage Bucket (Optional)

If you want to use the storage features:

#### Step 1: Create Bucket

1. Go to Supabase Dashboard > Storage
2. Click "Create a new bucket"
3. Create a bucket named `test-bucket`
4. Configure bucket settings:
   - **Public bucket**: OFF (recommended for security)
   - We'll set up RLS policies instead

#### Step 2: Set Row Level Security Policies

Go to Supabase Dashboard > SQL Editor and run the following SQL:

```sql
-- Policy 1: Allow authenticated users to upload files
CREATE POLICY "Allow authenticated uploads"
ON storage.objects
FOR INSERT
TO authenticated
WITH CHECK (bucket_id = 'test-bucket');

-- Policy 2: Allow authenticated users to update their files
CREATE POLICY "Allow authenticated updates"
ON storage.objects
FOR UPDATE
TO authenticated
USING (bucket_id = 'test-bucket');

-- Policy 3: Allow authenticated users to delete their files
CREATE POLICY "Allow authenticated deletes"
ON storage.objects
FOR DELETE
TO authenticated
USING (bucket_id = 'test-bucket');

-- Policy 4: Allow public read access to all files
CREATE POLICY "Allow public read"
ON storage.objects
FOR SELECT
TO public
USING (bucket_id = 'test-bucket');
```

**Alternative: Public Bucket (for development only)**

If you prefer to allow unrestricted access for testing:

1. Go to Supabase Dashboard > Storage
2. Select `test-bucket`
3. Click "Settings"
4. Toggle "Public bucket" to ON
5. This will allow anyone to upload/download files without authentication

### 4. Create RPC Function (Optional)

To use the RPC feature, create the following SQL function:

```sql
-- Sample RPC function
CREATE OR REPLACE FUNCTION hello_world(name TEXT)
RETURNS JSON AS $$
BEGIN
    RETURN json_build_object(
        'message', 'Hello, ' || name || '!',
        'timestamp', NOW()
    );
END;
$$ LANGUAGE plpgsql;

-- Grant execute permission to anonymous users
GRANT EXECUTE ON FUNCTION hello_world(TEXT) TO anon;
```

### 5. Authentication Settings

1. Go to Supabase Dashboard > Authentication > Settings
2. Disable "Enable Email Confirmations" (for development environment)
   - This allows user registration without email confirmation
3. For production, enable proper authentication settings and email confirmation

### 6. Enable Realtime

1. Go to Supabase Dashboard > Database > Replication
2. Enable Realtime for the `movies` table
   - Select the table > Turn "Enable Realtime" ON

## Environment Variables

Set the following environment variables:

```bash
export SUPABASE_URL="https://your-project.supabase.co"
export SUPABASE_KEY="your-anon-key"
```

### Windows (PowerShell)

```powershell
$env:SUPABASE_URL="https://your-project.supabase.co"
$env:SUPABASE_KEY="your-anon-key"
```

### Windows (Command Prompt)

```cmd
set SUPABASE_URL=https://your-project.supabase.co
set SUPABASE_KEY=your-anon-key
```

### Using .env File

Create a `.env` file in the project root:

```env
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_KEY=your-anon-key
```

Note: If using a `.env` file, you need to load the environment variables before running.

## Building and Running

### 1. Restore Dependencies

```bash
dotnet restore
```

### 2. Build the Project

```bash
dotnet build
```

### 3. Run

```bash
dotnet run
```

Or from the project root directory:

```bash
dotnet run --project Examples/SupabaseExample.FSharp/SupabaseExample.FSharp.fsproj
```

## Sample Code Explanations

### Example 1: Client Creation and Initialization

Demonstrates how to configure and initialize a Supabase client using Computation Expressions.

```fsharp
let options =
    supabaseOptions {
        schema "public"
        autoRefreshToken true
        autoConnectRealtime false
    }
```

**Key Points:**
- Uses F#-specific `supabaseOptions` Computation Expression
- Retrieves connection info from environment variables
- Safe null handling using F# Option types

### Example 2: Authentication

Implementation example of user sign up, login, and sign out.

```fsharp
let! signUpResponse = Auth.signUp email password client
match Auth.currentUser client with
| Some user -> // User exists
| None -> // No user
```

**Key Points:**
- Uses F# Async workflows
- Safe user info retrieval with Option types
- Converts nullable types to Options like `EmailOption`

### Example 3: Database Operations

Query operations using PostgREST.

```fsharp
let moviesTable = Supabase.from<Movie> client
let! response = moviesTable.Get() |> Async.AwaitTask
```

**Key Points:**
- Type-safe model definition (`Movie` type)
- Conversion from Task to F# Async
- LINQ-style query interface

### Example 4: Realtime

Real-time communication using WebSockets.

```fsharp
do! Realtime.connect client
let channel = Realtime.channel "public:movies" client
```

**Key Points:**
- Connection management using F# Async
- Channel-based subscription

### Example 5: Storage

Storage operations including file upload, download, and deletion.

```fsharp
let! uploadResult = Storage.upload bucketId filePath fileContent client
let publicUrl = Storage.publicUrl bucketId filePath client
```

**Key Points:**
- Handling binary data
- Generating public URLs
- Retrieving file lists

### Example 6: RPC (Remote Procedure Call)

How to remotely call PostgreSQL functions.

```fsharp
let parameters = {| name = "F# Developer" |}
let! result = Supabase.rpc procedureName parameters client
```

**Key Points:**
- Passing parameters using anonymous records
- Executing database functions

### Example 7: Computation Expressions

Building workflows using F# computation expressions.

```fsharp
let! result =
    auth {
        let! session = Auth.retrieveSession client
        // Workflow processing
        return Ok "Authenticated"
    }
```

**Key Points:**
- `auth` Computation Expression
- Error handling using Result types
- F#-idiomatic functional programming

### Example 8: Pipeline Style

Operations leveraging F# pipeline operators.

```fsharp
let session =
    client
    |> Auth.currentSession
    |> Option.defaultWith (fun () -> null)
```

**Key Points:**
- Uses pipeline operator `|>`
- Functional-style data transformation
- Combination with Option module functions

## Troubleshooting

### Authentication Error

```
Authentication error: Invalid credentials
```

**Solution:**
- Verify "Enable Email Confirmations" is disabled in your Supabase project
- Ensure email and password meet requirements (password minimum 6 characters)

### Database Connection Error

```
Database error: relation "movies" does not exist
```

**Solution:**
- Verify the `movies` table is created
- Check table name is correct (lowercase, plural)
- Verify Row Level Security (RLS) policies are correctly configured

### Movies Table Returns 0 Rows

```
Example 3: Database operations
Fetching movies...
✓ Found 0 movies
```

**Solution:**

This typically happens when RLS policies are misconfigured or data was deleted. Fix it by:

1. **Check if data exists:**
   ```sql
   -- Run in Supabase SQL Editor
   SELECT * FROM movies;
   ```

2. **Verify RLS policies:**
   ```sql
   -- Check existing policies
   SELECT policy_name, roles, qual
   FROM pg_policies
   WHERE tablename = 'movies';
   ```

3. **Fix RLS policies:**
   ```sql
   -- Remove old policy if it exists
   DROP POLICY IF EXISTS "Allow anonymous read access" ON movies;

   -- Create correct policy (allows both authenticated and anonymous users)
   CREATE POLICY "Allow read access to all users"
   ON movies
   FOR SELECT
   TO authenticated, anon
   USING (true);
   ```

4. **Re-insert data if needed:**
   ```sql
   INSERT INTO movies (name) VALUES
       ('The Matrix'),
       ('Inception'),
       ('Interstellar')
   ON CONFLICT DO NOTHING;
   ```

### Storage Error

```
Storage error: Bucket not found
```

**Solution:**
- Verify `test-bucket` is created in Supabase Dashboard
- Check bucket access permission settings

### Realtime Error

```
Realtime error: Connection failed
```

**Solution:**
- Verify Realtime is enabled for the table in your Supabase project
- Check WebSocket connections are not blocked by firewall

### Environment Variables Not Set

```
SUPABASE_URL and SUPABASE_KEY environment variables must be set
```

**Solution:**
- Verify environment variables are correctly set
- Set environment variables in the same terminal session before running

## Model Definitions

### Movie Model

```fsharp
[<Table("movies")>]
type Movie() =
    inherit BaseModel()

    [<PrimaryKey("id")>]
    member val Id = 0 with get, set

    [<Column("name")>]
    member val Name = "" with get, set

    [<Column("created_at")>]
    member val CreatedAt = System.DateTime.MinValue with get, set
```

### UserProfile Model

```fsharp
[<Table("users")>]
type UserProfile() =
    inherit BaseModel()

    [<PrimaryKey("id")>]
    member val Id = "" with get, set

    [<Column("username")>]
    member val Username = "" with get, set

    [<Column("email")>]
    member val Email = "" with get, set

    [<Column("created_at")>]
    member val CreatedAt = System.DateTime.MinValue with get, set
```

## References

- [Supabase Official Documentation](https://supabase.com/docs)
- [Supabase C# Client](https://github.com/supabase-community/supabase-csharp)
- [Supabase.FSharp Library](../../README.md)
- [F# Documentation](https://learn.microsoft.com/en-us/dotnet/fsharp/)

## Language

This README is available in multiple languages:
- [日本語 (Japanese)](README.ja.md)

## License

This sample code is provided under the MIT License.

## Next Steps

- For more complex examples, see [ContactApp](../ContactApp)
- For production use, configure appropriate authentication and authorization settings
- Adjust Row Level Security (RLS) policies to match your production requirements
