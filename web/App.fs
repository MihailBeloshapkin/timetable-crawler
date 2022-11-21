module web.App
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Http
open Giraffe
open web.Views
open web.Models
open Crawler.Base

module Urls =

    let index = "/"
    let faculty = "/selectFaculty"
    let year = "/year"

let indexHandler () =
    let facultyList = get_faculty_list ()
    let view = facultyList |> List.map snd |> faculties
    htmlView view

let index1Handler () =
    let view = year ()
    htmlView view

let selectFacultyHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! model = ctx.BindFormAsync<web.Models.Faculty>()
            return! redirectTo false Urls.year next ctx
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
                route Urls.index >=> indexHandler ()
                route Urls.year >=> index1Handler ()
            ]
        POST >=>
            choose [
                route "/input" >=> inputHandler
                route Urls.faculty >=> selectFacultyHandler
            ]
        setStatusCode 404 >=> text "Not Found" ]
