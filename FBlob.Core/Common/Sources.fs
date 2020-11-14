module FBlob.Core.Common.Sources

open System.IO
open System.Net.Http
open System.Text.Json
open System.Text.Json.Serialization
open FUtil.HttpClient


type SourceContentType =
    | Json
    | Text
    | Binary

[<CLIMutable>]
type UrlSourceSettings =
    { [<JsonPropertyName("url")>]
      Url: string

      [<JsonPropertyName("name")>]
      Name: string

      [<JsonPropertyName("get")>]
      Get: bool

      [<JsonPropertyName("set")>]
      Set: bool

      [<JsonPropertyName("collection")>]
      Collection: bool

      [<JsonPropertyName("contentType")>]
      ContentType: string }

[<CLIMutable>]
type FileSourceSettings =
    { [<JsonPropertyName("path")>]
      Path: string

      [<JsonPropertyName("name")>]
      Name: string

      [<JsonPropertyName("get")>]
      Get: bool

      [<JsonPropertyName("set")>]
      Set: bool

      [<JsonPropertyName("collection")>]
      Collection: bool

      [<JsonPropertyName("contentType")>]
      ContentType: string }

type SourceSettings =
    | UrlSettings of UrlSourceSettings
    | FileSettings of FileSourceSettings

type SourceContext =
    | UrlSource of UrlSourceContext
    | FileSource of FileSourceContext
    
and SourceResult =
    | Single of byte array
    | Collection of byte array list

and UrlSourceContext = { Url: string; Client: HttpClient; Collection: bool }

and FileSourceContext = { Path: string; Collection: bool }

let private getUrlSource (client: HttpClient) (url: string) =
    async { return! (tryGet ReturnType.String client url) }

let private getFileSource path = FUtil.Files.tryReadBytes path

let private splitCollection (data: byte array) =
    let buffer = System.Buffers.ReadOnlySequence<byte> data

    let doc = JsonDocument.Parse(buffer).RootElement.EnumerateArray()

    [
        for i in doc do
            FUtil.Serialization.Utilities.stringToBytes (i.ToString())        
    ]    

let private fileSourceHandler fileCtx =
    let r = getFileSource fileCtx.Path 
    match r with
    | Ok d ->
        match fileCtx.Collection with
        | true -> Ok (Collection (splitCollection d))
        | false -> Ok (Single d)
    | Error e -> Error e
    

let private urlSourceHandler urlCtx =
    // TODO Fix this up (potentially)
    let response =
        getUrlSource urlCtx.Client urlCtx.Url
        |> Async.RunSynchronously

    match response with
    | Ok r ->
        match r with
        | StringContent s ->
            // TODO return bytes to cut out conversion and conversion back.
            let d = FUtil.Serialization.Utilities.stringToBytes s
            
            match urlCtx.Collection with
            | true -> Ok (Collection (splitCollection d))
            | false -> Ok (Single d)
        | StreamContent s ->
            // TODO clean up or add toArray stream to `FUtil`.
            use ms = new MemoryStream()
            s.CopyTo(ms)
            
            let d = ms.ToArray()
            
            match urlCtx.Collection with
            | true -> Ok (Collection (splitCollection d))
            | false -> Ok (Single d)
    | Error e -> Error e

let createFileContext (settings: FileSourceSettings) = FileSource { Path = settings.Path; Collection = settings.Collection }

let createUrlContext (client: HttpClient) (settings: UrlSourceSettings) =
    UrlSource { Url = settings.Url; Client = client; Collection = settings.Collection }

let private createContext (client: HttpClient) (settings: SourceSettings) =
    match settings with
    | UrlSettings s -> createUrlContext client s
    | FileSettings s -> createFileContext s

let createContexts client (settings: SourceSettings list) =
    // use client = new HttpClient()

    let handler = createContext client

    settings |> List.map handler

let getSource (context: SourceContext) =
    match context with
    | UrlSource urlCtx -> urlSourceHandler urlCtx
    | FileSource fileCtx -> fileSourceHandler fileCtx
