// Learn more about F# at http://fsharp.org

open System
open FBlob.Host.Engine


let handler (instance: Instance) = async {
    let! response = postRequest instance { From = "Main" }    
    
    printfn "Response: %A" response
    
    let! _ = Async.Sleep 1000
    
    return response
}
    
/// Run the host with an internal recursive loop, the handler will be called each iteration.
let private internalRun (handler: Instance -> Async<Response>) (instance: Instance) =
    
    let rec loop (handler: Instance -> Async<Response>, instance: Instance) =   
        async {
             
        let! _ = handler instance
        
        return! loop (handler, instance)
    }
    
    // Force the result to be ignored, so it signatures match.
    loop (handler, instance) |> Async.Ignore

/// Run the host, delegating the to provided function.
/// The function will be responsible for the actual execution of the loop. 
let private delegatedRun (handler: Instance -> Async<unit>) instance = handler instance
    
type RunType =
    | Internal of (Instance -> Async<Response>)
    | Delegated of (Instance -> Async<unit>)
    
let run runType instance =
    match runType with
    | Internal h -> internalRun h instance
    | Delegated h -> delegatedRun h instance 

    
    
[<EntryPoint>]
let main argv =
    
    printfn "Starting FBlob Host"
    
    let instance = createInstance
    
    let runType = RunType.Internal handler
    
    run runType instance |> Async.RunSynchronously
    
    printfn "Shutting down"
    
    0 // return an integer exit code