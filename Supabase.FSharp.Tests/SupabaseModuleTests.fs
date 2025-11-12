module SupabaseModuleTests

open System
open Xunit
open FsUnit.Xunit
open Supabase
open Supabase.FSharp

// Test models for Postgrest
type TestModel() =
    inherit Supabase.Postgrest.Models.BaseModel()
    member val Id = 0 with get, set
    member val Name = "" with get, set

[<Fact>]
let ``Supabase.create creates client with options`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"
    let options = supabaseOptions {
        schema "public"
    }

    // Act
    let client = Supabase.create url key options

    // Assert
    client |> should be instanceOfType<Client>
    client |> should not' (equal null)

[<Fact>]
let ``Supabase.createDefault creates client without options`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"

    // Act
    let client = Supabase.createDefault url key

    // Assert
    client |> should be instanceOfType<Client>
    client |> should not' (equal null)

[<Fact>]
let ``Supabase.from returns table reference`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"
    let client = Supabase.createDefault url key

    // Act
    let table = Supabase.from<TestModel> client

    // Assert
    table |> should not' (equal null)

[<Fact>]
let ``Supabase functions can be pipelined`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"

    // Act
    let client =
        supabaseOptions {
            schema "public"
            autoRefreshToken true
        }
        |> Supabase.create url key

    // Assert
    client |> should be instanceOfType<Client>

[<Fact>]
let ``Supabase.create with different URLs creates different clients`` () =
    // Arrange
    let url1 = "https://test1.supabase.co"
    let url2 = "https://test2.supabase.co"
    let key = "test-key"
    let options = supabaseOptions { () }

    // Act
    let client1 = Supabase.create url1 key options
    let client2 = Supabase.create url2 key options

    // Assert
    client1 |> should not' (equal client2)

[<Fact>]
let ``Auth.currentSession returns option type`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"
    let client = Supabase.createDefault url key

    // Act
    let session = Auth.currentSession client

    // Assert
    // Session should be None for uninitialized client
    session |> should equal None

[<Fact>]
let ``Auth.currentUser returns option type`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"
    let client = Supabase.createDefault url key

    // Act
    let user = Auth.currentUser client

    // Assert
    // User should be None for uninitialized client
    user |> should equal None

[<Fact>]
let ``Realtime.setAuth accepts token and client`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"
    let client = Supabase.createDefault url key
    let token = "test-token"

    // Act & Assert - Should not throw
    Realtime.setAuth token client

[<Fact(Skip = "Realtime channels require a live connection in Supabase.Realtime 7.x")>]
let ``Realtime.channel returns channel`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"
    let client = Supabase.createDefault url key
    let channelName = "test-channel"

    // Act
    let channel = Realtime.channel channelName client

    // Assert
    channel |> should not' (equal null)

[<Fact>]
let ``Storage.bucket returns bucket reference`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"
    let client = Supabase.createDefault url key
    let bucketId = "test-bucket"

    // Act
    let bucket = Storage.bucket bucketId client

    // Assert
    bucket |> should not' (equal null)

[<Fact>]
let ``Storage.publicUrl returns URL string`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"
    let client = Supabase.createDefault url key
    let bucketId = "test-bucket"
    let path = "test/file.txt"

    // Act
    let publicUrl = Storage.publicUrl bucketId path client

    // Assert
    publicUrl |> should be instanceOfType<string>
    publicUrl |> should not' (equal null)

[<Fact>]
let ``Storage functions can be pipelined`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"
    let bucketId = "test-bucket"
    let path = "test/file.txt"

    // Act
    let publicUrl =
        Supabase.createDefault url key
        |> Storage.publicUrl bucketId path

    // Assert
    publicUrl |> should be instanceOfType<string>

[<Fact>]
let ``Multiple clients can be created independently`` () =
    // Arrange
    let url1 = "https://test1.supabase.co"
    let url2 = "https://test2.supabase.co"
    let key = "test-key"

    // Act
    let client1 = Supabase.createDefault url1 key
    let client2 = Supabase.createDefault url2 key

    // Assert
    client1 |> should not' (equal client2)
    client1 |> should be instanceOfType<Client>
    client2 |> should be instanceOfType<Client>

[<Fact>]
let ``Auth module functions accept client as last parameter for pipelining`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"

    // Act - This should compile, demonstrating pipeline compatibility
    let client = Supabase.createDefault url key
    let session = client |> Auth.currentSession
    let user = client |> Auth.currentUser

    // Assert
    session |> should equal None
    user |> should equal None

[<Fact(Skip = "Realtime channels require a live connection in Supabase.Realtime 7.x")>]
let ``Realtime module functions accept client as last parameter for pipelining`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"
    let token = "test-token"

    // Act - This should compile, demonstrating pipeline compatibility
    let client = Supabase.createDefault url key
    client |> Realtime.setAuth token
    let channel = client |> Realtime.channel "test"

    // Assert
    channel |> should not' (equal null)

[<Fact>]
let ``Storage module functions accept client as last parameter for pipelining`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"

    // Act - This should compile, demonstrating pipeline compatibility
    let bucket =
        Supabase.createDefault url key
        |> Storage.bucket "test-bucket"

    // Assert
    bucket |> should not' (equal null)

[<Fact>]
let ``Supabase.from can work with different model types`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"
    let client = Supabase.createDefault url key

    // Act
    let table1 = Supabase.from<TestModel> client

    // Assert
    table1 |> should not' (equal null)

[<Fact>]
let ``Client can be created with custom schema`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"
    let customSchema = "custom_schema"

    // Act
    let client =
        supabaseOptions {
            schema customSchema
        }
        |> Supabase.create url key

    // Assert
    client |> should be instanceOfType<Client>

[<Fact>]
let ``Client can be created with multiple configuration options`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"

    // Act
    let client =
        supabaseOptions {
            schema "public"
            autoRefreshToken true
            autoConnectRealtime false
            header "X-Custom" "Value"
        }
        |> Supabase.create url key

    // Assert
    client |> should be instanceOfType<Client>

[<Fact>]
let ``All module functions return expected types`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"
    let client = Supabase.createDefault url key

    // Act & Assert - Type checks
    Supabase.from<TestModel> client |> should be instanceOfType<Supabase.Postgrest.Table<TestModel>>
    Auth.currentSession client |> should equal None
    Auth.currentUser client |> should equal None
    Storage.bucket "test" client |> should not' (equal null)
    Storage.publicUrl "bucket" "path" client |> should be instanceOfType<string>

[<Fact>]
let ``Module functions support currying`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"
    let client = Supabase.createDefault url key

    // Act - Partial application
    let getBucket = Storage.bucket "test-bucket"
    let getPublicUrl = Storage.publicUrl "test-bucket"

    // Assert - Apply remaining parameters
    let bucket = getBucket client
    let publicUrl = getPublicUrl "test-path" client

    bucket |> should not' (equal null)
    publicUrl |> should be instanceOfType<string>

[<Fact>]
let ``Client creation with empty URL and key creates client`` () =
    // Arrange
    let url = ""
    let key = ""

    // Act
    let client = Supabase.createDefault url key

    // Assert
    client |> should be instanceOfType<Client>

[<Fact(Skip = "Realtime channels require a live connection in Supabase.Realtime 7.x")>]
let ``Realtime.channel with different names returns different channels`` () =
    // Arrange
    let url = "https://test.supabase.co"
    let key = "test-key"
    let client = Supabase.createDefault url key

    // Act
    let channel1 = Realtime.channel "channel1" client
    let channel2 = Realtime.channel "channel2" client

    // Assert
    channel1 |> should not' (equal channel2)
