module FBlob.Core.Store

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open FBlob.Core.DAL
open FBlob.Core.Models
open Microsoft.Data.Sqlite
open Peeps
open FBlob.Core.StoreConfig

type Context =
    { Connection: SqliteConnection
      Cache: Map<Guid, byte array>
      Logger: Logger
      GeneralReference: Guid }

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
        let jsonR = FUtil.Files.tryReadText path
        
        match jsonR with
        | Ok json ->
            let configR = FUtil.Serialization.Json.tryDeserialize<Config> json
            match configR with
            | Ok config -> config
            | Result.Error e -> failwith e
        | Result.Error e -> failwith e
        
    let loadDefaultConfig = loadConfig defaultConfigPath

module Initialization =

    open Configuration

    let defaultConfigPath = "FBlob-config.json"
 
    let private createTable conn (table: Table) =
        Helpers.runNonQuery conn table.Name table.Sql
   
    let private seedHashTypes conn supportedTypes =

        let commandText = """
            INSERT INTO hash_types
            VALUES (@name);
            """

        // Need to stop the seq being lazy (i.e. make it list),
        // or nothing happens!
        supportedTypes
        |> List.ofSeq
        |> List.map (fun name ->
            let p = [ ("@name", name) ] |> Map.ofList
            Helpers.runSeedQuery conn "seed_hash_types" commandText p)

    let private seedBlobTypes context (blobTypes: BlobTypeConfig seq) =
        let sql = """
        INSERT INTO blob_types
        VALUES (@name, @contentType, @extension)
        """

        blobTypes
        |> List.ofSeq
        |> List.map (fun t ->
            let p =
                [ ("@name", t.Name)
                  ("@contentType", t.ContentType)
                  ("@extension", t.Extension) ]
                |> Map.ofList

            Helpers.runSeedQuery context "seed_blob_types" sql p)

    let private seedSourceTypes conn supportedTypes =

        let commandText = """
            INSERT INTO source_types
            VALUES (@name);
            """

        // Need to stop the seq being lazy (i.e. make it list),
        // or nothing happens!
        supportedTypes
        |> List.ofSeq
        |> List.map (fun name ->
            let p = [ ("@name", name) ] |> Map.ofList
            Helpers.runSeedQuery conn "seed_source_types" commandText p)

    let private seedEncryptionTypes context supportedTypes =
        let sql = """
            INSERT INTO encryption_types
            VALUES (@name, zeroblob(1));
            """

        // Need to stop the seq being lazy (i.e. make it list),
        // or nothing happens!
        supportedTypes
        |> List.ofSeq
        |> List.map (fun name ->
            let p = [ ("@name", name) ] |> Map.ofList
            Helpers.runSeedQuery context "seed_hash_types" sql p)

    // TODO this should be in `DAL`.
    let private createCollection context reference name (anonRead: bool) (anonWrite: bool) =
        let sql = """
        INSERT INTO collections
        VALUES (@ref, @name, @now, zeroblob(1), @anonRead, @anonWrite)
        """

        let p =
            [ ("@ref", reference)
              ("@name", name)
              ("@now", DateTime.Now.ToString())
              ("@anonRead", anonRead.ToString())
              ("@anonWrite", anonWrite.ToString()) ]
            |> Map.ofList

        Helpers.runSeedQuery context "create_collection" sql p

    let private createGeneralCollection context generalReference =
        createCollection context generalReference "_general" true true

    let private createCollections context (collections: CollectionConfig seq) =
        collections
        |> List.ofSeq
        |> List.map (fun c -> createCollection context c.Reference c.Name c.AllowAnonymousRead c.AllowAnonymousWrite)

    /// Create the initialize tables in `Sqlite` tables.
    let private createTables conn =

        let handler = createTable conn

        let results =
            tables
            |> List.sortBy (fun t -> t.Order)
            |> List.map handler

        results

    
    /// Seed initial data.
    let private seedData context (config: Config) =
        // TODO Make a pipeline.
        seedHashTypes context config.HashTypes |> ignore
        seedBlobTypes context config.BlobTypes |> ignore
        seedSourceTypes context config.SourceTypes
        |> ignore
        seedEncryptionTypes context config.EncryptionTypes
        |> ignore
        createGeneralCollection context (config.GeneralReference.ToString())
        |> ignore
        createCollections context config.Collections
        |> ignore

    let initialize context config =
        createTables context.Connection
        |> ignore
        seedData context.Connection config
        |> ignore

module BlobStore =

    open DAL.Blobs

    let getBlob (context: Context) (reference: Guid) =
        getByReference context.Connection reference

    let getBlobsByCollection (context: Context) (categoryReference: Guid) =
        getByCollection context.Connection categoryReference

    let getGeneralBlobs (context: Context) =
        getByCollection context.Connection context.GeneralReference

    let addGeneralBlob (context: Context) blobType hashType (data: byte array) =

        let reference = Guid.NewGuid()

        let newBlob =
            { Reference = reference
              CollectionReference = context.GeneralReference
              Data = data
              KeyRef = "Secret_key"
              Type = blobType
              Encrypted = false
              HashType = hashType
              EncryptionType = { Name = "None" } }

        match add context.Connection newBlob with
        | Ok _ -> Ok reference
        | Result.Error e -> Result.Error e

module CollectionStore =

    open DAL.Collections

    let addNew context anonRead anonWrite name =
        let ref = Guid.NewGuid()

        let collection =
            { Name = name
              Reference = ref
              AnonymousRead = anonRead
              AnonymousWrite = anonWrite }

        add context.Connection collection |> ignore

        ref

    let get context reference = get context.Connection reference

    let getGeneral context = Collections.get context.Connection context.GeneralReference

let createStore path = File.WriteAllBytes(path, Array.empty)

let createContext path generalRef =
    let connString = sprintf "Data Source=%s" path

    use conn = new SqliteConnection(connString)

    let logger = Logger()

    { Connection = conn
      Logger = logger
      GeneralReference = generalRef
      Cache = Map.empty }

let initializeStore (config: Configuration.Config) path =

    let context =
        createContext path config.GeneralReference

    context.Connection.Open()

    Initialization.initialize context config

    context
