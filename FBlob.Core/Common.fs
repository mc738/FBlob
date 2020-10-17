namespace FBlob.Core

open System
open System.Data.SqlTypes
open System.IO
open System.Text.Json
open System.Text.Json.Serialization

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


    /// TODO Use this for config.
    let supportedTypes = [ json; text; binary ]

module HashTypes =

    open Models
    
    let (md5: HashType) = { Name = "MD5" }
    
    let (sha1: HashType) = { Name = "SHA1" }

    let (sha2: HashType) = { Name = "SHA2" }  

    let (sha256: HashType) = { Name = "SHA256" }  
                                                                              
    let (sha512: HashType) = { Name = "SHA512" }

    let sha1Hasher data = FUtil.Hashing.sha1 data

    let hashData (hashType: HashType) data =
        match hashType.Name with
        | "MD5" -> Ok(sha1Hasher data) // TODO fix this - upstream change to FUlit.
        | "SHA1" -> Ok(sha1Hasher data)
        | "SHA2" -> Ok(sha1Hasher data) // TODO fix this - upstream change to FUlit.
        | "SHA256" -> Ok(sha1Hasher data) // TODO fix this - upstream change to FUlit.
        | "SHA512" -> Ok(sha1Hasher data) // TODO fix this - upstream change to FUlit.
        | _ -> Error(sprintf "Algorithm `%s` not supported" hashType.Name)
        
 
 
 
 
 

    let toHex data =
        data                                                                
        |> Array.map (fun (x:byte) -> System.String.Format("{0:X2}", x))
        |> String.concat String.Empty
    
    let hashToHex (hashType: HashType) data =                                   
        match hashData hashType data with           
        | Ok h ->                                         
            Ok(toHex h)                                        
        | Error e -> Error(e)                                                            