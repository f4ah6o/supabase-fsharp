namespace Supabase.FSharp

open System
open Supabase
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
    let create url key options =
        new Client(url, key, options)

    /// <summary>
    /// Creates a new Supabase client with default options
    /// </summary>
    let createDefault url key =
        new Client(url, key)

    /// <summary>
    /// Initializes a Supabase client
    /// </summary>
    let initialize (client: Client) =
        client.InitializeAsyncF()

    /// <summary>
    /// Gets a table reference from the client
    /// </summary>
    let from<'T when 'T :> BaseModel and 'T : (new : unit -> 'T)> (client: Client) =
        client.From<'T>()

    /// <summary>
    /// Calls a remote procedure
    /// </summary>
    let rpc procedureName parameters (client: Client) =
        client.Rpc(procedureName, parameters) |> Async.AwaitTask

    /// <summary>
    /// Calls a remote procedure with a typed response
    /// </summary>
    let rpcTyped<'T> procedureName parameters (client: Client) =
        client.Rpc<'T>(procedureName, parameters) |> Async.AwaitTask

/// <summary>
/// F# module with idiomatic functions for authentication
/// </summary>
[<RequireQualifiedAccess>]
module Auth =

    /// <summary>
    /// Signs in with email and password
    /// </summary>
    let signIn email password (client: Client) = async {
        return! client.Auth.SignInAsyncF(email, password)
    }

    /// <summary>
    /// Signs up with email and password
    /// </summary>
    let signUp email password (client: Client) = async {
        return! client.Auth.SignUpAsyncF(email, password)
    }

    /// <summary>
    /// Signs up with email, password, and additional options
    /// </summary>
    let signUpWithOptions email password options (client: Client) = async {
        return! client.Auth.SignUp(email, password, options) |> Async.AwaitTask
    }

    /// <summary>
    /// Signs out the current user
    /// </summary>
    let signOut (client: Client) = async {
        do! client.Auth.SignOutAsyncF()
    }

    /// <summary>
    /// Gets the current session as an option
    /// </summary>
    let currentSession (client: Client) =
        ofObj client.Auth.CurrentSession

    /// <summary>
    /// Gets the current user as an option
    /// </summary>
    let currentUser (client: Client) =
        ofObj client.Auth.CurrentUser

    /// <summary>
    /// Retrieves the current session
    /// </summary>
    let retrieveSession (client: Client) = async {
        return! client.Auth.RetrieveSessionAsyncF()
    }

    /// <summary>
    /// Refreshes the current session
    /// </summary>
    let refreshSession (client: Client) = async {
        return! client.Auth.RefreshSession() |> Async.AwaitTask
    }

    /// <summary>
    /// Sends a password reset email
    /// </summary>
    let resetPasswordForEmail email (client: Client) = async {
        return! client.Auth.ResetPasswordForEmail(email) |> Async.AwaitTask
    }

    /// <summary>
    /// Updates the current user
    /// </summary>
    let updateUser attributes (client: Client) = async {
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
    let connect (client: Client) = async {
        do! client.Realtime.ConnectAsyncF()
    }

    /// <summary>
    /// Disconnects from Realtime
    /// </summary>
    let disconnect (client: Client) = async {
        do! client.Realtime.DisconnectAsyncF()
    }

    /// <summary>
    /// Sets the auth token for Realtime
    /// </summary>
    let setAuth token (client: Client) =
        client.Realtime.SetAuth(token)

    /// <summary>
    /// Gets a channel by name
    /// </summary>
    let channel name (client: Client) =
        client.Realtime.Channel(name)

/// <summary>
/// F# module with idiomatic functions for Storage
/// </summary>
[<RequireQualifiedAccess>]
module Storage =

    /// <summary>
    /// Gets a storage bucket
    /// </summary>
    let bucket bucketId (client: Client) =
        client.Storage.From(bucketId)

    /// <summary>
    /// Uploads a file to storage
    /// </summary>
    let upload bucketId path fileBytes (client: Client) = async {
        let bucket = client.Storage.From(bucketId)
        return! bucket.Upload(path, fileBytes) |> Async.AwaitTask
    }

    /// <summary>
    /// Downloads a file from storage
    /// </summary>
    let download bucketId path (client: Client) = async {
        let bucket = client.Storage.From(bucketId)
        return! bucket.Download(path) |> Async.AwaitTask
    }

    /// <summary>
    /// Deletes files from storage
    /// </summary>
    let delete bucketId paths (client: Client) = async {
        let bucket = client.Storage.From(bucketId)
        return! bucket.Remove(paths) |> Async.AwaitTask
    }

    /// <summary>
    /// Lists files in a bucket
    /// </summary>
    let list bucketId path (client: Client) = async {
        let bucket = client.Storage.From(bucketId)
        return! bucket.List(path) |> Async.AwaitTask
    }

    /// <summary>
    /// Gets a public URL for a file
    /// </summary>
    let publicUrl bucketId path (client: Client) =
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
    let invoke functionName (client: Client) = async {
        return! client.Functions.Invoke(functionName) |> Async.AwaitTask
    }

    /// <summary>
    /// Invokes an edge function with parameters
    /// </summary>
    let invokeWith functionName parameters (client: Client) = async {
        return! client.Functions.Invoke(functionName, parameters) |> Async.AwaitTask
    }

    /// <summary>
    /// Invokes an edge function with a typed response
    /// </summary>
    let invokeTyped<'T> functionName (client: Client) = async {
        return! client.Functions.Invoke<'T>(functionName) |> Async.AwaitTask
    }

    /// <summary>
    /// Invokes an edge function with parameters and a typed response
    /// </summary>
    let invokeTypedWith<'T> functionName parameters (client: Client) = async {
        return! client.Functions.Invoke<'T>(functionName, parameters) |> Async.AwaitTask
    }
