module web.App
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Http
open Giraffe
open web.Models
open web.Views

let indexHandler (name : string) =
    let greetings = sprintf "Hello %s, from Giraffe!" name
    let view      = index ()
    htmlView view

let inputHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! model = ctx.BindFormAsync<Input>()
            return! redirectTo false "/" next ctx
        }

let webApp : HttpFunc -> Http.HttpContext -> HttpFuncResult =
    choose [
        GET >=>
            choose [
                route "/" >=> indexHandler "world"
                routef "/hello/%s" indexHandler
            ]
        POST >=>
            choose [
                route "/input" >=> inputHandler
            ]
        setStatusCode 404 >=> text "Not Found" ]
