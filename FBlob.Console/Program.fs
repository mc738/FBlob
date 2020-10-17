// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Text
open FBlob.Core
open FBlob.Core.Models
open FBlob.Core.Store

[<EntryPoint>]
let main argv =
    
    let config = Configuration.loadDefaultConfig
    
    let path = "/home/max/Data/test.db"
    
    let context = createContext path config.GeneralReference
    
    context.Connection.Open()
    createStore path |> ignore
    
    let context = initializeStore config path

    let blobType =
        { Name = "Json"
          ContentType = "application/json"
          Extension = "json" }

    let hashType: HashType = { Name = "SHA1" }
    
    let ref = Blobs.addGeneralBlob context BlobTypes.json hashType (Encoding.UTF8.GetBytes """{"message": "Hello, World!"}""")
    
    match ref with
    | Ok r ->
        match Blobs.getBlob context r with
        | Some b -> 
            let blob = Encoding.UTF8.GetString b.Data 
            printfn "Blob %s: %s" (ref.ToString()) blob
        | None -> printfn "No blob found with reference `%s`" (ref.ToString())
    | Error e -> printfn "Error: %s" e
    
    printfn "Hello World from F#!"
    0 // return an integer exit code