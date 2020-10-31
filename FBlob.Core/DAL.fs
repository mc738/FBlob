module FBlob.Core.DAL

open System
open System.IO
open FUtil
open Microsoft.Data.Sqlite
open FBlob.Core.Models


// module Shim =

module Helpers =
    let runNonQuery connection name sql =

        use comm = new SqliteCommand(sql, connection)

        comm.ExecuteNonQuery()

    let runSeedQuery connection name sql (parameters: Map<string, string>) =

        let comm = new SqliteCommand(sql, connection)

        parameters
        |> Map.map (fun k v -> comm.Parameters.AddWithValue(k, v))
        |> ignore

        comm.Prepare()

        comm.ExecuteNonQuery()

module Blobs =

    open Models
    open FUtil.Security

    type NewBlob =
        { Reference: Guid
          CollectionReference: Guid
          Data: byte array
          KeyRef: string
          Type: BlobType
          Encrypted: bool
          HashType: HashType
          EncryptionType: EncryptionType }


    let private insertBlob connection newBlob hash =
        // https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/blob-io
        let sql = """
        INSERT INTO blobs (reference, collection_ref, data, hash, salt, created_on, metadata_blob, key_ref, type, path, encrypted, hash_type, encryption_type)
        VALUES (@ref, @collectionRef, zeroblob(@len), @hash, @salt, @now, zeroblob(1), @keyRef, @type, @path, @encrypted, @hashType, @encryptionType);
        SELECT last_insert_rowid();
        """

        // TODO Add error handling in here.
        use comm = new SqliteCommand(sql, connection)

        comm.Parameters.AddWithValue("@ref", newBlob.Reference.ToString())
        |> ignore
        comm.Parameters.AddWithValue("@collectionRef", newBlob.CollectionReference.ToString())
        |> ignore // Note - don't remove .ToString() here or it fails!
        comm.Parameters.AddWithValue("@len", newBlob.Data.Length)
        |> ignore
        comm.Parameters.AddWithValue("@hash", hash)
        |> ignore
        comm.Parameters.AddWithValue("@salt", Conversions.bytesToHex (generateSalt 16)) // TODO Test this!
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
            new SqliteBlob(connection, "blobs", "data", rowId)

        ms.CopyTo(wStream)

        Ok(rowId)

    let add connection (newBlob: NewBlob) =
        // Prepare the new blob and insert.
        // TODO Make preparation a pipeline.
        match Hashing.hashData newBlob.HashType newBlob.Data with
        | Ok h -> insertBlob connection newBlob h
        | Error e -> Error e

    let private createBlobFromReader (ms: MemoryStream) (reader: SqliteDataReader) =

        // Reset and empty the memory, to prevent data leakage.
        ms.Flush()
        ms.Position <- 0L
        
        let dataStream = reader.GetStream(2)

        dataStream.CopyTo(ms)

        { Reference = reader.GetGuid(0)
          CollectionRef = reader.GetGuid(1)
          Data = ms.ToArray()
          Hash = reader.GetString(3)
          Salt = reader.GetString(4)
          CreatedOn = reader.GetDateTime(5)
          MetaDataBlob = ""
          KeyRef = reader.GetString(6)
          Encrypted = reader.GetBoolean(7)
          Path = reader.GetString(8)
          Type =
              { Name = reader.GetString(9)
                ContentType = reader.GetString(10)
                Extension = reader.GetString(11) }
          Properties = []
          HashType = { Name = reader.GetString(12) }
          EncryptionType = { Name = reader.GetString(13) } }
    
    let getByReference connection (reference: Guid) =

        let sql = """
        SELECT
	        b.reference, b.collection_ref, b."data", b.hash, b.salt, b.created_on, b.key_ref, b.encrypted, b."path", bt.name, bt.content_type, bt.extension, ht.name, et.name
        FROM
	        blobs b
        JOIN collections c
        ON b.collection_ref = c.reference
        JOIN blob_types bt
        ON b."type" = bt.name
        JOIN hash_types ht
        ON b.hash_type = ht.name
        JOIN encryption_types et
        ON b.encryption_type = et.name
        WHERE b.reference = @ref;
        """

        use comm = new SqliteCommand(sql, connection)

        comm.Parameters.AddWithValue("@ref", reference.ToString())
        |> ignore

        comm.Prepare()

        use ms = new MemoryStream()

        use reader = comm.ExecuteReader()

        let r =
            [ while reader.Read() do
                yield createBlobFromReader ms reader ]

        match r.Length with
        | 1 -> Some r.Head
        | 0 -> None
        | _ -> Some r.Head // In the future this should be handled differently

    let getByCollection connection (categoryReference: Guid) =
        let sql = """
        SELECT
	        b.reference, b.collection_ref, b."data", b.hash, b.salt, b.created_on, b.key_ref, b.encrypted, b."path", bt.name, bt.content_type, bt.extension, ht.name, et.name
        FROM
	        blobs b
        JOIN collections c
        ON b.collection_ref = c.reference
        JOIN blob_types bt
        ON b."type" = bt.name
        JOIN hash_types ht
        ON b.hash_type = ht.name
        JOIN encryption_types et
        ON b.encryption_type = et.name
        WHERE b.collection_ref = @collectionRef;
        """

        use comm = new SqliteCommand(sql, connection)

        comm.Parameters.AddWithValue("@collectionRef", categoryReference.ToString())
        |> ignore

        comm.Prepare()

        use ms = new MemoryStream()

        use reader = comm.ExecuteReader()

        
        [ while reader.Read() do
            yield createBlobFromReader ms reader ]

module Collections =
    type NewCollection =
        { Reference: Guid
          Name: string
          AnonymousRead: bool
          AnonymousWrite: bool }
    
    let add connection collection =
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

        Helpers.runSeedQuery connection "create_collection" sql p


    let get connection reference =
        let sql = """
        SELECT * FROM collections WHERE reference = @ref;
        """

        use comm = new SqliteCommand(sql, connection)
        
        comm.Parameters.AddWithValue("@ref", reference.ToString()) |> ignore
        
        comm.Prepare()
        
        use reader = comm.ExecuteReader()

        let c = [
            while reader.Read() do
                        
            yield {
                Reference = reader.GetGuid(0)
                Name = reader.GetString(1)
                CreatedOn = DateTime.Now // TODO fix this bug! reader.GetDateTime(2)
                MetaData = ""
                AnonymousRead = reader.GetBoolean(4)
                AnonymousWrite = reader.GetBoolean(5)
                Blobs = Blobs.getByCollection connection reference
                Properties = []
                Sources = []
                UserPermissions = Map.empty
            }
        ]
        
        match c.Length with
        | 1 -> Some c.Head
        | 0 -> None
        | _ -> Some c.Head
            
        
        