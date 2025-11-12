module ContactApp.ContactService

open System
open System.Threading
open System.Threading.Tasks
open ContactApp.Models
open Supabase
open Supabase.FSharp

// Global Supabase client reference
let mutable private supabaseClient: Client option = None

// Initialize Supabase client
let initializeSupabase (url: string) (key: string) = async {
    let options =
        supabaseOptions {
            schema "public"
            autoRefreshToken true
            autoConnectRealtime false
        }

    let client = Supabase.create url key options
    let! initializedClient = Supabase.initialize client
    supabaseClient <- Some initializedClient
    return initializedClient
}

let private getClient() =
    match supabaseClient with
    | Some client -> client
    | None -> failwith "Supabase client not initialized. Call initializeSupabase first."

// Count contacts (with artificial delay like original)
let count() = task {
    Thread.Sleep 2000
    let client = getClient()
    let! response = client.From<ContactTable>().Get()
    return response.Models.Count
}

// Search contacts
let searchContact (search: string) = task {
    let client = getClient()
    let! response = client.From<ContactTable>().Get()
    return
        response.Models
        |> Seq.map (fun c -> c.ToDomain())
        |> Seq.filter(fun c ->
            c.First.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            c.Last.Contains(search, StringComparison.OrdinalIgnoreCase))
}

// Get paginated contacts
let all page = task {
    let client = getClient()
    let pageSize = 5
    let rangeFrom = (page - 1) * pageSize
    let rangeTo = rangeFrom + pageSize - 1

    let! response =
        client
            .From<ContactTable>()
            .Range(rangeFrom, rangeTo)
            .Get()

    return response.Models |> Seq.map (fun c -> c.ToDomain())
}

// Add new contact
let add (contact: Contact) = task {
    let client = getClient()
    let contactTable = ContactTable()
    contactTable.first <- contact.First
    contactTable.last <- contact.Last
    contactTable.phone <- contact.Phone
    contactTable.email <- contact.Email

    let! response = client.From<ContactTable>().Insert(contactTable)
    let newContact = response.Models.[0].ToDomain()
    return newContact
}

// Find contact by ID
let find id = task {
    let client = getClient()
    let! response =
        client
            .From<ContactTable>()
            .Where(fun c -> c.id = id)
            .Single()
    return response.ToDomain()
}

// Update contact
let update (contact: Contact) = task {
    let client = getClient()
    let contactTable = ContactTable.FromDomain(contact)

    let! response =
        client
            .From<ContactTable>()
            .Update(contactTable)

    return ()
}

// Delete contact
let delete id = task {
    let client = getClient()
    let! _ =
        client
            .From<ContactTable>()
            .Where(fun c -> c.id = id)
            .Delete()
    return ()
}

// Validate email uniqueness
let validateEmail (contact: Contact) = task {
    let client = getClient()
    let! response =
        client
            .From<ContactTable>()
            .Where(fun c -> c.email = contact.Email)
            .Get()

    let existingContact =
        response.Models
        |> Seq.map (fun c -> c.ToDomain())
        |> Seq.tryFind (fun c -> c.Id <> contact.Id)

    return existingContact.IsNone
}

// Get all contacts (for archiving)
let getAllContacts() = task {
    let client = getClient()
    let! response = client.From<ContactTable>().Get()
    return response.Models |> Seq.map (fun c -> c.ToDomain()) |> ResizeArray
}
