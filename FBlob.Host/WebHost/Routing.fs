module FBlob.Host.WebHost.Routing

open System
open System.IO
open Http.ContentTypes

type Route =
    { Paths: string seq
      ContentType: ContentType
      Handler: (Http.Request -> byte array) }

and StaticRouteType = { Name: string }

and FileRouteType = { Path: string; ContentType: ContentType; }

and BlobRouteType = { Reference: Guid }

and ActionRouteType = { Action: string; ContentType: ContentType }

and RouteType =
    | Static of StaticRouteType
    | File of FileRouteType
    | Blob of BlobRouteType
    | Action of ActionRouteType

and RouteSetting =
    { RoutePaths: string seq
      RouteType: RouteType }

and ErrorRoutes =
    { BadRequest: Route
      NotFound: Route
      Unauthorized: Route
      InternalError: Route }

let createStaticRoute settings srtSettings = ()
    
    // TODO fetch content type of static item in store.
    
    //
    //Ok {
    //    Paths = settings.RoutePaths
    //    ContentType = 
    //}


/// Create a static route.
/// Because the content is static it is
/// loaded into memory when the route is created.
let createFileRoute settings frtSettings  =

    match File.Exists(frtSettings.Path) with
    | true ->
        let data = File.ReadAllBytes(frtSettings.Path)
        Ok
            { Paths = settings.RoutePaths
              ContentType = frtSettings.ContentType
              Handler = fun _ -> data }
    | false -> Error (sprintf "Could not load static content: '%s'." frtSettings.Path)

let tryCreateRoute settings =
    match settings.RouteType with
    | Static srt -> Error "Not implemented"
    | File frt -> createFileRoute settings frt
    | Blob brt -> Error "Not implemented"
    | Action art -> Error "Not implemented"

let createRoutes<'a, 'b> results =
    results
    |> Seq.map tryCreateRoute
    |> FUtil.Results.splitResults

let createRouteMap (state: Map<string, Route>) (route: Route) =
    let newRoutes =
        route.Paths
        |> Seq.map (fun r -> (r, route))
        |> Map.ofSeq

    FUtil.Maps.join state newRoutes

let createRoutesMap (routes: RouteSetting seq) =
    let (successful, errors) = createRoutes routes

    // TODO log any errors.
    successful |> Seq.fold createRouteMap Map.empty

let matchRoute (routes: Map<string, Route>) (notFound: Route) (route: string) =
    match routes.TryFind route with
    | Some route -> route
    | None -> notFound