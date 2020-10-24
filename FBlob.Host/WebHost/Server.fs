module FBlob.Host.WebHost.Server

open System
open System.IO
open System.Net.Sockets
open FBlob.Host.Engine
open Peeps
open FBlob.Host.WebHost.Http
open FBlob.Host.WebHost.ConnectionHandler
open FBlob.Host.WebHost.Routing

let logger = Logger()

// TODO Set these in config.
let private ip = "127.0.0.1"
let private port = 42000

/// The tcp listener.
let private listener = TcpListener.Create(port)

let notFound =

    // TODO add error handling for missing 404 (or stick this in the db in initialization).
    let paths = seq { "NotFound"; "404" }

    let content =
        File.ReadAllBytes("/home/max/Projects/SemiFunctionalServer/ExampleWebSite/404.html")
    
    { Paths = paths
      ContentType = ContentTypes.Html
      Handler = (fun _ -> content) }

let private routeMap = createRoutesMap RoutesConfig.routes

/// The listening loop.
let rec private listen (context:Context) =
    // Await a connection.
    // This is blocking.
    let connection = listener.AcceptTcpClient()

    let handler = handleConnection context
 
    logger.Post
        { from = "Listener"
          message = "Connection received."
          time = DateTime.Now
          ``type`` = Debug }

    // Send to a background thread to handle.
    handler connection |> Async.Start |> ignore

    logger.Post
        { from = "Listener"
          message = "Back to main."
          time = DateTime.Now
          ``type`` = Debug }

    // Repeat the listen loop.
    listen (context)

/// Start the listening loop and accept incoming requests.
let private start instance =

    let context =
        { Logger = logger
          Routes = routeMap
          Instance = instance
          ErrorRoutes =
              { NotFound = notFound
                InternalError = notFound
                Unauthorized = notFound
                BadRequest = notFound } }

    // "Inject" the context.
    // The handler can then be passed to the listen loop.
    let handler = handleConnection context

    logger.Post
        { from = "Main"
          message = "Starting listener."
          time = DateTime.Now
          ``type`` = Information }
    // Start a tcp listener on specified ip and port.
    listener.Start()
    logger.Post
        { from = "Main"
          message = "Listener started."
          time = DateTime.Now
          ``type`` = Success }

    // Pass the listener to `listen` function.
    // This will recursively handle requests.
    listen (context) |> ignore
    
/// Run the web host.
let private run (instance: Instance) = start instance
    
/// Create a delegated `RunType` for the web host.
/// This can be passed to the hosting environment for execution.
let createRunType = Delegated run
    