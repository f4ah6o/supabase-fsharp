module SupabaseIntegrationPropertyTests

open System
open System.Diagnostics
open System.IO
open Xunit
open FsCheck
open FsCheck.Xunit
open Supabase.FSharp
open Supabase.Postgrest.Attributes
open Supabase.Postgrest.Models

// ==============================================
// Test Models
// ==============================================

[<Table("test_items")>]
type TestItem() =
    inherit BaseModel()

    [<PrimaryKey("id")>]
    member val Id = 0 with get, set

    [<Column("name")>]
    member val Name = "" with get, set

    [<Column("value")>]
    member val Value = 0 with get, set

    [<Column("is_active")>]
    member val IsActive = false with get, set

    [<Column("created_at")>]
    member val CreatedAt = DateTime.UtcNow with get, set

// ==============================================
// Test Helpers
// ==============================================

/// Lazily locate the repository root that contains the Supabase config
let private supabaseWorkdir =
    let rec tryFindSupabaseRoot (dir: DirectoryInfo) =
        if isNull dir then
            None
        else
            let configPath = Path.Combine(dir.FullName, "supabase", "config.toml")
            if File.Exists(configPath) then
                Some dir.FullName
            else
                tryFindSupabaseRoot dir.Parent

    lazy (Directory.GetCurrentDirectory() |> DirectoryInfo |> tryFindSupabaseRoot)

let private runSupabaseCommand (arguments: string) =
    match supabaseWorkdir.Value with
    | None -> None
    | Some workdir ->
        try
            let psi =
                ProcessStartInfo(
                    FileName = "supabase",
                    Arguments = arguments,
                    WorkingDirectory = workdir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                )

            use proc = new Process()
            proc.StartInfo <- psi
            if proc.Start() then
                let output = proc.StandardOutput.ReadToEnd()
                let error = proc.StandardError.ReadToEnd()
                proc.WaitForExit()
                Some(proc.ExitCode, output, error)
            else
                None
        with
        | _ -> None

let private tryPopulateEnvFromSupabaseCli() =
    match runSupabaseCommand "status -o env" with
    | Some (0, output, _) ->
        let tryExtract key =
            output.Split(Environment.NewLine)
            |> Array.tryPick (fun line ->
                let trimmed = line.Trim()
                if trimmed.StartsWith(key + "=") then
                    let value = trimmed.Substring(key.Length + 1).Trim().Trim('"')
                    if String.IsNullOrWhiteSpace(value) then None else Some value
                else
                    None)

        match tryExtract "API_URL", tryExtract "SERVICE_ROLE_KEY" with
        | Some url, Some key ->
            Environment.SetEnvironmentVariable("SUPABASE_TEST_URL", url)
            Environment.SetEnvironmentVariable("SUPABASE_TEST_SERVICE_ROLE_KEY", key)
            true
        | _ -> false
    | _ -> false

let private ensureLocalSchema() =
    match runSupabaseCommand "migration up --local --yes" with
    | Some (0, _, _) -> true
    | _ -> false

let private supabaseSetup =
    lazy (
        let url = Environment.GetEnvironmentVariable("SUPABASE_TEST_URL")
        let key = Environment.GetEnvironmentVariable("SUPABASE_TEST_SERVICE_ROLE_KEY")
        let loadedFromCli =
            if String.IsNullOrEmpty(url) || String.IsNullOrEmpty(key) then
                tryPopulateEnvFromSupabaseCli()
            else
                false

        let finalUrl = Environment.GetEnvironmentVariable("SUPABASE_TEST_URL")
        let finalKey = Environment.GetEnvironmentVariable("SUPABASE_TEST_SERVICE_ROLE_KEY")
        if String.IsNullOrEmpty(finalUrl) || String.IsNullOrEmpty(finalKey) then
            false
        else if loadedFromCli then
            ensureLocalSchema()
        else
            true
    )

