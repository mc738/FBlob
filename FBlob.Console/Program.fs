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
    // createStore path |> ignore
    
    // let context = initializeStore config path
    
    // let blobs = CollectionStore.getGeneral context
    
    // printfn "%A" blobs
    
    
    let ref = BlobStore.addGeneralBlob context BlobTypes.json Hashing.sha512 (Encoding.UTF8.GetBytes """{"message": "Hello, World!"}""")
    
    match ref with
    | Ok r ->
        match BlobStore.getBlob context r with
        | Some b -> 
            let blob = Encoding.UTF8.GetString b.Data 
            printfn "Blob %s: %s\n%A" (r.ToString()) blob b 
        | None -> printfn "No blob found with reference `%s`" (ref.ToString())
    | Error e -> printfn "Error: %s" e
    
    let blobs = CollectionStore.getGeneral context
    
    printfn "%A" blobs
    
    
    printfn "Hello World from F#!"
    0 // return an integer exit code