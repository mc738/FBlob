namespace FBlob.Core.Parsing

open System
open System.Text.Json



module Json =

    let tryParse<'a> (json: string) =
        
        try
            let r = JsonSerializer.Deserialize<'a>(json)
            Ok r
        with
        | :? ArgumentNullException -> Error "Arguments can not be null"
        | :? JsonException -> Error "Input is not json"
        | :? NotSupportedException -> Error "There is no compatible JsonConverter for returnType or its serializable members."
            
