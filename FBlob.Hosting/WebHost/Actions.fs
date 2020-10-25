module FBlob.Hosting.WebHost.Actions

open System.IO
open FBlob.Hosting.Actions

let private importContent =
    { Name = "import"
      Parameters = [ "path"; "route" ]
      Handler =
          (fun args ->

              let path = args.TryFind "path"
              let route = args.TryFind "route"

              match (path, route) with
              | Some p, Some r -> Ok "Yay it worked!"
              | _ -> Error "Invalid arguments"
          ) }

let private exportContent =
    { Name = "import"
      Parameters = [ "reference" ]
      Handler =
          (fun args ->

              let reference = args.TryFind "reference"

              match reference with
              | Some r -> Ok "Yay it worked!"
              | _ -> Error "Invalid arguments"

          ) }

let export =
    { Name = "wh"
      Actions = [ importContent ] }
