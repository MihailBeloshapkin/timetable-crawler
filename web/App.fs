module web.App
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Http
open Giraffe
open web.Views
open web.Models
open Crawler.Base

let indexHandler (name : string) =
    let facultyList = get_faculty_list ()
    let view      = index facultyList
    htmlView view

let selectHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! model = ctx.BindFormAsync<web.Models.Faculty>()
            return! redirectTo false "/" next ctx
        }

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
            ]
        POST >=>
            choose [
                route "/input" >=> inputHandler
                route "/selectSub" >=> selectHandler
            ]
        setStatusCode 404 >=> text "Not Found" ]
