# Supabase.FSharp

F# extensions and idiomatic wrappers for the [Supabase C# client](https://github.com/supabase-community/supabase-csharp).

## Features

- **F# Async Integration** - Seamless conversion between Task and F# Async workflows
- **Option Type Support** - Automatic conversion of nullable types to F# options
- **Computation Expressions** - Idiomatic builders for configuration and workflows
- **Pipeline-Friendly Functions** - Module functions designed for F# pipelines
- **Type Extensions** - F#-friendly extensions for Supabase types

## Installation

```bash
dotnet add package Supabase.FSharp
```

## Quick Start

### Basic Client Setup

```fsharp
open Supabase.FSharp

// Create client with options using computation expression
let options =
    supabaseOptions {
        schema "public"
        autoRefreshToken true
        autoConnectRealtime false
    }

let url = "YOUR_SUPABASE_URL"
let key = "YOUR_SUPABASE_KEY"

// Create and initialize client
let client = async {
    let clientInterface = Supabase.create url key options
    let client = clientInterface :?> Supabase.Client
    do! Supabase.initialize client
    return client
}
```

### Authentication

```fsharp
open Supabase.FSharp

let authenticate (client: Supabase.Client) = async {
    // Sign up
    let! response = Auth.signUp "user@example.com" "password123" client

    // Get current user as Option
    match Auth.currentUser client with
    | Some user ->
        printfn "User ID: %s" user.Id
        // Access email safely with Option extension
        match user.EmailOption with
        | Some email -> printfn "Email: %s" email
        | None -> printfn "No email set"
    | None ->
        printfn "No user logged in"

    // Sign out
    do! Auth.signOut client
}
```

### Database Operations

```fsharp
open Supabase.FSharp
open Supabase.Postgrest.Models

[<Table("movies")>]
type Movie() =
    inherit BaseModel()

    [<PrimaryKey("id")>]
    member val Id = 0 with get, set

    [<Column("name")>]
    member val Name = "" with get, set

let queryMovies (client: Supabase.Client) = async {
    let table = Supabase.from<Movie> client
    let! response = table.Get() |> Async.AwaitTask

    for movie in response.Models do
        printfn "Movie: %s (ID: %d)" movie.Name movie.Id
}
```

### Realtime

```fsharp
open Supabase.FSharp

let setupRealtime (client: Supabase.Client) = async {
    // Connect to realtime
    do! Realtime.connect client

    // Get a channel
    let channel = Realtime.channel "public:movies" client

    // Disconnect when done
    do! Realtime.disconnect client
}
```

### Storage

```fsharp
open Supabase.FSharp

let storageOperations (client: Supabase.Client) = async {
    let bucketId = "my-bucket"
    let filePath = "example.txt"
    let fileContent = System.Text.Encoding.UTF8.GetBytes("Hello from F#!")

    // Upload file
    let! uploadResult = Storage.upload bucketId filePath fileContent client

    // Get public URL
    let publicUrl = Storage.publicUrl bucketId filePath client
    printfn "File URL: %s" publicUrl

    // List files
    let! files = Storage.list bucketId "" client
    printfn "Found %d files" files.Count

    // Download file
    let! downloadedBytes = Storage.download bucketId filePath client

    // Delete files
    do! Storage.delete bucketId [filePath] client
}
```

### Edge Functions

```fsharp
open Supabase.FSharp

let callFunction (client: Supabase.Client) = async {
    // Invoke function
    let! result = Functions.invoke "hello-world" client

    // Invoke with parameters
    let parameters = {| name = "F# Developer" |}
    let! result = Functions.invokeWith "greet" parameters client

    // Invoke with typed response
    let! typedResult = Functions.invokeTyped<MyResponseType> "my-function" client
}
```

### Remote Procedure Calls (RPC)

```fsharp
open Supabase.FSharp

let callRPC (client: Supabase.Client) = async {
    // Call database function
    let parameters = {| userId = 123 |}
    let! result = Supabase.rpc "my_function" parameters client

    // Call with typed response
    let! typedResult = Supabase.rpcTyped<MyType> "my_function" parameters client
}
```

## Module Functions

The library provides pipeline-friendly module functions:

### `Supabase` Module

- `create` - Create a new Supabase client
- `createDefault` - Create client with default options
- `initialize` - Initialize the client
- `from` - Get a table reference
- `rpc` - Call a remote procedure
- `rpcTyped` - Call a remote procedure with typed response

### `Auth` Module

- `signIn` - Sign in with email and password
- `signUp` - Sign up a new user
- `signOut` - Sign out the current user
- `currentSession` - Get current session as Option
- `currentUser` - Get current user as Option
- `retrieveSession` - Retrieve the session
- `refreshSession` - Refresh the session
- `resetPasswordForEmail` - Send password reset email
- `updateUser` - Update user attributes

### `Realtime` Module

- `connect` - Connect to Realtime
- `disconnect` - Disconnect from Realtime
- `setAuth` - Set auth token
- `channel` - Get a channel by name

### `Storage` Module

