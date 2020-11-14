// Learn more about F# at http://fsharp.org

open System
open System.Text.Json
open FBlob.Core
open XDOM.Core.Models

let settings =
    Sources.FileSettings {
            Path = ""
            Name = "R2"
            Get = true
            Set = false
            Collection = false
            ContentType = "text/plain"
        }



let splitter (json:string) =
    
    let doc = JsonDocument.Parse(json).RootElement.EnumerateArray()

    [
        for i in doc do
            i.ToString()        
    ]    
    


[<EntryPoint>]
let main argv =
    
    // Using `FBlob`
    
    // 1. Load blobs into a collection (collection type file source).
    
    
    
    // 2. Load a random quote from thr collection for display.
    
    let path = "/home/max/Projects/FBlob/Projects/Inspired/quotes_1_99999.json"

    
        
    let file = FUtil.Files.tryReadText path
    
    match file with
    | Ok data ->
        let r = splitter data
    
        let jsonR = FUtil.Serialization.Json.tryDeserialize<Quote seq> data

        match jsonR with
        | Ok json ->
            let index = Random().Next(0, Seq.length json)
            
            let quote = json |> Seq.item index
            
            printfn "%A" quote
            
        | Error e -> failwith e
    | Error e -> failwith e    
    
    
    
    printfn "Hello World from F#!"
    0 // return an integer exit code
