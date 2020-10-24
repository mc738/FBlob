module FBlob.Hosting.WebHost.RoutesConfig

open FBlob.Hosting.WebHost.Http.ContentTypes
open FBlob.Hosting.WebHost.Routing

let private index =
    { RoutePaths = seq { "/" }
      RouteType =
          File
              { Path = "/home/max/Projects/SemiFunctionalServer/ExampleWebSite/index.html"
                ContentType = ContentType.Html } }

let private mainCss =
    { RoutePaths = seq { "/css/style.css" }
      RouteType =
          File
              { Path = "/home/max/Projects/SemiFunctionalServer/ExampleWebSite/css/style.css"
                ContentType = ContentType.Css } }

let private indexJs =
    { RoutePaths = seq { "/js/index.js" }
      RouteType =
          File
              { Path = "/home/max/Projects/SemiFunctionalServer/ExampleWebSite/js/index.js"
                ContentType = ContentType.JavaScript } }



let routes =
    seq {
        index
        mainCss
        indexJs
    }
