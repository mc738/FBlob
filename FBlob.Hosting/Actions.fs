module FBlob.Hosting.Actions

open System
open System.Reflection.Metadata

type Action =
    { Name: string
      Parameters: string list
      Handler: Map<string,Arg> -> Result<string,string> }

and ActionCollection = { Name: string; Actions: Action list }

and Arg = { Name: string; Value: string }

let private matchParameter (args: Map<string, Arg>) (parameter: string) =
    match args.TryFind parameter with
    | Some a -> Ok a
    | None -> Error (sprintf "Missing argument `%s`" parameter)
        
/// Handle an action by validating the provided arguments and executing the action.    
let handleAction (action: Action) (args: Map<string, Arg>) =
    
    // Just looking for errors. If none the args will be passed to the handler.
    let (_, errors) =
        action.Parameters
        |> List.map (fun p -> matchParameter args p)
        |> FUtil.Results.splitResults 
    
    // If there are no errors pass the args to the handler, if there are return a consolidated message.
    match Seq.length errors with
    | 0 -> action.Handler args
    | _ -> Error (errors |> Seq.fold (fun msg e -> sprintf"%s, %s" msg e) "The following errors occured: ")