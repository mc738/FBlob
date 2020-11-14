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

// TODO move this into `Sources`
let splitter (json:byte array) =
    
    let buffer = System.Buffers.ReadOnlySequence<byte> json
    
    let doc = JsonDocument.Parse(buffer).RootElement.EnumerateArray()

    [
        for i in doc do
            FUtil.Serialization.Utilities.stringToBytes (i.ToString())        
    ]    

// TODO move this into `Store`
let loadFromSource =
    
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
    
    let context = Sources.createFileContext settings
    
    getSource context
    
type RunMode =
    | WebHost
    | REPL

[<EntryPoint>]
let main argv =

    printfn "%A" loadFromSource
    
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
