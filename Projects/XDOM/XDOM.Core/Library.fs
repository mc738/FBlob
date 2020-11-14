namespace XDOM.Core

open System
open System.Text.Json.Serialization


module Models =
    
    [<CLIMutable>]
    type Quote = {
        [<JsonPropertyName("reference")>]
        Reference: Guid
        
        [<JsonPropertyName("author")>]
        Author: string
        
        [<JsonPropertyName("text")>]
        Text: string
        
        [<JsonPropertyName("categories")>]
        Categories: string seq
    }


module Say =
    let hello name =
        printfn "Hello %s" name
