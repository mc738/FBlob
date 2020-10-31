namespace FBlob.Core

open System
open System.Data.SqlTypes
open System.IO
open System.Net.Http
open System.Text.Json
open System.Text.Json.Serialization
open FUtil
open FUtil.HttpClient

module Models =

    type BlobProperty = { Key: string; Value: string }

    and BlobType =
        { Name: string
          ContentType: string
          Extension: string }

    and Blob =
        { Reference: Guid
          CollectionRef: Guid
          Data: byte array
          Hash: string
          Salt: string
          CreatedOn: DateTime
          MetaDataBlob: JsonBlob
          KeyRef: string
          Type: BlobType
          Properties: BlobProperty list
          Encrypted: bool
          Path: string
          HashType: HashType
          EncryptionType: EncryptionType }

    and CollectionProperty = { Key: string; Value: string }

    and Collection =
        { Reference: Guid
          Name: string
          CreatedOn: DateTime
          MetaData: JsonBlob
          AnonymousRead: bool
          AnonymousWrite: bool
          Blobs: Blob list
          Properties: CollectionProperty list
          Sources: Source list
          UserPermissions: Map<Guid, UserCollectionPermissions> }

    and EncryptionType = { Name: string }

    and Extension = { Name: string; Settings: JsonBlob }

    and HashType = { Name: string }

    and SourceType = { Name: string }

    and Source =
        { Name: string
          Type: Source
          Path: string
          CollectionRef: Guid
          Get: bool
          Set: bool
          Settings: JsonBlob }

    and UserCollectionPermissions =
        { UserReference: Guid
          CollectionReference: Guid
          CanRead: bool
          CanWrite: bool }

    and User =
        { Reference: Guid
          Username: string
          Password: string
          Salt: string
          CreateOn: DateTime }

    and JsonBlob = string

/// Common helpers for blob types.
module BlobTypes =

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

module Hashing =

    open Models

    let (md5: HashType) = { Name = "MD5" }

    let (sha1: HashType) = { Name = "SHA1" }

    let (sha256: HashType) = { Name = "SHA256" }

    let (sha384: HashType) = { Name = "SHA384" }

    let (sha512: HashType) = { Name = "SHA512" }

    let hashData (hashType: HashType) data =
        match hashType.Name with
        | "MD5" -> Ok(FUtil.Hashing.md5Hex data)
        | "SHA1" -> Ok(FUtil.Hashing.sha1Hex data)
        | "SHA256" -> Ok(FUtil.Hashing.sha256Hex data)
        | "SHA384" -> Ok(FUtil.Hashing.sha384Hex data)
        | "SHA512" -> Ok(FUtil.Hashing.sha512Hex data)
        | _ -> Error(sprintf "Algorithm `%s` not supported" hashType.Name)


module Sources =

    type SourceContentType =
        | Json
        | Text
        | Binary
        
    [<CLIMutable>]
    type UrlSourceSettings =
        { [<JsonPropertyName("type")>]
          Type: string

          [<JsonPropertyName("url")>]
          Url: string

          [<JsonPropertyName("get")>]
          Get: bool

          [<JsonPropertyName("set")>]
          Set: bool

          [<JsonPropertyName("collection")>]
          Collection: bool

          [<JsonPropertyName("contentType")>]
          ContentType: SourceContentType }

    type SourceContext =
        | UrlSource of UrlSourceContext
        | FileSource of FileSourceContext
        
    and UrlSourceContext = {
        Url: string
        Client: HttpClient
    }
    
    and FileSourceContext = {
        Path: string
    }
    
    let private getUrlSource (client: HttpClient) (url: string) =
        async { return! (tryGet ReturnType.String client url) }
        
    let private getFileSource path = FUtil.Files.tryReadBytes path

    let private fileSourceHandler fileCtx = getFileSource fileCtx.Path
    
    let private urlSourceHandler urlCtx =
        // TODO Fix this up (potentially)
        let response = getUrlSource urlCtx.Client urlCtx.Url |> Async.RunSynchronously

        match response with
        | Ok r ->
            match r with
            | StringContent s ->
                // TODO return bytes to cut out conversion and conversion back.
                Ok (FUtil.Serialization.Utilities.stringToBytes s)
            | StreamContent s ->
                // TODO clean up or add toArray stream to `FUtil`.
                use ms = new MemoryStream()
                s.CopyTo(ms)
                Ok (ms.ToArray())
            
    let getSource (context:SourceContext) =
        match context with
        | UrlSource urlCtx -> urlSourceHandler urlCtx
        | FileSource fileCtx -> fileSourceHandler fileCtx
         
module Encryption =

    open FUtil.Security

    let private encryptor context data = encryptBytesAes context data

    let private decryptor context cipher = decryptBytesAes context cipher

    /// Encrypt data and append the IV to the front.
    /// The IV will be 16 bytes.
    let encrypt (keys: Map<string, byte array>) keyRef data =
        match keys.TryFind keyRef with
        | Some k ->
            let context = { Key = k; IV = generateSalt 16 }

            let encrypted = encryptor context data

            let r = Array.append context.IV encrypted

            Ok(r)

        | None -> Error(sprintf "Key `%s` not found." keyRef)

    let decrypt (keys: Map<string, byte array>) keyRef data =

        match keys.TryFind keyRef with
        | Some k ->
            // TODO Add check that array is larger than 16.
            let (iv, cipher) = Array.splitAt 16 data

            let context = { Key = k; IV = iv }

            Ok(decryptor context cipher)


        | None -> Error(sprintf "Key `%s` not found." keyRef)
