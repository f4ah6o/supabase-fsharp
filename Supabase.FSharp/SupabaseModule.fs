namespace Supabase.FSharp

open System
open Supabase
open Supabase.Interfaces
open Supabase.Gotrue
open Supabase.Postgrest.Models

/// <summary>
/// F# module with idiomatic functions for working with Supabase
/// </summary>
[<RequireQualifiedAccess>]
module Supabase =

    /// <summary>
    /// Creates a new Supabase client
    /// </summary>
    let create url key options : ISupabaseClient<Supabase.Gotrue.User, Supabase.Gotrue.Session, Supabase.Realtime.RealtimeSocket, Supabase.Realtime.RealtimeChannel, Supabase.Storage.Bucket, Supabase.Storage.FileObject> =
        new Supabase.Client(url, key, options) :> _

    /// <summary>
    /// Creates a new Supabase client with default options
    /// </summary>
    let createDefault url key : ISupabaseClient<Supabase.Gotrue.User, Supabase.Gotrue.Session, Supabase.Realtime.RealtimeSocket, Supabase.Realtime.RealtimeChannel, Supabase.Storage.Bucket, Supabase.Storage.FileObject> =
        new Supabase.Client(url, key) :> _

    /// <summary>
    /// Initializes a Supabase client
    /// </summary>
    let inline initialize (client: Supabase.Client) =
        client.InitializeAsync() |> Async.AwaitTask

    /// <summary>
    /// Gets a table reference from the client
    /// </summary>
    let inline from<'T when 'T :> BaseModel and 'T : (new : unit -> 'T)> (client: Supabase.Client) =
        client.From<'T>()

    /// <summary>
    /// Calls a remote procedure
    /// </summary>
    let inline rpc procedureName parameters (client: Supabase.Client) =
        client.Rpc(procedureName, parameters) |> Async.AwaitTask

    /// <summary>
    /// Calls a remote procedure with a typed response
    /// </summary>
    let inline rpcTyped<'T> procedureName parameters (client: Supabase.Client) =
        client.Rpc<'T>(procedureName, parameters) |> Async.AwaitTask

/// <summary>
/// F# module with idiomatic functions for authentication
/// </summary>
[<RequireQualifiedAccess>]
module Auth =

    /// <summary>
    /// Signs in with email and password
    /// </summary>
    let inline signIn (email: string) (password: string) (client: Supabase.Client) = async {
        return! client.Auth.SignIn(email, password) |> Async.AwaitTask
    }

    /// <summary>
    /// Signs up with email and password
    /// </summary>
    let inline signUp email password (client: Supabase.Client) = async {
        return! client.Auth.SignUp(email, password) |> Async.AwaitTask
    }

    /// <summary>
    /// Signs up with email, password, and additional options
    /// </summary>
    let inline signUpWithOptions email password options (client: Supabase.Client) = async {
        return! client.Auth.SignUp(email, password, options) |> Async.AwaitTask
    }

    /// <summary>
    /// Signs out the current user
    /// </summary>
    let inline signOut (client: Supabase.Client) = async {
        do! client.Auth.SignOut() |> Async.AwaitTask
    }

    /// <summary>
    /// Gets the current session as an option
    /// </summary>
    let inline currentSession (client: Supabase.Client) =
        ofObj client.Auth.CurrentSession

    /// <summary>
    /// Gets the current user as an option
    /// </summary>
    let inline currentUser (client: Supabase.Client) =
        ofObj client.Auth.CurrentUser

    /// <summary>
    /// Retrieves the current session
    /// </summary>
    let inline retrieveSession (client: Supabase.Client) = async {
        return! client.Auth.RetrieveSessionAsync() |> Async.AwaitTask
    }

    /// <summary>
    /// Refreshes the current session
    /// </summary>
    let inline refreshSession (client: Supabase.Client) = async {
        return! client.Auth.RefreshSession() |> Async.AwaitTask
    }

    /// <summary>
    /// Sends a password reset email
    /// </summary>
    let inline resetPasswordForEmail (email: string) (client: Supabase.Client) = async {
        return! client.Auth.ResetPasswordForEmail(email) |> Async.AwaitTask
    }

    /// <summary>
    /// Updates the current user
    /// </summary>
    let inline updateUser attributes (client: Supabase.Client) = async {
        return! client.Auth.Update(attributes) |> Async.AwaitTask
    }

