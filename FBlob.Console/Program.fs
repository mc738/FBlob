// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Net.Http
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open FBlob.Core
open FBlob.Core.Models
open FBlob.Core.Store

[<CLIMutable>]
type Post = {
    [<JsonPropertyName("userId")>]
    UserId: int
    
    [<JsonPropertyName("id")>]
    Id: int
    
    [<JsonPropertyName("title")>]
    Title: string
    
    [<JsonPropertyName("body")>]
    Body: string
}

let testUrl =
    "https://jsonplaceholder.typicode.com/posts"

[<EntryPoint>]
let main argv =

    
    use http = new HttpClient()
    
    let r = Sources.getUrlSource http testUrl |> Async.RunSynchronously

    match r with
    | Ok s ->
        let posts = Parsing.Json.tryParse<Post seq> s
        
        printfn "Success: %A" posts
    | Error e -> printfn "Error: %s" e
    
    
    let data =
        (Encoding.UTF8.GetBytes """{"message": "Hello, World!"}""")

    let keys =
        Map.ofList [ "test", Encoding.UTF8.GetBytes "Super secret key" ]

    let eR = Encryption.encrypt keys "test" data

    match eR with
    | Ok e ->
        let dR = Encryption.decrypt keys "test" e
        
        match dR with
        | Ok d ->
            printfn "Data: %s\nEncrypted: %A\nDecrypted: %A\nResult: %s" (Encoding.UTF8.GetString data) e d (Encoding.UTF8.GetString d)
        | Error _ -> ()
    | Error _ -> ()
    
    let config = Configuration.loadDefaultConfig

    let path = "/home/max/Data/test.db"

    let context =
        createContext path config.GeneralReference

    context.Connection.Open()
    // createStore path |> ignore

    // let context = initializeStore config path

    // let blobs = CollectionStore.getGeneral context

    // printfn "%A" blobs


    let ref =
        BlobStore.addGeneralBlob
            context
            BlobTypes.json
            Hashing.sha512
            (Encoding.UTF8.GetBytes """{"message": "Hello, World!"}""")

    match ref with
    | Ok r ->
        match BlobStore.getBlob context r with
        | Some b ->
            let blob = Encoding.UTF8.GetString b.Data
            printfn "Blob %s: %s\n%A" (r.ToString()) blob b
        | None -> printfn "No blob found with reference `%s`" (ref.ToString())
    | Error e -> printfn "Error: %s" e

    let blobs = CollectionStore.getGeneral context

    printfn "%A" blobs


    printfn "Hello World from F#!"
    0 // return an integer exit code
