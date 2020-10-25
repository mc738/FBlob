module FBlob.Hosting.WebHost.Routing

open System
open System.IO
open System.Text.Json.Serialization
open FBlob.Hosting.WebHost.Http
open FBlob.Hosting.WebHost.Http.ContentTypes
open FBlob.Hosting.WebHost.DAL
open Microsoft.Data.Sqlite
open Microsoft.Data.Sqlite

type Route =
    { Paths: string seq
      ContentType: ContentType
      Handler: (HttpRequest -> byte array) }

and [<CLIMutable>] StaticRouteType =
    { [<JsonPropertyName("reference")>]
      Reference: Guid }

and [<CLIMutable>] FileRouteType =
    { [<JsonPropertyName("path")>]
      Path: string

      [<JsonPropertyName("contentType")>]
      ContentType: ContentType }

and [<CLIMutable>] BlobRouteType =
    { [<JsonPropertyName("reference")>]
      Reference: Guid }

and [<CLIMutable>] ActionRouteType =
    { [<JsonPropertyName("action")>]
      Action: string

      [<JsonPropertyName("contentType")>]
      ContentType: ContentType }

and RouteType =
    | Static of StaticRouteType
    | File of FileRouteType
    | Blob of BlobRouteType
    | Action of ActionRouteType

and [<CLIMutable>] RouteSetting =
    { [<JsonPropertyName("routePaths")>]
      RoutePaths: string seq
      
      [<JsonPropertyName("routeType")>]
      RouteType: RouteType }

and ErrorRoutes =
    { BadRequest: Route
      NotFound: Route
      Unauthorized: Route
      InternalError: Route }

/// Create a static route.
/// This will use content stored in the `__wh_content` table in the store.
let private createStaticRoute settings (srtSettings: StaticRouteType) (connection: SqliteConnection) =

    // TODO fetch content type of static item in store.
    let r =
        Content.get connection srtSettings.Reference

    match r with
    | Some c ->
        Ok
            { Paths = settings.RoutePaths
              ContentType = ContentType.Literal c.ContentTypeLiteral
              Handler = fun _ -> c.Content } // TODO This should call the db each time.
    | None -> Error "" // TODO add error message.

/// Create a file route.
/// Because the content is static it is
/// loaded into memory when the route is created.
let private createFileRoute settings frtSettings =

    match File.Exists(frtSettings.Path) with
    | true ->
        let data = File.ReadAllBytes(frtSettings.Path)
        Ok
            { Paths = settings.RoutePaths
              ContentType = frtSettings.ContentType
              Handler = fun _ -> data }
    | false -> Error(sprintf "Could not load static content: '%s'." frtSettings.Path)

let private tryCreateRoute connection settings =
    match settings.RouteType with
    | Static srt -> createStaticRoute settings srt connection
    | File frt -> createFileRoute settings frt
    | Blob brt -> Error "Not implemented"
    | Action art -> Error "Not implemented"

let private createRoutes<'a, 'b> (connection: SqliteConnection) results =

    let handler = tryCreateRoute connection

    results
    |> Seq.map handler
    |> FUtil.Results.splitResults

let private createRouteMap (state: Map<string, Route>) (route: Route) =
    let newRoutes =
        route.Paths
        |> Seq.map (fun r -> (r, route))
        |> Map.ofSeq

    FUtil.Maps.join state newRoutes

let createRoutesMap (routes: RouteSetting seq) (connection: SqliteConnection) =
    let (successful, errors) = createRoutes connection routes

    // TODO log any errors.
    successful |> Seq.fold createRouteMap Map.empty

let matchRoute (routes: Map<string, Route>) (notFound: Route) (route: string) =
    match routes.TryFind route with
    | Some route -> route
    | None -> notFound


// let serializeRouteSettings settings = 


let loadRoute context reference =
    ()
    
let saveRoute context route =
    ()