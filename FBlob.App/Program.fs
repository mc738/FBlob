// Learn more about F# at http://fsharp.org

open System
open FBlob.Core
open FBlob.Core.Store
open FBlob.Hosting.Engine
open FBlob.Hosting.WebHost
open FBlob.Hosting.WebHost.DAL.Content
open FBlob.Hosting.WebHost.Routing
open Peeps

type RunMode =
    | WebHost
    | REPL

[<EntryPoint>]
let main argv =

    let config = Configuration.loadDefaultConfig

    let path = "/home/max/Data/test.db"

    let context =
        createContext path config.GeneralReference

    context.Connection.Open()

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
