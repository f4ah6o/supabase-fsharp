module SupabaseBuilderTests

open System
open Xunit
open FsUnit.Xunit
open Supabase
open Supabase.FSharp

[<Fact>]
let ``SupabaseOptionsBuilder creates default options`` () =
    // Arrange & Act
    let options = supabaseOptions { () }

    // Assert
    options |> should be instanceOfType<SupabaseOptions>
    options |> should not' (equal null)

[<Fact>]
let ``SupabaseOptionsBuilder can set schema`` () =
    // Arrange
    let expectedSchema = "custom_schema"

    // Act
    let options = supabaseOptions {
        schema expectedSchema
    }

    // Assert
    options.Schema |> should equal expectedSchema

[<Fact>]
let ``SupabaseOptionsBuilder can set autoRefreshToken`` () =
    // Arrange & Act
    let optionsEnabled = supabaseOptions {
        autoRefreshToken true
    }

    let optionsDisabled = supabaseOptions {
        autoRefreshToken false
    }

    // Assert
    optionsEnabled.AutoRefreshToken |> should equal true
    optionsDisabled.AutoRefreshToken |> should equal false

[<Fact>]
let ``SupabaseOptionsBuilder can set autoConnectRealtime`` () =
    // Arrange & Act
    let optionsEnabled = supabaseOptions {
        autoConnectRealtime true
    }

    let optionsDisabled = supabaseOptions {
        autoConnectRealtime false
    }

    // Assert
    optionsEnabled.AutoConnectRealtime |> should equal true
    optionsDisabled.AutoConnectRealtime |> should equal false

[<Fact>]
let ``SupabaseOptionsBuilder can add custom header`` () =
    // Arrange
    let headerKey = "X-Custom-Header"
    let headerValue = "CustomValue"

    // Act
    let options = supabaseOptions {
        header headerKey headerValue
    }

    // Assert
    options.Headers |> should not' (equal null)
    options.Headers.[headerKey] |> should equal headerValue

[<Fact>]
let ``SupabaseOptionsBuilder can add multiple headers`` () =
    // Arrange & Act
    let options = supabaseOptions {
        header "X-Header-1" "Value1"
        header "X-Header-2" "Value2"
        header "X-Header-3" "Value3"
    }

    // Assert
    options.Headers.["X-Header-1"] |> should equal "Value1"
    options.Headers.["X-Header-2"] |> should equal "Value2"
    options.Headers.["X-Header-3"] |> should equal "Value3"

[<Fact>]
let ``SupabaseOptionsBuilder can set multiple options together`` () =
    // Arrange & Act
    let options = supabaseOptions {
        schema "public"
        autoRefreshToken true
        autoConnectRealtime false
        header "Authorization" "Bearer token123"
    }

    // Assert
    options.Schema |> should equal "public"
    options.AutoRefreshToken |> should equal true
    options.AutoConnectRealtime |> should equal false
    options.Headers.["Authorization"] |> should equal "Bearer token123"

[<Fact>]
let ``createSimpleClient creates client with URL and key`` () =
    // Arrange
    let url = "https://example.supabase.co"
    let key = "test-api-key"

    // Act
    let client = createSimpleClient url key

    // Assert
    client |> should be instanceOfType<Client>
    client |> should not' (equal null)

[<Fact>]
let ``createClient creates client with options`` () =
    // Arrange
    let url = "https://example.supabase.co"
    let key = "test-api-key"
    let options = supabaseOptions {
        schema "custom"
        autoRefreshToken false
    }

    // Act
    let client = createClient url key options

    // Assert
    client |> should be instanceOfType<Client>
    client |> should not' (equal null)

[<Fact>]
let ``createClient can be used with computation expression`` () =
    // Arrange
    let url = "https://example.supabase.co"
    let key = "test-api-key"

    // Act
    let client =
        createClient url key (supabaseOptions {
            schema "public"
            autoRefreshToken true
            header "X-Test" "TestValue"
        })

    // Assert
    client |> should be instanceOfType<Client>

[<Fact>]
let ``AuthWorkflowBuilder supports return`` () =
    // Arrange
    let expectedValue = "test result"

    // Act
    let workflow = auth {
        return expectedValue
    }

    let result = Async.RunSynchronously workflow

    // Assert
    result |> should equal expectedValue

[<Fact>]
let ``AuthWorkflowBuilder supports bind`` () =
    // Arrange
    let asyncOp = async { return 10 }

    // Act
    let workflow = auth {
        let! value = asyncOp
        return value * 2
    }

    let result = Async.RunSynchronously workflow

    // Assert
    result |> should equal 20

[<Fact>]
let ``AuthWorkflowBuilder supports multiple binds`` () =
    // Arrange
    let async1 = async { return 5 }
    let async2 = async { return 3 }

    // Act
    let workflow = auth {
        let! x = async1
        let! y = async2
        return x + y
    }

    let result = Async.RunSynchronously workflow

    // Assert
    result |> should equal 8

[<Fact>]
let ``AuthWorkflowBuilder supports returnFrom`` () =
    // Arrange
    let asyncOp = async { return "from async" }

    // Act
    let workflow = auth {
        return! asyncOp
    }

    let result = Async.RunSynchronously workflow

    // Assert
    result |> should equal "from async"

[<Fact>]
let ``AuthWorkflowBuilder supports delay`` () =
    // Arrange
    let mutable executed = false

    // Act
    let workflow = auth {
        executed <- true
        return "delayed"
    }

    // Assert - workflow not executed yet
    executed |> should equal false

    // Execute the workflow
    let result = Async.RunSynchronously workflow
    executed |> should equal true
    result |> should equal "delayed"

[<Fact>]
let ``AuthWorkflowBuilder supports combine`` () =
    // Arrange
    let mutable step1 = false
    let mutable step2 = false

    // Act
    let workflow = auth {
        step1 <- true
        step2 <- true
        return "combined"
    }

    let result = Async.RunSynchronously workflow

    // Assert
    step1 |> should equal true
    step2 |> should equal true
    result |> should equal "combined"

[<Fact>]
let ``AuthWorkflowBuilder supports exception handling with TryWith`` () =
    // Arrange
    let workflow = auth {
        try
            failwith "test exception"
            return "success"
        with
        | ex -> return "caught: " + ex.Message
    }

    // Act
    let result = Async.RunSynchronously workflow

    // Assert
    result |> should equal "caught: test exception"

[<Fact>]
let ``AuthWorkflowBuilder supports TryFinally`` () =
    // Arrange
    let mutable finallyCalled = false

    let workflow = auth {
        try
            return "result"
        finally
            finallyCalled <- true
    }

    // Act
    let result = Async.RunSynchronously workflow

    // Assert
    result |> should equal "result"
    finallyCalled |> should equal true

[<Fact>]
let ``SupabaseOptionsBuilder default values are preserved`` () =
    // Arrange & Act
    let options = supabaseOptions { () }

    // Assert - Check that unset options have their defaults
    options |> should not' (equal null)
    options.Headers |> should not' (equal null)

[<Fact>]
let ``Multiple SupabaseOptionsBuilder instances are independent`` () =
    // Arrange & Act
    let options1 = supabaseOptions {
        schema "schema1"
        autoRefreshToken true
    }

    let options2 = supabaseOptions {
        schema "schema2"
        autoRefreshToken false
    }

    // Assert
    options1.Schema |> should equal "schema1"
    options2.Schema |> should equal "schema2"
    options1.AutoRefreshToken |> should equal true
    options2.AutoRefreshToken |> should equal false
