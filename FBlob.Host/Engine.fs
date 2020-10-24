module FBlob.Host.Engine

open System
open FBlob.Core.DAL.Blobs

//type Action =
//    | AddBlob of NewBlob
//    | GetBlob of Guid

type Instance =
    { Processor: MailboxProcessor<Action> }

/// The host run time, either internal (where an internal recursive loop is used) or delegated to a provided function.
and RunType =
    /// Run the host using an internally provided recursive loop that will check for actions and process them.
    | Internal of (Instance -> Async<Response>)
    /// Delegate the running of the host to provided function.
    /// Useful for situations when a program loop might be handled else where (such as the web host).
    | Delegated of (Instance -> Async<unit>)

and Request = { From: string }

and Response = { Successful: bool }

and Action =
    { Request: Request
      ReplyChannel: AsyncReplyChannel<Response> }

let createInstance =

    let mbp =
        MailboxProcessor<Action>
            .Start(fun inbox ->

                  let rec loop () =
                      async {

                          let! action = inbox.Receive()

                          action.ReplyChannel.Reply { Successful = true }

                          return! loop ()
                      }


                  loop ())

    { Processor = mbp }

let postRequest instance request =
    instance.Processor.PostAndAsyncReply(fun r -> { Request = request; ReplyChannel = r })


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
    
let run runType instance =
    match runType with
    | Internal h -> internalRun h instance
    | Delegated h -> delegatedRun h instance 