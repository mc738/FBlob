namespace Inspired.Core


module Initialization =

    open FBlob.Core.Common.Sources

    let source path =
        FileSettings
            { Path = path
              Name = "quotes_1_99999"
              Get = true
              Set = false
              Collection = true
              ContentType = "application/json" }



module Say =
    let hello name = printfn "Hello %s" name
