module ContactApp.Models
open System.ComponentModel.DataAnnotations
open Supabase.Postgrest.Attributes
open Supabase.Postgrest.Models


type Contact = {
    Id: int
    First: string
    Last: string
    Phone: string
    Email: string
}

[<CLIMutable>]
type ContactDTO = {
    id: int
    [<Required>]
    first: string
    [<Required>]
    last: string
    [<Required>]
    phone: string
    [<Required>]
    [<EmailAddress>]
    email: string
} with
    member this.ToDomain() =
        { Id = this.id; First = this.first; Last = this.last; Phone = this.phone; Email = this.email }
    static member FromDomain(contact: Contact) =
        { id = contact.Id; first = contact.First; last = contact.Last; phone = contact.Phone; email = contact.Email }

// Supabase table model
[<Table("contacts")>]
type ContactTable() =
    inherit BaseModel()
    [<PrimaryKey("id", false)>]
    member val id = 0 with get, set
    [<Column("first")>]
    member val first = "" with get, set
    [<Column("last")>]
    member val last = "" with get, set
    [<Column("phone")>]
    member val phone = "" with get, set
    [<Column("email")>]
    member val email = "" with get, set

    member this.ToDomain() =
        { Id = this.id; First = this.first; Last = this.last; Phone = this.phone; Email = this.email }

    static member FromDomain(contact: Contact) =
        let table = ContactTable()
        table.id <- contact.Id
        table.first <- contact.First
        table.last <- contact.Last
        table.phone <- contact.Phone
        table.email <- contact.Email
        table
