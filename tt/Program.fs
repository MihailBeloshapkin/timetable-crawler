module Crawler.Base
open System.Net.Http
open System.Text.RegularExpressions
open HtmlAgilityPack
open System.Linq

let base_url = "https://timetable.spbu.ru" 

let client = new HttpClient()

type StudyDirection = {
    name : string;
    data : list<string * string> // Year and href
}

type StudyProgram = {
    name : string;
    programs : list<StudyDirection>
}

let process_get_request (url : string) =
    async {
        try
            let! response = client.GetAsync(url) |> Async.AwaitTask
            response.EnsureSuccessStatusCode () |> ignore
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            return Some content
        with
            | _ ->
                printfn "Error: failed to access %s" url
                return None
    }

let ind = ref 0

// Get faculty names and their url's.
let get_tags html =
    [for matches in (Regex("<a href\s*=\s*\"?([^\"]+)\"?\s*>([^\"]+)</a>", RegexOptions.Compiled)
    .Matches(html) : MatchCollection) -> (matches.Groups.[1].Value, matches.Groups.[2].Value)]

let getSpecInfo html =
    let helper (el : HtmlNode) =
        let names = [for matches in (Regex("<div class\s*=\s*\"col-sm-5\">([^#<>]+)</div>", RegexOptions.Compiled)
                     .Matches(el.InnerHtml) : MatchCollection) -> matches.Groups.[1].Value]
        
        let years = [for matches in (Regex("[^_]20([0-9])([0-9])", RegexOptions.Compiled)
                      .Matches(el.InnerHtml) : MatchCollection) -> matches.Groups.[0].Value] 
        
        let href = [for matches in (Regex("<a href\s*=\s*\"?([^\"]+)\"?", RegexOptions.Compiled)
                     .Matches(el.InnerHtml) : MatchCollection) -> matches.Groups.[1].Value]
        match (names, years, href) with
        | [ n ], y, h when List.length y = List.length h ->
          let yh = List.zip y h
          Some {
            name = n
            data = yh
          }
        | _ -> None

    let doc = new HtmlDocument()
    doc.LoadHtml html
    
    try
      doc.DocumentNode.SelectNodes("//li[@class='common-list-item row']")
        .Select(helper)
        .ToList()
      |> Seq.toList
      |> List.fold (fun acc x -> match x with | Some x -> acc @ [x] | None -> acc) []
      |> (fun l -> 
          Some { 
            name = string !ind
            programs = l
          })  
    with 
    | _ -> None

// Get base elements on the chosen faculty page.
let getAllSpecs html =
    let doc = new HtmlDocument()
    doc.LoadHtml html
    let studyPrograms = 
      doc.DocumentNode.SelectNodes("//div[@class='panel panel-default']")
        .Select(fun t -> t.InnerHtml)
        .ToList() |> Seq.toList
    studyPrograms
    |> List.map getSpecInfo

let ex =
    async {
        let! data = process_get_request "https://timetable.spbu.ru/MATH"
        let hrefs = 
            match data with 
            | Some x -> 
              let a = getAllSpecs x
              get_tags x 
              |> List.filter (fun (s, _) -> String.length s > 0 && s.[0] = '/')
              |> List.map (fun (s, name) -> (String.concat "" [base_url; s]), name)
            | _ -> []
        let new_data =
            hrefs
            |> List.map 
                (fun (s, name) -> 
                    let html = process_get_request s |> Async.RunSynchronously
                    (html, name))
            |> List.fold (fun acc -> function | Some h, name -> (name, h) :: acc | _ -> acc) []
            |> List.rev
        return new_data
    }

// Get list of faculties
let get_faculty_list () =
    let hrefs = ex |> Async.RunSynchronously
    async {
        let! data = process_get_request base_url
        return 
            match data with 
            | Some x -> 
              get_tags x 
              |> List.filter (fun (s, _) -> String.length s > 0 && s.[0] = '/')
            | _ -> []
    } |> Async.RunSynchronously



[<EntryPoint>]
let main argv =
    let l = get_faculty_list ()
    let hrefs = ex |> Async.RunSynchronously
    0
