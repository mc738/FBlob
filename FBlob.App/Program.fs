// Learn more about F# at http://fsharp.org

open System
open System.Net.Http
open System.Text.Json
open FBlob.Core
open FBlob.Core.Sources
open FBlob.Core.Store
open FBlob.Hosting.Engine
open FBlob.Hosting.WebHost
open FBlob.Hosting.WebHost.DAL.Content
open FBlob.Hosting.WebHost.Routing
open Peeps

let path = "/home/max/Projects/FBlob/Projects/Inspired/quotes_1_99999.json"

let sourceName = "quotes_1_99999"

// TODO move this into `Store`
let createSourceContext =
    
    let settings : FileSourceSettings =
        {
            Path = path
            Name = sourceName
            Get = true
            Set = false
            Collection = true
            ContentType = "application/json"
        }
    
    use http = new HttpClient()
    
    Sources.createFileContext settings
    
type RunMode =
    | WebHost
    | REPL

[<EntryPoint>]
let main argv =

    let config = Configuration.loadDefaultConfig

    let path = "/home/max/Data/test.db"

    let context =
        createContext path config.GeneralReference

    let sourceContext = createSourceContext
    
    context.Connection.Open()

    
    
    printfn "%A" (Store.populateCollectionFromSource context context.GeneralReference sourceContext)
    
    let content =
        [ { Reference = Guid.NewGuid()
            Data = System.IO.File.ReadAllBytes "/home/max/Projects/SemiFunctionalServer/ExampleWebSite/index.html"
            Type = BlobTypes.html
            HashType = Hashing.sha512 }
          { Reference = Guid.NewGuid()
            Data = System.IO.File.ReadAllBytes "/home/max/Projects/SemiFunctionalServer/ExampleWebSite/css/style.css"
            Type = BlobTypes.css
            HashType = Hashing.sha512 }
          { Reference = Guid.NewGuid()
            Data = System.IO.File.ReadAllBytes "/home/max/Projects/SemiFunctionalServer/ExampleWebSite/js/index.js"
            Type = BlobTypes.javascript
            HashType = Hashing.sha512 } ]


    
    

    //let _ =
    //    content
    //    |> List.map (fun c -> add context.Connection c)


    let rm = WebHost

    printfn "Starting FBlob Host"

    let logger = Logger()

    match rm with
    | WebHost ->


        let instance = createInstance context

        let runType = Server.createRunType

        run runType instance
    | REPL -> ()

    printfn "Shutting down"

    0 // return an integer exit code
