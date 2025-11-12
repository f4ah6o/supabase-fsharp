module ContactApp.templates.edit

open Oxpecker.ModelValidation
open Oxpecker.ViewEngine
open Oxpecker.Htmx
open ContactApp.Models
open ContactApp.templates.shared

let html (contact: ModelState<ContactDTO>) =
    Fragment() {
        form(action= $"/contacts/{contact.Value(_.id >> string)}/edit", method="post") {
            fieldset() {
                legend() { "Contact Values" }
                contactFields.html contact
                button() { "Save" }
            }
        }

        button(hxDelete= $"/contacts/{contact.Value(_.id >> string)}",
               hxPushUrl="true",
               hxConfirm="Are you sure you want to delete this contact?",
               hxTarget="body",
               hxTrigger="delete-btn") { "Delete Contact" }

        p() {
            a(href="/contacts") { "Back" }
        }
    }
    |> layout.html
