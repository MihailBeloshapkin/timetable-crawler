open System
open System.Net.Http
open System.Text.RegularExpressions

let base_url = "https://timetable.spbu.ru" 

let client = new HttpClient()

type Data = {
    subject_name : string;
    date : string;
    lecturer_name : string
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

let get_tags html =
    [for matches in (Regex("<a href\s*=\s*\"?([^\"]+)\"?\s*>([^\"]+)</a>", RegexOptions.Compiled)
    .Matches(html) : MatchCollection) -> (matches.Groups.[1].Value, matches.Groups.[2].Value)]

let ex =
    async {
        let! data = process_get_request base_url
        let hrefs = 
            match data with 
            | Some x -> 
              let a = get_tags x
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

[<EntryPoint>]
let main argv =
    let hrefs = ex |> Async.RunSynchronously
    0