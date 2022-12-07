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
    
    let mutable allAboutFaculty = []
    let mutable currentStudyProgram = { name = ""; programs = [] }
    let mutable urlAndFac = []
    let mutable facultyUrl = ""    
    let mutable group = ""

    let getAllAboutFaculty () = allAboutFaculty 

// USE ONLY THIS TO COMPARE NAMES FROM REQUESTS
let namesEqual (n1 : string) (n2 : string) =
    let toNormalForm s = 
        s |> Seq.toList |> List.filter (fun x -> x <> ' ' && x <> '\n' && x <> '\r' && x <> '\t')
    (toNormalForm n1) = (toNormalForm n2)

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
            let a = CurrentConfiguration.allAboutFaculty
            let program = a |> List.find (fun x -> namesEqual x.name model.StudyProgram)
            CurrentConfiguration.currentStudyProgram <- program
            let view = program.programs |> List.map (fun x -> x.name) |> studyDirection 
            return! htmlView view next ctx
        }

let studyDirectionHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! model = ctx.BindFormAsync<web.Models.StudyDirection>()
            let p = CurrentConfiguration.currentStudyProgram.programs
            let directon = p |> List.find (fun x -> namesEqual model.StudyDirection x.name)
            let view = directon.data |> List.map fst |> years
            return! htmlView view next ctx
        }

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
                route "/studyDirection" >=> studyDirectionHandler
                route Urls.faculty >=> selectFacultyHandler
            ]
        setStatusCode 404 >=> text "Not Found" ]
