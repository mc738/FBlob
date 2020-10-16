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

    module Config =

        type Table =
            { Name: string
              Sql: string
              Order: int }

        let tables =
            [ { Name = "blob_properties"
                Sql = """
                CREATE TABLE blob_properties (
                    blob_ref TEXT (36) NOT NULL
                                       REFERENCES blobs (reference),
                    [key]    TEXT      NOT NULL,
                    value    TEXT      NOT NULL
                );
                """
                Order = 100 }
              { Name = "blob-types"
                Sql = """
                CREATE TABLE blob_types (
                    name         TEXT PRIMARY KEY,
                    content_type TEXT NOT NULL,
                    extension    TEXT NOT NULL
                );
                """
                Order = 2 }
              { Name = "blobs"
                Sql = """
                CREATE TABLE blobs (
                    reference       VARCHAR (36) PRIMARY KEY
                                                 UNIQUE
                                                 NOT NULL,
                    collection_ref  TEXT (36)    REFERENCES collections (reference)
                                                 NOT NULL,
                    data            BLOB         NOT NULL,
                    hash            TEXT         NOT NULL,
                    salt            TEXT         NOT NULL
                                                 UNIQUE,
                    created_on      DATETIME     NOT NULL,
                    metadata_blob   BLOB         NOT NULL
                                                 DEFAULT "",
                    key_ref         TEXT         NOT NULL,
                    type            TEXT         REFERENCES blob_types (name)
                                                 NOT NULL,
                    path            VARCHAR      NOT NULL,
                    encrypted       BOOLEAN      DEFAULT (0)
                                                 NOT NULL,
                    hash_type       TEXT         REFERENCES hash_types (name)
                                                 NOT NULL,
                    encryption_type TEXT         REFERENCES encryption_types (name)
                );
                """
                Order = 50 }
              { Name = "collection_properties"
                Sql = """
                CREATE TABLE collection_properties (
                    collection_ref TEXT (36) REFERENCES collections (reference)
                                             NOT NULL,
                    [key]          TEXT      NOT NULL,
                    value          TEXT      NOT NULL
                );
                """
                Order = 101 }
              { Name = "collections"
                Sql = """
                CREATE TABLE collections (
                    reference       TEXT (36) PRIMARY KEY
                                              NOT NULL
                                              UNIQUE,
                    name            TEXT      NOT NULL
                                              UNIQUE,
                    created_on      DATETIME  NOT NULL,
                    metadata        BLOB      NOT NULL
                                              DEFAULT "",
                    anonymous_read  BOOLEAN   NOT NULL
                                              DEFAULT (1),
                    anonymous_write BOOLEAN   DEFAULT (0)
                                              NOT NULL
                );
                """
                Order = 10 }
              { Name = "encryption_types"
                Sql = """
                CREATE TABLE encryption_types (
                    name     TEXT PRIMARY KEY
                                  NOT NULL,
                    settings BLOB NOT NULL
                );
                """
                Order = 4 }
              { Name = "extensions"
                Sql = """
                CREATE TABLE extensions (
                    name TEXT PRIMARY KEY
                              NOT NULL,
                    data BLOB NOT NULL
                );
                """
                Order = 5 }
              { Name = "hash_types"
                Sql = """
                CREATE TABLE hash_types (
                    name TEXT NOT NULL
                              PRIMARY KEY
                );
                """
                Order = 6 }
              { Name = "source_types"
                Sql = """
                CREATE TABLE source_types (
                    name TEXT PRIMARY KEY
                            NOT NULL
                );
                """
                Order = 7 }
              { Name = "sources"
                Sql = """
                CREATE TABLE sources (
                    name           TEXT    PRIMARY KEY
                                           NOT NULL,
                    type           TEXT    REFERENCES source_type (name)
                                           NOT NULL,
                    path           TEXT    NOT NULL,
                    collection_ref TEXT    REFERENCES collections (reference)
                                           NOT NULL,
                    get            BOOLEAN DEFAULT (1)
                                           NOT NULL,
                    [set]          BOOLEAN NOT NULL
                                           DEFAULT (0),
                    settings       BLOB    NOT NULL
                );
                """
                Order = 8 }
              { Name = "user_collection_permissions"
                Sql = """
                CREATE TABLE user_collection_permissions (
                    user_ref       TEXT    REFERENCES users (reference)
                                           NOT NULL,
                    collection_ref TEXT    REFERENCES collections (reference)
                                           NOT NULL,
                    can_read       BOOLEAN DEFAULT (1)
                                           NOT NULL,
                    can_write      BOOLEAN DEFAULT (0)
                                           NOT NULL
                );
                """
                Order = 11 }
              { Name = "users"
                Sql = """
                CREATE TABLE users (
                    reference  TEXT (36) NOT NULL
                                         UNIQUE
                                         PRIMARY KEY,
                    username   TEXT      NOT NULL
                                         UNIQUE,
                    password   TEXT      NOT NULL,
                    salt       TEXT      NOT NULL,
                    created_on DATETIME  NOT NULL
                );
                """
                Order = 9 } ]

module Configuration =
    [<CLIMutable>]
    type Config =
        { [<JsonPropertyName("blobTypes")>]
          BlobTypes: BlobTypeConfig seq
          [<JsonPropertyName("hashTypes")>]
          HashTypes: string seq
          [<JsonPropertyName("version")>]
          Version: int
          [<JsonPropertyName("sourceTypes")>]
          SourceTypes: string seq
          [<JsonPropertyName("users")>]
          Users: string seq
          [<JsonPropertyName("encryptionTypes")>]
          EncryptionTypes: string seq
          [<JsonPropertyName("collections")>]
          Collections: CollectionConfig seq
          [<JsonPropertyName("generalReference")>]
          GeneralReference: Guid }

    and [<CLIMutable>] CollectionConfig =
        { [<JsonPropertyName("name")>]
          Name: string
          [<JsonPropertyName("reference")>]
          Reference: string
          [<JsonPropertyName("allowAnonymousRead")>]
          AllowAnonymousRead: bool
          [<JsonPropertyName("allowAnonymousWrite")>]
          AllowAnonymousWrite: bool }
   
    and [<CLIMutable>] BlobTypeConfig =
        { [<JsonPropertyName("name")>]
          Name: string
          [<JsonPropertyName("contentType")>]
          ContentType: string
          [<JsonPropertyName("extension")>]
          Extension: string }
         
    let defaultConfigPath = "FBlob-config.json"

    let loadConfig path =
        let json = File.ReadAllText path
        JsonSerializer.Deserialize<Config> json

    let loadDefaultConfig = loadConfig defaultConfigPath