/// Get Supabase client from environment variables or Supabase CLI
let getSupabaseClient() =
    let url = Environment.GetEnvironmentVariable("SUPABASE_TEST_URL")
    let key = Environment.GetEnvironmentVariable("SUPABASE_TEST_SERVICE_ROLE_KEY")

    let url, key =
        if String.IsNullOrEmpty(url) || String.IsNullOrEmpty(key) then
            let _ = supabaseSetup.Value
            Environment.GetEnvironmentVariable("SUPABASE_TEST_URL"),
            Environment.GetEnvironmentVariable("SUPABASE_TEST_SERVICE_ROLE_KEY")
        else
            url, key

    if String.IsNullOrEmpty(url) || String.IsNullOrEmpty(key) then
        None
    else
        let options =
            supabaseOptions {
                schema "public"
                autoRefreshToken false
                autoConnectRealtime false
            }
        let clientInterface = Supabase.create url key options
        let client = clientInterface :?> Supabase.Client
        Some client

/// Initialize client (Supabase SDK handles redundant calls internally)
let ensureInitialized (client: Supabase.Client) = async {
    let! _ = Supabase.initialize client
    return ()
}

/// Create a unique test table item
let createTestItem name value isActive =
    let item = TestItem()
    item.Name <- name
    item.Value <- value
    item.IsActive <- isActive
    item

let getTestItemTable (client: Supabase.Client) =
    Supabase.from<TestItem> client

/// Cleanup test items created during test
let cleanupTestItems (client: Supabase.Client) names = async {
    try
        for name in names do
            try
                let! response =
                    (getTestItemTable client)
                        .Filter("name", Supabase.Postgrest.Constants.Operator.Equals, name)
                        .Delete()
                    |> Async.AwaitTask
                ()
            with
            | _ -> () // Ignore cleanup errors
    with
    | _ -> () // Ignore cleanup errors
}

// ==============================================
// Generators for Property-Based Testing
// ==============================================

type TestGenerators =
    /// Generate valid item names (non-empty, reasonable length)
    static member ItemName() =
        Arb.Default.NonEmptyString()
        |> Arb.filter (fun (NonEmptyString s) -> s.Length <= 100)
        |> Arb.convert (fun (NonEmptyString s) -> s) NonEmptyString

    /// Generate valid integer values
    static member ItemValue() =
        Arb.Default.Int32()
        |> Arb.filter (fun x -> x >= 0 && x <= 1000000)

// ==============================================
// Property-Based Tests - Client Creation
// ==============================================

[<Fact>]
let ``Client creation with valid credentials returns initialized client`` () =
    match getSupabaseClient() with
    | None ->
        // Skip test if credentials not available
        Assert.True(true, "Skipped: SUPABASE_TEST_URL or SUPABASE_TEST_SERVICE_ROLE_KEY not set")
    | Some client ->
        async {
            do! ensureInitialized client
            Assert.NotNull(client)
            Assert.NotNull(client.Auth)
            Assert.NotNull(client.Storage)
        }
        |> Async.RunSynchronously

[<Fact>]
let ``Client can be created and initialized multiple times`` () =
    match getSupabaseClient() with
    | None ->
        Assert.True(true, "Skipped: SUPABASE_TEST_URL or SUPABASE_TEST_SERVICE_ROLE_KEY not set")
    | Some _ ->
        async {
            // Create and initialize multiple clients
            for i in 1..3 do
                match getSupabaseClient() with
                | Some client ->
                    do! ensureInitialized client
                    Assert.NotNull(client)
                | None ->
                    Assert.Fail("Failed to create client")
        }
        |> Async.RunSynchronously

// ==============================================
// Property-Based Tests - Database Operations
// ==============================================

