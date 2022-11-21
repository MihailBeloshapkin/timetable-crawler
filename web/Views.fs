module web.Views

open web.Models
open Giraffe.ViewEngine

let layout (content: XmlNode list) =
    html [] [
        head [] [
            title []  [ encodedText "web" ]
            link 
                [ 
                    _rel "stylesheet"
                    _type "text/css"
                    _href "/main.css" 
                ]
            ]
        body [] content
    ]

let partial () =
    h1 [] [ encodedText "ZOV" ]

let inputData () = 
    form [ _action "/input"; _method "POST" ] [
        div [] [
            label [] [ str "Faculty" ]
            input [ _name "Faculty"; _type "text" ]
        ]
        div [] [
            label [] [ str "Speciality" ]
            input [ _name "Speciality"; _type "text" ]
        ]
        div [] [
            label [] [ str "Year" ]
            input [ _name "Year"; _type "text" ]
        ]
        input [ _type "submit" ]
    ]

let slist fl =
    form [ _action "/selectSub"; _method "POST"] [
        select [ _name "Faculty" ] [ 
            for i in fl -> option [] [ str i ]
            //option [] [str "Math"]; 
            //option [] [str "Phyth"]; 
            //option [] [str "INOZ"] 
        ]
        input [ _type "submit" ]
    ]

let index facList =
    [
        partial()
        p [] [ encodedText "Input your data" ]
        // inputData ()
        slist facList
    ] |> layout