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

    let blobType = {
        Name = "Json"
        ContentType = "application/json"
        Extension = "json"
    }
    
    let hashType: HashType = { Name = "SHA1" }
    
    let ref = Blobs.addGeneralBlob context blobType hashType (Encoding.UTF8.GetBytes """{"message": "Hello, World!"}""")
    
    match Blobs.getBlob context ref with
    | Some (bRef, cRef, data) -> 
        let blob = Encoding.UTF8.GetString data 
        printfn "Blob %s: %s" (ref.ToString()) blob
    | None -> printfn "No blob found with reference `%s`" (ref.ToString())
         
    printfn "Hello World from F#!"
    0 // return an integer exit code