/// <summary>
/// F# module with idiomatic functions for Realtime
/// </summary>
[<RequireQualifiedAccess>]
module Realtime =

    /// <summary>
    /// Connects to Realtime
    /// </summary>
    let inline connect (client: Supabase.Client) = async {
        let! _ = client.Realtime.ConnectAsync() |> Async.AwaitTask
        return ()
    }

    /// <summary>
    /// Disconnects from Realtime
    /// </summary>
    let inline disconnect (client: Supabase.Client) = async {
        client.Realtime.Disconnect() |> ignore
    }

    /// <summary>
    /// Sets the auth token for Realtime
    /// </summary>
    let inline setAuth token (client: Supabase.Client) =
        client.Realtime.SetAuth(token)

    /// <summary>
    /// Gets a channel by name
    /// </summary>
    let inline channel name (client: Supabase.Client) =
        client.Realtime.Channel(name)

/// <summary>
/// F# module with idiomatic functions for Storage
/// </summary>
[<RequireQualifiedAccess>]
module Storage =

    /// <summary>
    /// Gets a storage bucket
    /// </summary>
    let inline bucket bucketId (client: Supabase.Client) =
        client.Storage.From(bucketId)

    /// <summary>
    /// Uploads a file to storage
    /// </summary>
    let inline upload (bucketId: string) (path: string) (fileBytes: byte[]) (client: Supabase.Client) = async {
        let bucket = client.Storage.From(bucketId)
        return! bucket.Upload(fileBytes, path) |> Async.AwaitTask
    }

    /// <summary>
    /// Downloads a file from storage
    /// </summary>
    let inline download (bucketId: string) (path: string) (client: Supabase.Client) = async {
        let bucket = client.Storage.From(bucketId)
        return!
            bucket.Download(path, ?transformOptions = None, ?onProgress = None)
            |> Async.AwaitTask
    }

    /// <summary>
    /// Deletes files from storage
    /// </summary>
    let inline delete (bucketId: string) (paths: string list) (client: Supabase.Client) = async {
        let bucket = client.Storage.From(bucketId)
        let dotnetList = System.Collections.Generic.List<string>(paths)
        return! bucket.Remove(dotnetList) |> Async.AwaitTask
    }

    /// <summary>
    /// Lists files in a bucket
    /// </summary>
    let inline list (bucketId: string) (path: string) (client: Supabase.Client) = async {
        let bucket = client.Storage.From(bucketId)
        return! bucket.List(path) |> Async.AwaitTask
    }

    /// <summary>
    /// Gets a public URL for a file
    /// </summary>
    let inline publicUrl bucketId path (client: Supabase.Client) =
        let bucket = client.Storage.From(bucketId)
        bucket.GetPublicUrl(path)

/// <summary>
/// F# module with idiomatic functions for Functions
/// </summary>
[<RequireQualifiedAccess>]
module Functions =

    /// <summary>
    /// Invokes an edge function
    /// </summary>
    let inline invoke functionName (client: Supabase.Client) = async {
        return! client.Functions.Invoke(functionName) |> Async.AwaitTask
    }

    /// <summary>
    /// Invokes an edge function with parameters
    /// </summary>
    let inline invokeWith functionName parameters (client: Supabase.Client) = async {
        return! client.Functions.Invoke(functionName, parameters) |> Async.AwaitTask
    }

    /// <summary>
    /// Invokes an edge function with a typed response
    /// </summary>
    let inline invokeTyped<'T when 'T : not struct> functionName (client: Supabase.Client) = async {
        return! client.Functions.Invoke<'T>(functionName) |> Async.AwaitTask
    }

    /// <summary>
    /// Invokes an edge function with parameters and a typed response
    /// </summary>
    let inline invokeTypedWith<'T when 'T : not struct> functionName parameters (client: Supabase.Client) = async {
        return! client.Functions.Invoke<'T>(functionName, parameters) |> Async.AwaitTask
    }