- `bucket` - Get a storage bucket
- `upload` - Upload a file
- `download` - Download a file
- `delete` - Delete files
- `list` - List files in a bucket
- `publicUrl` - Get public URL for a file

### `Functions` Module

- `invoke` - Invoke an edge function
- `invokeWith` - Invoke with parameters
- `invokeTyped` - Invoke with typed response
- `invokeTypedWith` - Invoke with parameters and typed response

## Computation Expressions

### Supabase Options Builder

```fsharp
let options =
    supabaseOptions {
        schema "public"
        autoRefreshToken true
        autoConnectRealtime false
        header "X-Custom-Header" "value"
    }
```

### Auth Workflow

```fsharp
let authFlow (client: Supabase.Client) =
    auth {
        let! session = Auth.retrieveSession client

        match ofObj session with
        | Some s ->
            match s.AccessTokenOption with
            | Some token -> return Ok token
            | None -> return Error "No token"
        | None ->
            return Error "No session"
    }
```

## Option Type Extensions

The library automatically converts nullable types to F# options:

```fsharp
open Supabase.FSharp

// Session extensions
let session = client.Auth.CurrentSession
session.AccessTokenOption  // string option
session.RefreshTokenOption // string option
session.UserOption         // User option

// User extensions
let user = client.Auth.CurrentUser
user.EmailOption          // string option
user.PhoneOption          // string option

// Helper functions
let maybeValue = ofObj someNullableReference  // 'T option
let maybeInt = ofNullable someNullableInt     // int option
```

## Async Extensions

Seamless conversion between Task and Async:

```fsharp
open Supabase.FSharp

// Task to Async
let task = client.InitializeAsync()
let asyncOp = task.AsAsyncResult()

// Async to Task
let async = async { return 42 }
let task = async.AsTask()

// F#-friendly async methods
let! _ = client.InitializeAsyncF()
let! session = client.Auth.SignInAsyncF("email", "password")
let! _ = client.Realtime.ConnectAsyncF()
```

## Pipeline Style

All functions are designed to work naturally with F# pipelines:

```fsharp
open Supabase.FSharp

let workflow = async {
    let! client =
        Supabase.create url key options
        |> fun c -> c :?> Supabase.Client
        |> Supabase.initialize

    // Pipeline-style auth
    let currentUser =
        client
        |> Auth.currentUser
        |> Option.map (fun u -> u.EmailOption)

    // Pipeline-style queries
    let! movies =
        client
        |> Supabase.from<Movie>
        |> fun table -> table.Get()
        |> Async.AwaitTask

    return movies
}
```

## Examples

See the [Examples](Examples/) folder for complete working examples:

- **[SupabaseExample.FSharp](Examples/SupabaseExample.FSharp)** - Comprehensive examples of all features
- **[ContactApp](Examples/ContactApp)** - Real-world CRUD application using Oxpecker web framework

## Test Loadmap

The CI (and `dotnet test`) already runs a Supabase-backed property test suite in `SupabaseIntegrationPropertyTests.fs`. We track further PBT expansion here:

| Area | Status | Notes |
| --- | --- | --- |
| CRUD round-trip (insert/update/delete/query) | ✅ implemented | Covers scalar columns and basic filters, cleans up created rows. |
| Concurrent insert/idempotent query | ✅ implemented | Ensures same query returns consistent result counts and concurrent inserts succeed. |
| Rich data types (JSON/arrays/storage references) | 🔜 planned | Add properties that round-trip complex payloads to catch serialization regressions. |
| Authorization / RLS policies | 🔜 planned | Spin up alternative roles/headers to verify policies accept/deny requests as expected. |
| Realtime + RPC behaviours | 🔜 planned | Property tests for channel subscription lifecycle and RPC responses. |
| Command-based state transitions | 🔜 planned | Use FsCheck command model to mix insert/update/delete actions randomly and assert invariants. |

Contributions are welcome—feel free to send PRs that implement any "planned" item above or propose new properties.

## Requirements

- .NET 9.0 or later
- F# 9.0 or later

## Dependencies

This library wraps the official Supabase C# client libraries:

- [Supabase](https://www.nuget.org/packages/Supabase) (v1.1.1)
- [Supabase.Gotrue](https://www.nuget.org/packages/Supabase.Gotrue) (v6.0.3)
- [Supabase.Postgrest](https://www.nuget.org/packages/Supabase.Postgrest) (v4.1.0)
- [Supabase.Realtime](https://www.nuget.org/packages/Supabase.Realtime) (v7.2.0)
- [Supabase.Storage](https://www.nuget.org/packages/Supabase.Storage) (v2.0.2)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Credits

This library is built on top of the excellent [Supabase C# client](https://github.com/supabase-community/supabase-csharp) by the Supabase community.

## Resources

- [Supabase Documentation](https://supabase.com/docs)
- [Supabase C# Documentation](https://supabase.com/docs/reference/csharp/introduction)
- [F# Documentation](https://learn.microsoft.com/en-us/dotnet/fsharp/)
