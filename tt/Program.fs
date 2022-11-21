module Crawler.Base
open System.Net.Http
open System.Text.RegularExpressions
open HtmlAgilityPack
open System.Linq

let base_url = "https://timetable.spbu.ru" 

let client = new HttpClient()

type Data = {
    subject_name : string;
    date : string;
    lecturer_name : string
}

module StudyPrograms =
    let bacheor = "#studyProgramLevel1"
    let magistrate = "#studyProgramLevel4"

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

// Get faculty names and their url's.
let get_tags html =
    [for matches in (Regex("<a href\s*=\s*\"?([^\"]+)\"?\s*>([^\"]+)</a>", RegexOptions.Compiled)
    .Matches(html) : MatchCollection) -> (matches.Groups.[1].Value, matches.Groups.[2].Value)]

// Get base elements on the chosen faculty page.
let get_specs html =
    let helper (el : HtmlNode) =
        [for matches in (Regex("<div class\s*=\s*\"col-sm-5\">([^#<>]+)</div>", RegexOptions.Compiled)
        .Matches(el.InnerHtml) : MatchCollection) -> matches.Groups]
    let doc = new HtmlDocument() 
    doc.LoadHtml html
    
    doc.DocumentNode.SelectNodes("//li[@class='common-list-item row']")
        .Select(helper)
        .ToList()
    |> Seq.toList 
    |> List.filter (fun l -> List.length l = 1) 
    |> List.map (fun x -> List.head x)
    |> List.map (fun x -> x.[1].Value)
    
let ex =
    async {
        let! data = process_get_request base_url
        let hrefs = 
            match data with 
            | Some x -> 
              // let a = get_specs x
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
//    let hrefs = ex |> Async.RunSynchronously
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
