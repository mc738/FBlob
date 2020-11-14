module FBlob.Hosting.WebHost.Actions

open System
open System.IO
open FBlob.Core
open FBlob.Core.Actions
open FBlob.Core.Common
open FBlob.Core.Store
open FBlob.Hosting
open FBlob.Hosting.WebHost.DAL.Content

let private handContentImport (context: Context) route path =
    match File.Exists path with
    | true ->

        let ref = Guid.NewGuid()

        let data = File.ReadAllBytes path

        // Create the new content.
        let newContent: NewContent =
            { Reference = ref
              Data = data
              Type = BlobTypes.json
              HashType = Hashing.sha512 }

        let result = WebHost.DAL.Content.add context.Connection newContent
        // Create the new route.
        match result with
        | Ok _ ->
            let newRoute = ()
            
            // TODO create route...
                    
            Ok(ref.ToString())
        | Error e -> Error e
    | false -> Error(sprintf "File `%s` does not exist" path)

let private importContent =
    { Name = "import"
      Parameters = [ "path"; "route" ]
      Handler =
          (fun context args ->

              let path = args.TryFind "path"
              let route = args.TryFind "route"

              match (path, route) with
              | Some p, Some r -> Ok "Yay it worked!"
              | _ -> Error "Invalid arguments") }

let private exportContent =
    { Name = "export"
      Parameters = [ "reference" ]
      Handler =
          (fun context args ->

              let reference = args.TryFind "reference"

              match reference with
              | Some r -> Ok "Yay it worked!"
              | _ -> Error "Invalid arguments"

          ) }

let export =
    { Name = "wh"
      Actions = [ importContent ] }