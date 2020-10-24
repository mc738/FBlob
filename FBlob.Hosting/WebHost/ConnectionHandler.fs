module FBlob.Hosting.WebHost.ConnectionHandler

open System
open System.Net.Sockets
open System.Text
open FUtil
open Peeps
open FBlob.Hosting.Engine
open FBlob.Hosting.WebHost.Routing
open FBlob.Hosting.WebHost.Http
open FBlob.Hosting.WebHost.Http.ContentTypes


type Context =
    { Routes: Map<string, Route>
      ErrorRoutes: ErrorRoutes
      Instance: Instance
      Logger: Logger }

let private standardHeaders =
    seq {
        "Server", "SFS"
        "Connection", "close"
    }


let private createResponseHeaders (contentType: ContentType) contentLength (otherHeaders: (string * string) seq) =

    let cDetails =
        seq {
            ("Content-Type", (getCTString contentType))
            ("Content-Length", contentLength.ToString())
        }

    standardHeaders
    |> Seq.append cDetails
    |> Seq.append otherHeaders
    |> Map.ofSeq

let private createResponse (context: Context) (route: Route) (request: HttpRequest option) =
    
    // TODO Update to call the store.
    // let _ = postRequest context.Instance.Processor
    let contentType = route.ContentType
    
    let (content, contentLength) =    
        match request with
        | Some r ->
            let c = route.Handler r
            (Some (Binary c), c.Length)          
        | None -> (None, 0)
    
    // Any other headers associated with this response,
    // i.e. none standard and now generated.
    let otherHeaders = Seq.empty
    
    // Make the headers.
    let headers = createResponseHeaders contentType contentLength otherHeaders

    // Create the response.
    createResponse 200s headers content

/// Handle a request and return a response.
/// This function is designed to be testable against, with out network infrastructure.
let handlerRequest context request =
    let (route, r) =
            match request with
            | Ok r -> (matchRoute context.Routes context.ErrorRoutes.NotFound r.Route, Some r)
                // TODO match on route type.
                
                // TODO translate HttpRequest into Request.
                
                // TODO call processor, send the request and create the http response.
               
                // TODO translate response into HttpResponse
               
            | Result.Error e ->
                // TODO Log error.
                (context.ErrorRoutes.BadRequest, None)

    // Create the response and serialize it.
    createResponse context route r
    
/// Accepts a context and a connection and handles it.
/// This is meant to be run on a background thread.
let handleConnection (context: Context) (connection: TcpClient) =
    async {

        // For now accept a message, convert to string and send back a message.
        context.Logger.Post
            { from = "Connection Handler"
              message = "In handler."
              time = DateTime.Now
              ``type`` = Debug }

        // Get the network stream
        let stream = connection.GetStream()

        // Rec loop to make sure we don't read into buffer before data is available.
        // Github issue: `Issue with request buffer #4`
        let rec waitForData() =
            if stream.DataAvailable then
                ()
            else
                Async.Sleep 100 |> ignore
                waitForData()
        
        waitForData()
        
        // Read the incoming request into the buffer.
        let! buffer = Streams.readToBuffer stream 1056 // |> Async.RunSynchronously

        let request = deserializeRequest buffer

        // Handle the request, and serialize the response.
        let response = handlerRequest context request |> serializeResponse
        
        let test = Encoding.UTF8.GetString(response)
        
        // Send the response.
        stream.Write(response, 0, response.Length)

        // Create the headers.
        connection.Close()

        return ()
    }