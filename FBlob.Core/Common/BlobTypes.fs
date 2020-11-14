module FBlob.Core.Common.BlobTypes

open Models

let json =
    { Name = "Json"
      ContentType = "application/json"
      Extension = "json" }

let text =
    { Name = "Text"
      ContentType = "text/json"
      Extension = "txt" }

let binary =
    { Name = "Binary"
      ContentType = "application/oct-stream"
      Extension = "bin" }

let html =
    { Name = "Html"
      ContentType = "text/html"
      Extension = "html" }

let css =
    { Name = "Css"
      ContentType = "text/css"
      Extension = "css" }

let javascript =
    { Name = "Javascript"
      ContentType = "text/javascript"
      Extension = "js" }

let supportedTypes =
    [ json
      text
      binary
      html
      css
      javascript ]
