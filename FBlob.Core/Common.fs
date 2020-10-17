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