namespace Supabase.FSharp

open System
open Supabase
open Supabase.Gotrue

/// <summary>
/// Computation expression builder for configuring Supabase options
/// </summary>
type SupabaseOptionsBuilder() =

    member _.Yield(_) = SupabaseOptions()

    /// <summary>
    /// Sets the database schema
    /// </summary>
    [<CustomOperation("schema")>]
    member _.Schema(options: SupabaseOptions, schema: string) =
        options.Schema <- schema
        options

    /// <summary>
    /// Enables or disables automatic token refresh
    /// </summary>
    [<CustomOperation("autoRefreshToken")>]
    member _.AutoRefreshToken(options: SupabaseOptions, enabled: bool) =
        options.AutoRefreshToken <- enabled
        options

    /// <summary>
    /// Enables or disables automatic realtime connection
    /// </summary>
    [<CustomOperation("autoConnectRealtime")>]
    member _.AutoConnectRealtime(options: SupabaseOptions, enabled: bool) =
        options.AutoConnectRealtime <- enabled
        options

    /// <summary>
    /// Sets a custom session handler
    /// </summary>
    [<CustomOperation("sessionHandler")>]
    member _.SessionHandler(options: SupabaseOptions, handler: IGotrueSessionPersistence<Session>) =
        options.SessionHandler <- handler
        options

    /// <summary>
    /// Adds a custom header
    /// </summary>
    [<CustomOperation("header")>]
    member _.Header(options: SupabaseOptions, key: string, value: string) =
        options.Headers.[key] <- value
        options

    /// <summary>
    /// Sets storage client options
    /// </summary>
    [<CustomOperation("storageOptions")>]
    member _.StorageOptions(options: SupabaseOptions, storageOptions: Storage.ClientOptions) =
        options.StorageClientOptions <- storageOptions
        options

/// <summary>
/// Computation expression for building Supabase client configuration
/// </summary>
[<AutoOpen>]
module SupabaseBuilders =

    /// <summary>
    /// Creates a computation expression for configuring Supabase options
    /// </summary>
    let supabaseOptions = SupabaseOptionsBuilder()

    /// <summary>
    /// Creates a Supabase client with the given URL and key
    /// </summary>
    let createClient url key (options: SupabaseOptions) =
        new Client(url, key, options)

    /// <summary>
    /// Creates a Supabase client with URL and key only
    /// </summary>
    let createSimpleClient url key =
        new Client(url, key)

    /// <summary>
    /// Initializes a Supabase client asynchronously
    /// </summary>
    let initializeClient (client: Client) = async {
        let! _ = client.InitializeAsyncF()
        return client
    }

/// <summary>
/// Workflow builder for authentication operations
/// </summary>
type AuthWorkflowBuilder() =

    member _.Bind(x, f) = async.Bind(x, f)
    member _.Return(x) = async.Return(x)
    member _.ReturnFrom(x) = x
    member _.Zero() = async.Zero()
    member _.Delay(f) = async.Delay(f)
    member _.Combine(a, b) = async.Combine(a, b)
    member _.For(seq, body) = async.For(seq, body)
    member _.While(guard, body) = async.While(guard, body)
    member _.TryFinally(body, compensation) = async.TryFinally(body, compensation)
    member _.TryWith(body, handler) = async.TryWith(body, handler)
    member _.Using(resource, body) = async.Using(resource, body)

/// <summary>
/// Computation expression for authentication workflows
/// </summary>
[<AutoOpen>]
module AuthWorkflows =

    /// <summary>
    /// Creates a computation expression for authentication operations
    /// </summary>
    let auth = AuthWorkflowBuilder()
