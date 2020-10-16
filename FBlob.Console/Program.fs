// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Text
open FBlob.Core
open FBlob.Core.Models

[<EntryPoint>]
let main argv =
    
    let config = Configuration.loadDefaultConfig
    
    let path = "/home/max/Data/test.db"
    
    let context = DAL.createContext path config.GeneralReference
    
    context.Connection.Open()
    // DAL.createStore path |> ignore
    
    // let context = DAL.initializeStore config path

    // DAL.addGeneralBlob context "test_blob" (Encoding.UTF8.GetBytes """{"message": "Hello, World!"}""") |> ignore
    
    let refString = "FC85F600-7AC6-4275-AE6C-00CD4AE859EF"
    
    let ref = Guid.Parse refString
    
    match DAL.Blobs.get context ref with
    | Some (bRef, cRef, data) -> 
        let blob = Encoding.UTF8.GetString data 
        printfn "Blob %s: %s" refString blob
    | None -> printfn "No blob found with reference `%s`" refString
        
        
    printfn "Hello World from F#!"
    0 // return an integer exit code