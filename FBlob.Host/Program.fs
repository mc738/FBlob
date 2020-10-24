// Learn more about F# at http://fsharp.org

open System
open FBlob.Host.Engine
open FBlob.Host.WebHost
    
[<EntryPoint>]
let main argv =
    
    printfn "Starting FBlob Host"
    
    let instance = createInstance
    
    let runType = Server.createRunType
    
    run runType instance
    
    printfn "Shutting down"
    
    0 // return an integer exit code