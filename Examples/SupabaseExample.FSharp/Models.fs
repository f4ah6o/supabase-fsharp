namespace SupabaseExample.FSharp.Models

open Supabase.Postgrest.Attributes
open Supabase.Postgrest.Models

/// <summary>
/// Example Movie model for demonstrating Supabase usage
/// </summary>
[<Table("movies")>]
type Movie() =
    inherit BaseModel()

    [<PrimaryKey("id")>]
    member val Id = 0 with get, set

    [<Column("name")>]
    member val Name = "" with get, set

    [<Column("created_at")>]
    member val CreatedAt = System.DateTime.MinValue with get, set

/// <summary>
/// Example User model
/// </summary>
[<Table("users")>]
type UserProfile() =
    inherit BaseModel()

    [<PrimaryKey("id")>]
    member val Id = "" with get, set

    [<Column("username")>]
    member val Username = "" with get, set

    [<Column("email")>]
    member val Email = "" with get, set

    [<Column("created_at")>]
    member val CreatedAt = System.DateTime.MinValue with get, set
