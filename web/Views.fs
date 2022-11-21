module web.Views

open web.Models
open Giraffe.ViewEngine

let layout (content: XmlNode list) =
    html [] [
        head [] [
            title []  [ encodedText "web" ]
            link [ 
                _rel "stylesheet"
                _type "text/css"
                _href "/main.css" 
            ]
        ]
        body [] content
    ]

let partial () =
    h1 [] [ encodedText "Spbu Timetable" ]

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

let flist fl =
    form [ _action "/selectFaculty"; _method "POST"] [
        select [ _name "Faculty" ] [ 
            for i in fl -> option [] [ str i ]
        ]
        input [ _type "submit" ]
    ]

let faculties facList =
    [
        partial()
        p [] [ encodedText "Input your data" ]
        // inputData ()
        flist facList
    ] |> layout

let year () =
    [
        partial()
        p [] [ encodedText "Right!" ]
    ]
    |> layout