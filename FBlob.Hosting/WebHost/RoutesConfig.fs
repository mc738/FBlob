module FBlob.Hosting.WebHost.RoutesConfig

open System
open FBlob.Hosting.WebHost.Http.ContentTypes
open FBlob.Hosting.WebHost.Routing

let homeRef =
    Guid.Parse("4f789b6f-7c9c-4531-aab0-81163cac9203")

let mainCssRef =
    Guid.Parse("893ca70a-e13e-4a51-8191-c2c65e849908")

let mainJsRef =
    Guid.Parse("e8ec157a-f3fd-4654-a66f-ed348047453d")

let private index =
    { Route = "/"
      Type = Static { Reference = homeRef } }

// Path = "/home/max/Projects/SemiFunctionalServer/ExampleWebSite/index.html"
//ContentType = ContentType.Html } }

let private mainCss =
    { Route = "/css/style.css"
      Type = Static { Reference = mainCssRef } }

let private indexJs =
    { Route = "/js/index.js"
      Type = Static { Reference = mainJsRef } }

let routes =
    seq {
        index
        mainCss
        indexJs
    }