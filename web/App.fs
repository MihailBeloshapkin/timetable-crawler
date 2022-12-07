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

module CurrentConfiguration =
    type Program =
    | Bacheor
    | Magistrate
    | FullGraduate
    | No

    let mutable allAboutFaculty = []
    let mutable urlAndFac = []
    let mutable facultyUrl = ""    
    let mutable program = No
    let mutable year = 0
    let mutable group = ""

// First page
let indexHandler () =
    let facultyList = get_faculty_list ()
    CurrentConfiguration.urlAndFac <- facultyList
    let view = facultyList |> List.map snd |> faculties
    htmlView view

// Change view
let indexYearHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
      let data = getAllSpecs CurrentConfiguration.facultyUrl    
      CurrentConfiguration.allAboutFaculty <- data
      let view = data |> List.map (fun x -> x.name) |> studyProgram 
      htmlView view next ctx

let selectFacultyHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! model = ctx.BindFormAsync<web.Models.Faculty>()
            let u = CurrentConfiguration.urlAndFac 
                    |> List.find (snd >> (=) model.Faculty)
            CurrentConfiguration.facultyUrl <- u |> fst
            // let data = getAllSpecs CurrentConfiguration.facultyUrl
            return! redirectTo false Urls.year next ctx
        }

let inputHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! model = ctx.BindFormAsync<Input>()
            return! redirectTo false "/" next ctx
        }

let studyProgramHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! model = ctx.BindFormAsync<web.Models.StudyProgram>()
            let program = CurrentConfiguration.allAboutFaculty |> List.find (fun x -> x.name = model.StudyProgram)
            let view = program.programs |> List.map (fun x -> x.name) |> studyDirection 
            return! htmlView view next ctx 
        }

let app : HttpFunc -> Http.HttpContext -> HttpFuncResult = compose (route "/") (Successful.OK "A")

let webApp : HttpFunc -> Http.HttpContext -> HttpFuncResult =
    choose [
        GET >=>
            choose [
                route "/" >=> indexHandler ()
                route "/year" >=> indexYearHandler
            ]
        POST >=>
            choose [
                route "/input" >=> inputHandler
                route "/studyProgram" >=> studyProgramHandler
                route Urls.faculty >=> selectFacultyHandler
            ]
        setStatusCode 404 >=> text "Not Found" ]
