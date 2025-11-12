module ContactApp.templates.shared.layout

open ContactApp.Tools
open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let html (content: HtmlElement) (ctx: HttpContext) =
    html() {
        head() {
            title() { "Contact App" }
            script(src="https://unpkg.com/htmx.org@1.9.10") { "" }
            link(rel="stylesheet", href="/site.css")
        }
        body() {
            header() {
                h1() {
                    p() { raw "Contact" }
                    p() { "App" }
                }
            }
            let message = getFlashedMessage ctx
            if message <> "" then
                div(class'="alert fadeOut") { message }
            content
        }
    }
