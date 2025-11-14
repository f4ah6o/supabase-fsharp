open System
open Supabase.FSharp
open SupabaseExample.FSharp.Models

// Configure Supabase client using computation expression
let configureSupabaseOptions() =
    supabaseOptions {
        schema "public"
        autoRefreshToken true
        autoConnectRealtime false
    }

// Example 1: Create and initialize client using idiomatic F# modules
let example1_CreateClient() : Async<Supabase.Client option> = async {
    printfn "Example 1: Creating Supabase client"

    let url = Environment.GetEnvironmentVariable("SUPABASE_URL")
    let key = Environment.GetEnvironmentVariable("SUPABASE_KEY")

    match ofObj url, ofObj key with
    | Some url, Some key ->
        let options = configureSupabaseOptions()
        let clientInterface = Supabase.create url key options
        // Downcast to concrete Client type
        let client = clientInterface :?> Supabase.Client

        // Initialize using F# async
        let! _ = Supabase.initialize client
        printfn "✓ Client initialized successfully"
        return Some client
    | _ ->
        printfn "✗ SUPABASE_URL and SUPABASE_KEY environment variables must be set"
        return None
}

// Example 2: Authentication using idiomatic F# functions
let example2_Authentication (client: Supabase.Client) = async {
    printfn "\nExample 2: Authentication"

    try
        // Use a fixed test user to avoid creating new users on every run
        let email = "fsharp.demo@example.com"
        let password = "securepassword123"

        // Try to sign in first, if that fails, sign up
        printfn "Attempting to sign in user: %s" email
        try
            let! signInResponse = Auth.signIn email password client
            match signInResponse with
            | null ->
                printfn "  Sign in failed, attempting to sign up..."
                let! signUpResponse = Auth.signUp email password client
                match signUpResponse with
                | null -> printfn "✗ Sign up also failed"
                | _ ->
                    printfn "✓ User signed up successfully"
                    printfn "  Response type: %s" (signUpResponse.GetType().Name)
            | _ ->
                printfn "✓ User signed in successfully"
                printfn "  Response type: %s" (signInResponse.GetType().Name)
        with
        | :? AggregateException as aggEx when aggEx.InnerException <> null ->
            // Sign in failed, try sign up
            printfn "  Sign in failed (%s), attempting to sign up..." aggEx.InnerException.Message
            let! signUpResponse = Auth.signUp email password client
            match signUpResponse with
            | null -> printfn "✗ Sign up failed"
            | _ ->
                printfn "✓ User signed up successfully"
                printfn "  Response type: %s" (signUpResponse.GetType().Name)

        // Check current user using option types
        match Auth.currentUser client with
        | Some user ->
            printfn "✓ User authenticated successfully"

            // Get email safely using option extension
            match user.EmailOption with
            | Some email -> printfn "  Email: %s" email
            | None -> printfn "  Email: (not set)"

            // Keep user signed in for subsequent examples
            printfn "✓ User remains signed in for storage operations"
        | None ->
            printfn "✗ No current user - checking session..."
            match Auth.currentSession client with
            | Some session ->
                printfn "  Session exists but user not available"
                printfn "  This may indicate email confirmation is required"
            | None ->
                printfn "  No active session found"
    with
    | ex ->
        printfn "✗ Authentication error: %s" ex.Message
        printfn "  Note: This may be due to Supabase email validation settings or email confirmation requirements"
}

// Example 3: Database operations using F# async
let example3_DatabaseOperations (client: Supabase.Client) = async {
    printfn "\nExample 3: Database operations"

    try
        // Get table reference
        let moviesTable = Supabase.from<Movie> client

        // Query movies (converted to F# async automatically)
        printfn "Fetching movies..."
        let! response = moviesTable.Get() |> Async.AwaitTask

        match response with
        | null -> printfn "✗ No response from database"
        | response ->
            let movies = response.Models
            printfn "✓ Found %d movies" movies.Count

            for movie in movies do
                printfn "  - %s (ID: %d)" movie.Name movie.Id
    with
    | ex -> printfn "✗ Database error: %s" ex.Message
}

// Example 4: Realtime using F# async
let example4_Realtime (client: Supabase.Client) = async {
    printfn "\nExample 4: Realtime"

    try
        // Connect to realtime
        printfn "Connecting to realtime..."
        do! Realtime.connect client
        printfn "✓ Connected to realtime"

        // Create a channel
        let channel = Realtime.channel "public:movies" client
        printfn "✓ Channel created: %s" channel.Topic

        // Disconnect
        printfn "Disconnecting from realtime..."
        do! Realtime.disconnect client
        printfn "✓ Disconnected from realtime"
    with
    | ex -> printfn "✗ Realtime error: %s" ex.Message
}

