namespace Supabase.FSharp

open System
open System.Threading.Tasks
open System.Runtime.CompilerServices

/// <summary>
/// Extensions for converting between Task&lt;T&gt; and Async&lt;'T&gt; for F# developers
/// </summary>
[<Extension>]
module AsyncExtensions =

    /// <summary>
    /// Converts a Task to an F# Async computation
    /// </summary>
    [<Extension>]
    let AsAsync (task: Task) =
        task |> Async.AwaitTask

    /// <summary>
    /// Converts a Task&lt;T&gt; to an F# Async&lt;'T&gt; computation
    /// </summary>
    [<Extension>]
    let AsAsyncResult (task: Task<'T>) =
        task |> Async.AwaitTask

    /// <summary>
    /// Converts an F# Async&lt;'T&gt; computation to a Task&lt;T&gt;
    /// </summary>
    [<Extension>]
    let AsTask (async: Async<'T>) =
        async |> Async.StartAsTask

/// <summary>
/// Type extensions for Supabase Client to provide F#-friendly async methods
/// </summary>
[<AutoOpen>]
module ClientAsyncExtensions =
    open Supabase
    open Supabase.Gotrue
    open Supabase.Realtime
    open Supabase.Storage

    type Client with
        /// <summary>
        /// Initializes the Supabase client using F# async
        /// </summary>
        member this.InitializeAsyncF() =
            this.InitializeAsync() |> Async.AwaitTask

    type IGotrueClient<'TUser, 'TSession> with
        /// <summary>
        /// Retrieves the session using F# async
        /// </summary>
        member this.RetrieveSessionAsyncF() =
            this.RetrieveSessionAsync() |> Async.AwaitTask

        /// <summary>
        /// Signs in with email and password using F# async
        /// </summary>
        member this.SignInAsyncF(email: string, password: string) =
            this.SignIn(email, password) |> Async.AwaitTask

        /// <summary>
        /// Signs up with email and password using F# async
        /// </summary>
        member this.SignUpAsyncF(email: string, password: string) =
            this.SignUp(email, password) |> Async.AwaitTask

        /// <summary>
        /// Signs out using F# async
        /// </summary>
        member this.SignOutAsyncF() =
            this.SignOut() |> Async.AwaitTask

    type IRealtimeClient<'TSocket, 'TChannel> with
        /// <summary>
        /// Connects to realtime using F# async
        /// </summary>
        member this.ConnectAsyncF() =
            this.ConnectAsync() |> Async.AwaitTask

        /// <summary>
        /// Disconnects from realtime using F# async
        /// </summary>
        member this.DisconnectAsyncF() =
            this.DisconnectAsync() |> Async.AwaitTask