[<Property(MaxTest = 5, Arbitrary = [| typeof<TestGenerators> |])>]
let ``Inserting and retrieving item preserves data`` (name: string) (value: int) (isActive: bool) =
    match getSupabaseClient() with
    | None ->
        // Skip if no credentials
        true
    | Some client ->
        async {
            do! ensureInitialized client

            // Create unique name to avoid conflicts
            let uniqueName = sprintf "%s_%s" name (Guid.NewGuid().ToString("N").Substring(0, 8))
            let item = createTestItem uniqueName value isActive

            try
                // Insert item
                let! insertResponse = (getTestItemTable client).Insert(item) |> Async.AwaitTask
                Assert.NotNull(insertResponse)

                // Retrieve item
                let! getResponse =
                    (getTestItemTable client)
                        .Filter("name", Supabase.Postgrest.Constants.Operator.Equals, uniqueName)
                        .Get()
                    |> Async.AwaitTask

                Assert.NotNull(getResponse)
                let retrievedItems = getResponse.Models
                Assert.True(retrievedItems.Count > 0, "Should retrieve at least one item")

                let retrievedItem = retrievedItems.[0]
                let result =
                    retrievedItem.Name = uniqueName &&
                    retrievedItem.Value = value &&
                    retrievedItem.IsActive = isActive

                // Cleanup
                do! cleanupTestItems client [uniqueName]

                return result
            with
            | ex ->
                // Cleanup on error
                do! cleanupTestItems client [uniqueName]
                return false
        }
        |> Async.RunSynchronously

[<Property(MaxTest = 5, Arbitrary = [| typeof<TestGenerators> |])>]
let ``Updating item changes values correctly`` (name: string) (initialValue: int) (updatedValue: int) =
    match getSupabaseClient() with
    | None -> true
    | Some client ->
        async {
            do! ensureInitialized client

            let uniqueName = sprintf "%s_%s" name (Guid.NewGuid().ToString("N").Substring(0, 8))
            let item = createTestItem uniqueName initialValue true

            try
                // Insert item
                let! insertResponse = (getTestItemTable client).Insert(item) |> Async.AwaitTask
                let insertedItem = insertResponse.Models.[0]

                // Update item
                insertedItem.Value <- updatedValue
                let! updateResponse = (getTestItemTable client).Update(insertedItem) |> Async.AwaitTask
                Assert.NotNull(updateResponse)

                // Retrieve and verify
                let! getResponse =
                    (getTestItemTable client)
                        .Filter("name", Supabase.Postgrest.Constants.Operator.Equals, uniqueName)
                        .Get()
                    |> Async.AwaitTask

                let retrievedItem = getResponse.Models.[0]
                let result = retrievedItem.Value = updatedValue

                // Cleanup
                do! cleanupTestItems client [uniqueName]

                return result
            with
            | ex ->
                do! cleanupTestItems client [uniqueName]
                return false
        }
        |> Async.RunSynchronously

[<Property(MaxTest = 5, Arbitrary = [| typeof<TestGenerators> |])>]
let ``Deleting item removes it from database`` (name: string) (value: int) =
    match getSupabaseClient() with
    | None -> true
    | Some client ->
        async {
            do! ensureInitialized client

            let uniqueName = sprintf "%s_%s" name (Guid.NewGuid().ToString("N").Substring(0, 8))
            let item = createTestItem uniqueName value true

            try
                // Insert item
                let! insertResponse = (getTestItemTable client).Insert(item) |> Async.AwaitTask
                Assert.NotNull(insertResponse)

                // Delete item
                let! _ =
                    (getTestItemTable client)
                        .Filter("name", Supabase.Postgrest.Constants.Operator.Equals, uniqueName)
                        .Delete()
                    |> Async.AwaitTask

                // Try to retrieve - should be empty
                let! getResponse =
                    (getTestItemTable client)
                        .Filter("name", Supabase.Postgrest.Constants.Operator.Equals, uniqueName)
                        .Get()
                    |> Async.AwaitTask

                return getResponse.Models.Count = 0
            with
            | ex ->
                // Try cleanup anyway
                do! cleanupTestItems client [uniqueName]
                return false
        }
        |> Async.RunSynchronously

// ==============================================
// Property-Based Tests - Query Operations
// ==============================================

