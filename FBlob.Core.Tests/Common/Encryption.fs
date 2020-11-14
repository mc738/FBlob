module FBlob.Core.Tests.Common.Encryption

open System
open System.Text
open Microsoft.VisualStudio.TestTools.UnitTesting
open FBlob.Core.Common.Encryption

[<TestClass>]
type DecryptionTests () =
    
    [<TestMethod>]
    member this.``Attempt to decrypt data with no iv`` () =
        
        let data = Array.create 15 0uy
        
        let keys = [
            "test",
            [|0uy|]
        ]
        
        let mappedKeys = keys |> Map.ofList
        
        let actual = decrypt mappedKeys "test" data
        
        match actual with
        | Ok _ -> Assert.Fail()
        | Error e ->
            Assert.AreEqual("Data length less than 16, no attached iv.", e)
        
