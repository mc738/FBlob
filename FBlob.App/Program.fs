// Learn more about F# at http://fsharp.org

open FBlob.Core.Store
open FBlob.Hosting.Engine
open FBlob.Hosting.WebHost
open Peeps

type RunMode =
    | WebHost
    | REPL

[<EntryPoint>]
let main argv =
    
    let rm = WebHost

    printfn "Starting FBlob Host"

    let logger = Logger()
         
    match rm with
    | WebHost ->
        
        let config = Configuration.loadDefaultConfig

        let path = "/home/max/Data/test.db"

        let context =
            createContext path config.GeneralReference

        context.Connection.Open()
    
        let instance = createInstance context
    
        let runType = Server.createRunType
    
        run runType instance
    | REPL -> ()
    
    printfn "Shutting down"
    
    0 // return an integer exit code