[<Property(MaxTest = 3, Arbitrary = [| typeof<TestGenerators> |])>]
let ``Filtering by value returns only matching items`` (targetValue: int) =
    match getSupabaseClient() with
    | None -> true
    | Some client ->
        async {
            do! ensureInitialized client

            let baseName = sprintf "filter_test_%s" (Guid.NewGuid().ToString("N").Substring(0, 8))
            let names = [
                sprintf "%s_match_1" baseName
                sprintf "%s_match_2" baseName
                sprintf "%s_nomatch" baseName
            ]

            try
                // Insert test items
                let item1 = createTestItem names.[0] targetValue true
                let item2 = createTestItem names.[1] targetValue false
                let item3 = createTestItem names.[2] (targetValue + 1) true

                let! _ = (getTestItemTable client).Insert(item1) |> Async.AwaitTask
                let! _ = (getTestItemTable client).Insert(item2) |> Async.AwaitTask
                let! _ = (getTestItemTable client).Insert(item3) |> Async.AwaitTask

                // Query items with targetValue
                let! response =
                    (getTestItemTable client)
                        .Filter("value", Supabase.Postgrest.Constants.Operator.Equals, targetValue)
                        .Filter("name", Supabase.Postgrest.Constants.Operator.Like, sprintf "%s_match*" baseName)
                        .Get()
                    |> Async.AwaitTask

                let matchingItems = response.Models
                let result =
                    matchingItems.Count >= 2 &&
                    matchingItems |> Seq.forall (fun item -> item.Value = targetValue)

                // Cleanup
                do! cleanupTestItems client names

                return result
            with
            | ex ->
                do! cleanupTestItems client names
                return false
        }
        |> Async.RunSynchronously

// ==============================================
// Property-Based Tests - Idempotency
// ==============================================

[<Property(MaxTest = 3, Arbitrary = [| typeof<TestGenerators> |])>]
let ``Querying same filter multiple times returns consistent results`` (value: int) =
    match getSupabaseClient() with
    | None -> true
    | Some client ->
        async {
            do! ensureInitialized client

            let uniqueName = sprintf "idempotent_%d_%s" value (Guid.NewGuid().ToString("N").Substring(0, 8))
            let item = createTestItem uniqueName value true

            try
                // Insert item
                let! _ = (getTestItemTable client).Insert(item) |> Async.AwaitTask

                // Query multiple times
                let query() =
                    (getTestItemTable client)
                        .Filter("name", Supabase.Postgrest.Constants.Operator.Equals, uniqueName)
                        .Get()
                    |> Async.AwaitTask

                let! response1 = query()
                let! response2 = query()
                let! response3 = query()

                let count1 = response1.Models.Count
                let count2 = response2.Models.Count
                let count3 = response3.Models.Count

                let result = count1 = count2 && count2 = count3 && count1 > 0

                // Cleanup
                do! cleanupTestItems client [uniqueName]

                return result
            with
            | ex ->
                do! cleanupTestItems client [uniqueName]
                return false
        }
        |> Async.RunSynchronously

// ==============================================
// Property-Based Tests - Async Operations
// ==============================================

[<Fact>]
let ``Multiple concurrent inserts complete successfully`` () =
    match getSupabaseClient() with
    | None ->
        Assert.True(true, "Skipped: SUPABASE_TEST_URL or SUPABASE_TEST_SERVICE_ROLE_KEY not set")
    | Some client ->
        async {
            do! ensureInitialized client

            let baseName = sprintf "concurrent_%s" (Guid.NewGuid().ToString("N").Substring(0, 8))
            let names = [1..5] |> List.map (fun i -> sprintf "%s_%d" baseName i)

            try
                // Create multiple insert operations
                let insertOps =
                    names
                    |> List.mapi (fun i name ->
                        async {
                            let item = createTestItem name (i * 10) true
                            let! _ = (getTestItemTable client).Insert(item) |> Async.AwaitTask
                            return ()
                        }
                    )

                // Run all inserts concurrently
                do! insertOps |> Async.Parallel |> Async.Ignore

                // Verify all items were inserted
                let! response =
                    (getTestItemTable client)
                        .Filter("name", Supabase.Postgrest.Constants.Operator.Like, sprintf "%s*" baseName)
                        .Get()
                    |> Async.AwaitTask

                Assert.True(response.Models.Count >= 5, sprintf "Expected at least 5 items, got %d" response.Models.Count)

                // Cleanup
                do! cleanupTestItems client names
            with
            | ex ->
                // Cleanup
                do! cleanupTestItems client names
                Assert.Fail(sprintf "Concurrent insert test failed: %s" ex.Message)
        }
        |> Async.RunSynchronously
