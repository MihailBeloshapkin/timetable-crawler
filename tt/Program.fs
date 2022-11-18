open System
open System.Net
open System.Net.Http
open System.Text.RegularExpressions
open HtmlAgilityPack

let base_url = "https://timetable.spbu.ru" 

let get_genera_info (url : string) =
    async {
        try
            let client = new HttpClient()
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
    [for matches in (Regex("<a href\s*=\s*\"?([^\"]+)\"?\s*>", RegexOptions.Compiled)
    .Matches(html) : MatchCollection) -> matches.Groups.[1].Value]

let get_href_info data =
    let hd = new HtmlDocument()
    hd.LoadHtml(data)
    ()

let ex =
    async {
        let! data = get_genera_info "https://timetable.spbu.ru"
        let hrefs = 
            match data with 
            | Some x -> 
              get_tags x 
              |> List.filter (fun s -> s.[0] = '/')
              |> List.map (fun s -> String.concat "" [base_url; s])
            | _ -> []
        let new_data =
            hrefs
            |> List.map get_genera_info
        return hrefs
    }



[<EntryPoint>]
let main argv =
    let hrefs = ex |> Async.RunSynchronously
    0