// Example 5: Storage operations
let example5_Storage (client: Supabase.Client) = async {
    printfn "\nExample 5: Storage"

    let bucketId = "test-bucket"

    try
        let filePath = "test-file.txt"
        let fileContent = System.Text.Encoding.UTF8.GetBytes("Hello from F#!")

        // Check if authenticated
        match Auth.currentUser client with
        | None ->
            printfn "⚠ Not authenticated - some storage operations may require authentication"
        | Some _ ->
            printfn "✓ Authenticated user detected"

        // Note: ListBuckets() may require admin permissions, so we'll proceed with upload directly
        printfn "Preparing to upload file to bucket '%s'..." bucketId

        // Try to remove existing file first (if it exists)
        try
            let bucket = client.Storage.From(bucketId)
            let! _ = bucket.Remove(filePath) |> Async.AwaitTask
            printfn "  Removed existing file: %s" filePath
        with
        | ex ->
            // File doesn't exist or couldn't be deleted - this is fine
            printfn "  No existing file to remove (continuing...)"

        // Upload file
        printfn "Uploading file..."
        let! uploadResult = Storage.upload bucketId filePath fileContent client
        printfn "✓ File uploaded: %s" filePath

        // Get public URL
        let publicUrl = Storage.publicUrl bucketId filePath client
        printfn "✓ Public URL: %s" publicUrl

        // List files
        printfn "Listing files in bucket..."
        let! files = Storage.list bucketId "" client
        printfn "✓ Found %d files" files.Count

    with
    | ex ->
        printfn "✗ Storage error: %s" ex.Message

        // Check for specific error types
        if ex.Message.Contains("row-level security policy") then
            printfn "  Hint: RLS policy issue. Try making the bucket public in Supabase Dashboard"
        elif ex.Message.Contains("already exists") then
            printfn "  Hint: File already exists. The example should handle this, please report this issue."
        elif ex.Message.Contains("not found") || ex.Message.Contains("does not exist") then
            printfn "  Hint: Bucket '%s' may not exist. Create it in Supabase Dashboard > Storage" bucketId
        else
            printfn "  Note: Check Supabase Dashboard for more details"
}

// Example 6: RPC (Remote Procedure Call)
let example6_RPC (client: Supabase.Client) = async {
    printfn "\nExample 6: Remote Procedure Call"

    try
        let procedureName = "hello_world"
        let parameters = {| name = "F# Developer" |}

        printfn "Calling RPC function '%s'..." procedureName
        let! result = Supabase.rpc procedureName parameters client
        printfn "✓ RPC call completed"

    with
    | ex -> printfn "✗ RPC error: %s" ex.Message
}

// Example 7: Using computation expressions
let example7_ComputationExpression (client: Supabase.Client) = async {
    printfn "\nExample 7: Computation expressions"

    // Using auth workflow
    let! result =
        auth {
            printfn "Starting auth workflow..."

            // Retrieve current session
            let! session = Auth.retrieveSession client

            match ofObj session with
            | Some s ->
                printfn "✓ Session found"

                // Get access token as option
                match s.AccessTokenOption with
                | Some token ->
                    printfn "  Access token length: %d" token.Length
                    return Ok "Authenticated"
                | None ->
                    return Error "No access token"
            | None ->
                printfn "  No active session"
                return Error "Not authenticated"
        }

    match result with
    | Ok msg -> printfn "✓ %s" msg
    | Error err -> printfn "✗ %s" err
}

// Example 8: Pipeline-style operations
let example8_PipelineStyle() = async {
    printfn "\nExample 8: F# pipeline style"

    let url = Environment.GetEnvironmentVariable("SUPABASE_URL")
    let key = Environment.GetEnvironmentVariable("SUPABASE_KEY")

    match ofObj url, ofObj key with
    | Some url, Some key ->
        // Create options using computation expression
        let options =
            supabaseOptions {
                schema "public"
                autoRefreshToken true
            }

        // Create and initialize client using F# pipeline
        let clientInterface = Supabase.create url key options
        let client = clientInterface :?> Supabase.Client
        let! _ = Supabase.initialize client

        printfn "✓ Client created and initialized using pipeline"

        // Get current session using pipeline
        let session =
            client
            |> Auth.currentSession
            |> Option.defaultWith (fun () ->
                printfn "  No active session"
                null)

        return Some client
    | _ ->
        printfn "✗ Environment variables not set"
        return None
}

[<EntryPoint>]
let main argv =
    printfn "=========================================="
    printfn "Supabase F# Examples"
    printfn "=========================================="

    async {
        // Run all examples
        let! clientOpt = example1_CreateClient()

        match clientOpt with
        | Some client ->
            do! example2_Authentication client
            do! example3_DatabaseOperations client
            do! example4_Realtime client
            do! example5_Storage client
            do! example6_RPC client
            do! example7_ComputationExpression client
            let! _ = example8_PipelineStyle()

            // Clean up: Sign out at the end
            try
                printfn "\nCleaning up..."
                do! Auth.signOut client
                printfn "✓ Signed out successfully"
            with
            | ex -> printfn "✗ Sign out error: %s" ex.Message
        | None ->
            printfn "\nSkipping examples due to missing configuration"
            printfn "Please set SUPABASE_URL and SUPABASE_KEY environment variables"

        printfn "\n=========================================="
        printfn "Examples completed!"
        printfn "=========================================="
    }
    |> Async.RunSynchronously

    0 // return an integer exit code
