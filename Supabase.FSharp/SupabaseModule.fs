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
    let initialize (client: ISupabaseClient<_,_,_,_,_,_>) =
        client.InitializeAsync() |> Async.AwaitTask

    /// <summary>
    /// Gets a table reference from the client
    /// </summary>
    let from<'T when 'T :> BaseModel and 'T : (new : unit -> 'T)> (client: ISupabaseClient<_,_,_,_,_,_>) =
        client.From<'T>()

    /// <summary>
    /// Calls a remote procedure
    /// </summary>
    let rpc procedureName parameters (client: ISupabaseClient<_,_,_,_,_,_>) =
        client.Rpc(procedureName, parameters) |> Async.AwaitTask

    /// <summary>
    /// Calls a remote procedure with a typed response
    /// </summary>
    let rpcTyped<'T> procedureName parameters (client: ISupabaseClient<_,_,_,_,_,_>) =
        client.Rpc<'T>(procedureName, parameters) |> Async.AwaitTask

/// <summary>
/// F# module with idiomatic functions for authentication
/// </summary>
[<RequireQualifiedAccess>]
module Auth =

    /// <summary>
    /// Signs in with email and password
    /// </summary>
    let signIn (email: string) (password: string) (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        return! client.Auth.SignIn(email, password) |> Async.AwaitTask
    }

    /// <summary>
    /// Signs up with email and password
    /// </summary>
    let signUp email password (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        return! client.Auth.SignUp(email, password) |> Async.AwaitTask
    }

    /// <summary>
    /// Signs up with email, password, and additional options
    /// </summary>
    let signUpWithOptions email password options (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        return! client.Auth.SignUp(email, password, options) |> Async.AwaitTask
    }

    /// <summary>
    /// Signs out the current user
    /// </summary>
    let signOut (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        do! client.Auth.SignOut() |> Async.AwaitTask
    }

    /// <summary>
    /// Gets the current session as an option
    /// </summary>
    let currentSession (client: ISupabaseClient<_,_,_,_,_,_>) =
        ofObj client.Auth.CurrentSession

    /// <summary>
    /// Gets the current user as an option
    /// </summary>
    let currentUser (client: ISupabaseClient<_,_,_,_,_,_>) =
        ofObj client.Auth.CurrentUser

    /// <summary>
    /// Retrieves the current session
    /// </summary>
    let retrieveSession (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        return! client.Auth.RetrieveSessionAsync() |> Async.AwaitTask
    }

    /// <summary>
    /// Refreshes the current session
    /// </summary>
    let refreshSession (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        return! client.Auth.RefreshSession() |> Async.AwaitTask
    }

    /// <summary>
    /// Sends a password reset email
    /// </summary>
    let resetPasswordForEmail (email: string) (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        return! client.Auth.ResetPasswordForEmail(email) |> Async.AwaitTask
    }

    /// <summary>
    /// Updates the current user
    /// </summary>
    let updateUser attributes (client: ISupabaseClient<_,_,_,_,_,_>) = async {
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
    let connect (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        let! _ = client.Realtime.ConnectAsync() |> Async.AwaitTask
        return ()
    }

    /// <summary>
    /// Disconnects from Realtime
    /// </summary>
    let disconnect (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        client.Realtime.Disconnect() |> ignore
    }

    /// <summary>
    /// Sets the auth token for Realtime
    /// </summary>
    let setAuth token (client: ISupabaseClient<_,_,_,_,_,_>) =
        client.Realtime.SetAuth(token)

    /// <summary>
    /// Gets a channel by name
    /// </summary>
    let channel name (client: ISupabaseClient<_,_,_,_,_,_>) =
        client.Realtime.Channel(name)

/// <summary>
/// F# module with idiomatic functions for Storage
/// </summary>
[<RequireQualifiedAccess>]
module Storage =

    /// <summary>
    /// Gets a storage bucket
    /// </summary>
    let bucket bucketId (client: ISupabaseClient<_,_,_,_,_,_>) =
        client.Storage.From(bucketId)

    /// <summary>
    /// Uploads a file to storage
    /// </summary>
    let upload (bucketId: string) (path: string) (fileBytes: byte[]) (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        let bucket = client.Storage.From(bucketId)
        return! bucket.Upload(fileBytes, path) |> Async.AwaitTask
    }

    /// <summary>
    /// Downloads a file from storage
    /// </summary>
    let download (bucketId: string) (path: string) (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        let bucket = client.Storage.From(bucketId)
        return!
            bucket.Download(path, ?transformOptions = None, ?onProgress = None)
            |> Async.AwaitTask
    }

    /// <summary>
    /// Deletes files from storage
    /// </summary>
    let delete (bucketId: string) (paths: string list) (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        let bucket = client.Storage.From(bucketId)
        let dotnetList = System.Collections.Generic.List<string>(paths)
        return! bucket.Remove(dotnetList) |> Async.AwaitTask
    }

    /// <summary>
    /// Lists files in a bucket
    /// </summary>
    let list (bucketId: string) (path: string) (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        let bucket = client.Storage.From(bucketId)
        return! bucket.List(path) |> Async.AwaitTask
    }

    /// <summary>
    /// Gets a public URL for a file
    /// </summary>
    let publicUrl bucketId path (client: ISupabaseClient<_,_,_,_,_,_>) =
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
    let invoke functionName (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        return! client.Functions.Invoke(functionName) |> Async.AwaitTask
    }

    /// <summary>
    /// Invokes an edge function with parameters
    /// </summary>
    let invokeWith functionName parameters (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        return! client.Functions.Invoke(functionName, parameters) |> Async.AwaitTask
    }

    /// <summary>
    /// Invokes an edge function with a typed response
    /// </summary>
    let invokeTyped<'T when 'T : not struct> functionName (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        return! client.Functions.Invoke<'T>(functionName) |> Async.AwaitTask
    }

    /// <summary>
    /// Invokes an edge function with parameters and a typed response
    /// </summary>
    let invokeTypedWith<'T when 'T : not struct> functionName parameters (client: ISupabaseClient<_,_,_,_,_,_>) = async {
        return! client.Functions.Invoke<'T>(functionName, parameters) |> Async.AwaitTask
    }
