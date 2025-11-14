module SupabaseIntegrationPropertyTests

open System
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

/// Get Supabase client from environment variables
let getSupabaseClient() =
    let url = Environment.GetEnvironmentVariable("SUPABASE_TEST_URL")
    let key = Environment.GetEnvironmentVariable("SUPABASE_TEST_SERVICE_ROLE_KEY")

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

/// Initialize client if not already initialized
let ensureInitialized (client: Supabase.Client) = async {
    try
        // Try a simple operation to check if initialized
        let _ = client.Auth
        return ()
    with
    | _ ->
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

/// Cleanup test items created during test
let cleanupTestItems (client: Supabase.Client) names = async {
    try
        let table = Supabase.from<TestItem> client
        for name in names do
            try
                let! response =
                    table
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
                let table = Supabase.from<TestItem> client

                // Insert item
                let! insertResponse = table.Insert(item) |> Async.AwaitTask
                Assert.NotNull(insertResponse)

                // Retrieve item
                let! getResponse =
                    table
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
                let table = Supabase.from<TestItem> client

                // Insert item
                let! insertResponse = table.Insert(item) |> Async.AwaitTask
                let insertedItem = insertResponse.Models.[0]

                // Update item
                insertedItem.Value <- updatedValue
                let! updateResponse = table.Update(insertedItem) |> Async.AwaitTask
                Assert.NotNull(updateResponse)

                // Retrieve and verify
                let! getResponse =
                    table
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
                let table = Supabase.from<TestItem> client

                // Insert item
                let! insertResponse = table.Insert(item) |> Async.AwaitTask
                Assert.NotNull(insertResponse)

                // Delete item
                let! _ =
                    table
                        .Filter("name", Supabase.Postgrest.Constants.Operator.Equals, uniqueName)
                        .Delete()
                    |> Async.AwaitTask

                // Try to retrieve - should be empty
                let! getResponse =
                    table
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
                let table = Supabase.from<TestItem> client

                // Insert test items
                let item1 = createTestItem names.[0] targetValue true
                let item2 = createTestItem names.[1] targetValue false
                let item3 = createTestItem names.[2] (targetValue + 1) true

                let! _ = table.Insert(item1) |> Async.AwaitTask
                let! _ = table.Insert(item2) |> Async.AwaitTask
                let! _ = table.Insert(item3) |> Async.AwaitTask

                // Query items with targetValue
                let! response =
                    table
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
                let table = Supabase.from<TestItem> client

                // Insert item
                let! _ = table.Insert(item) |> Async.AwaitTask

                // Query multiple times
                let query() =
                    table
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
                let table = Supabase.from<TestItem> client

                // Create multiple insert operations
                let insertOps =
                    names
                    |> List.mapi (fun i name ->
                        async {
                            let item = createTestItem name (i * 10) true
                            let! _ = table.Insert(item) |> Async.AwaitTask
                            return ()
                        }
                    )

                // Run all inserts concurrently
                do! insertOps |> Async.Parallel |> Async.Ignore

                // Verify all items were inserted
                let! response =
                    table
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
