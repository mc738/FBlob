module FBlob.Core.DAL

open System
open System.IO
open Microsoft.Data.Sqlite
open Peeps
open FBlob.Core.Configuration
open FBlob.Core.Models
open FBlob.Core.Models.Config

type Context =
    { Connection: SqliteConnection
      Cache: Map<Guid, byte array>
      Logger: Logger
      GeneralReference: Guid }

// module Shim =

module Helpers =
    let runNonQuery context name sql =

        use comm =
            new SqliteCommand(sql, context.Connection)

        comm.ExecuteNonQuery()

    let runSeedQuery context name sql (parameters: Map<string, string>) =

        let comm =
            new SqliteCommand(sql, context.Connection)

        parameters
        |> Map.map (fun k v -> comm.Parameters.AddWithValue(k, v))
        |> ignore

        comm.Prepare()

        comm.ExecuteNonQuery()

module Initialization =

    let defaultConfigPath = "FBlob-config.json"

    let createTable conn table =
        Helpers.runNonQuery conn table.Name table.Sql

    /// Create the initialize tables in `Sqlite` tables.
    let createTables conn =

        let handler = createTable conn

        let results =
            tables
            |> List.sortBy (fun t -> t.Order)
            |> List.map handler

        results

    let seedHashTypes conn supportedTypes =

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

    let seedBlobTypes context (blobTypes: BlobTypeConfig seq) =
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

    let seedSourceTypes conn supportedTypes =

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

    let seedEncryptionTypes context supportedTypes =
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

    let createCollection context reference name (anonRead: bool) (anonWrite: bool) =
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

    let createGeneralCollection context generalReference =
        createCollection context generalReference "_general" true true

    let createCollections context (collections: CollectionConfig seq) =
        collections
        |> List.ofSeq
        |> List.map (fun c -> createCollection context c.Reference c.Name c.AllowAnonymousRead c.AllowAnonymousWrite)

    /// Seed initial data.
    let seedData context (config: Config) =
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

module Blobs =

    type NewBlob =
        { Reference: Guid
          CollectionReference: Guid
          Data: byte array
          KeyRef: string
          Type: BlobType
          Encrypted: bool
          HashType: HashType
          EncryptionType: EncryptionType }

    let add (context: Context) (newBlob: NewBlob) =
        let hash =
            Convert.ToBase64String(FUtil.Hashing.sha1 newBlob.Data)
        // https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/blob-io
        let sql = """
        INSERT INTO blobs (reference, collection_ref, data, hash, salt, created_on, metadata_blob, key_ref, type, path, encrypted, hash_type, encryption_type)
        VALUES (@ref, @collectionRef, zeroblob(@len), @hash, @salt, @now, zeroblob(1), @keyRef, @type, @path, @encrypted, @hashType, @encryptionType);
        SELECT last_insert_rowid();
        """

        use comm =
            new SqliteCommand(sql, context.Connection)

        comm.Parameters.AddWithValue("@ref", newBlob.Reference.ToString())
        |> ignore
        comm.Parameters.AddWithValue("@collectionRef", context.GeneralReference.ToString())
        |> ignore // Note - don't remove .ToString() here or it fails!
        comm.Parameters.AddWithValue("@len", newBlob.Data.Length)
        |> ignore
        comm.Parameters.AddWithValue("@hash", hash)
        |> ignore
        comm.Parameters.AddWithValue("@salt", Convert.ToBase64String(FUtil.Passwords.generateSalt 16))
        |> ignore
        comm.Parameters.AddWithValue("@now", DateTime.UtcNow)
        |> ignore
        comm.Parameters.AddWithValue("@keyRef", newBlob.KeyRef)
        |> ignore
        comm.Parameters.AddWithValue("@type", newBlob.Type.Name)
        |> ignore
        comm.Parameters.AddWithValue("@path", "some/path")
        |> ignore
        comm.Parameters.AddWithValue("@encrypted", newBlob.Encrypted)
        |> ignore
        comm.Parameters.AddWithValue("@hashType", newBlob.HashType.Name)
        |> ignore
        comm.Parameters.AddWithValue("@encryptionType", newBlob.EncryptionType.Name)
        |> ignore
        comm.Prepare()
        let rowId = comm.ExecuteScalar() :?> int64

        use ms = new MemoryStream(newBlob.Data)

        use wStream =
            new SqliteBlob(context.Connection, "blobs", "data", rowId)

        ms.CopyTo(wStream)
        
    let get (context: Context) (reference: Guid) =
        
        let sql = """
SELECT * FROM blobs
WHERE reference = @ref
"""
        use comm = new SqliteCommand(sql, context.Connection)
        
        comm.Parameters.AddWithValue("@ref", reference.ToString()) |> ignore
        
        comm.Prepare()
        
        use ms = new MemoryStream()
        
        use reader = comm.ExecuteReader()
    
        let r = [
            while reader.Read() do
                let reference = reader.GetGuid(0)
                let collectionRef = reader.GetGuid(1)
                let dataStream = reader.GetStream(2)

                dataStream.CopyTo(ms)
                    
                let i = (reference, collectionRef, ms.ToArray())

                                        
                yield i
        ]
            
        match r.Length with
        | 1 -> Some r.Head
        | 0 -> None
        | _ -> Some r.Head // In the future this should be handled differently

module Collections =
    type NewCollection =
        { Reference: Guid
          Name: string
          AnonymousRead: bool
          AnonymousWrite: bool }

    let add context collection =
        let sql = """
        INSERT INTO collections
        VALUES (@ref, @name, @now, zeroblob(1), @anonRead, @anonWrite)
        """

        let p =
            [ ("@ref", collection.Reference.ToString())
              ("@name", collection.Name)
              ("@now", DateTime.Now.ToString())
              ("@anonRead", collection.AnonymousRead.ToString())
              ("@anonWrite", collection.AnonymousWrite.ToString()) ]
            |> Map.ofList

        Helpers.runSeedQuery context "create_collection" sql p


let createStore path = File.WriteAllBytes(path, Array.empty)

let createContext path generalRef =
    let connString = sprintf "Data Source=%s" path

    use conn = new SqliteConnection(connString)

    let logger = Logger()

    { Connection = conn
      Logger = logger
      GeneralReference = generalRef
      Cache = Map.empty }

let initializeStore (config: Config) path =

    let context =
        createContext path config.GeneralReference

    context.Connection.Open()

    Initialization.createTables context |> ignore
    Initialization.seedData context config |> ignore

    context

// Create a new blob store object