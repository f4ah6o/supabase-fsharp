namespace Supabase.FSharp

open System
open System.Runtime.CompilerServices

/// <summary>
/// Extensions for working with nullable types as F# options
/// </summary>
[<AutoOpen>]
module OptionExtensions =

    /// <summary>
    /// Converts a nullable value to an F# option
    /// </summary>
    let ofNullable (value: Nullable<'T>) =
        if value.HasValue then Some value.Value else None

    /// <summary>
    /// Converts a reference type to an F# option
    /// </summary>
    let ofObj (value: 'T when 'T: null) =
        match value with
        | null -> None
        | v -> Some v

    /// <summary>
    /// Converts an F# option to a nullable value
    /// </summary>
    let toNullable (opt: 'T option) =
        match opt with
        | Some v -> Nullable<'T>(v)
        | None -> Nullable<'T>()

    /// <summary>
    /// Converts an F# option to a reference type (possibly null)
    /// </summary>
    let toObj (opt: 'T option when 'T: null) =
        match opt with
        | Some v -> v
        | None -> Unchecked.defaultof<'T>

    /// <summary>
    /// Gets the value of an option or a default value
    /// </summary>
    let defaultValue defaultVal opt =
        match opt with
        | Some v -> v
        | None -> defaultVal

    /// <summary>
    /// Gets the value of an option or computes a default value
    /// </summary>
    let defaultWith defaultThunk opt =
        match opt with
        | Some v -> v
        | None -> defaultThunk()

/// <summary>
/// Type extensions for Supabase types to provide F#-friendly option handling
/// </summary>
[<AutoOpen>]
module SupabaseOptionExtensions =
    open Supabase.Gotrue

    type Session with
        /// <summary>
        /// Gets the access token as an option
        /// </summary>
        member this.AccessTokenOption =
            ofObj this.AccessToken

        /// <summary>
        /// Gets the refresh token as an option
        /// </summary>
        member this.RefreshTokenOption =
            ofObj this.RefreshToken

        /// <summary>
        /// Gets the user as an option
        /// </summary>
        member this.UserOption =
            ofObj this.User

    type User with
        /// <summary>
        /// Gets the email as an option
        /// </summary>
        member this.EmailOption =
            ofObj this.Email

        /// <summary>
        /// Gets the phone as an option
        /// </summary>
        member this.PhoneOption =
            ofObj this.Phone
