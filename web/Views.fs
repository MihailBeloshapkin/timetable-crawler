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
    h1 [] [ encodedText "web" ]

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

let index () =
    [
        partial()
        p [] [ encodedText "Hello" ]
        inputData ()
    ] |> layout
