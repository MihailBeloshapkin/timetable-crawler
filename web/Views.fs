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

let flist fl =
    form [ _action "/selectFaculty"; _method "POST"] [
        select [ _name "Faculty" ] [ 
            for i in fl -> option [] [ str i ]
        ]
        input [ _type "submit" ]
    ]

let faculties facList =
    [
        partial ()
        p [] [ encodedText "Input your data" ]
        // inputData ()
        flist facList
    ] |> layout

let studyProgram data =
    [
        partial ()  
        form [ _action "/studyProgram"; _method "POST"] [
          select [ _name "StudyProgram" ] [ 
              for i in data -> option [] [ str i ]
          ]
          input [ _type "submit" ]
      ]
    ]
    |> layout

let studyDirection data =
    [
        partial ()  
        form [ _action "/studyDirection"; _method "POST"] [
          select [ _name "StudyProgram" ] [ 
              for i in data -> option [] [ str i ]
          ]
          input [ _type "submit" ]
      ]
    ]
    |> layout