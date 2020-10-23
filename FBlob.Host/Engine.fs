module FBlob.Host.Engine

open System
open FBlob.Core.DAL.Blobs

//type Action =
//    | AddBlob of NewBlob
//    | GetBlob of Guid

type Request = { From: string }

and Response = { Successful: bool }

and Action =
    { Request: Request
      ReplyChannel: AsyncReplyChannel<Response> }

type Instance =
    { Processor: MailboxProcessor<Action> }

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
