module FBlob.Core.DAL

open System
open System.IO
open Microsoft.Data.Sqlite
open FBlob.Core.Models


// module Shim =

module Helpers =
    let runNonQuery connection name sql =

        use comm =
            new SqliteCommand(sql, connection)

        comm.ExecuteNonQuery()

    let runSeedQuery connection name sql (parameters: Map<string, string>) =

        let comm =
            new SqliteCommand(sql, connection)

        parameters
        |> Map.map (fun k v -> comm.Parameters.AddWithValue(k, v))
        |> ignore

        comm.Prepare()

        comm.ExecuteNonQuery()

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

    let add connection (newBlob: NewBlob) =
        let hash =
            HashTypes.toHex(FUtil.Hashing.sha1 newBlob.Data)
        // https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/blob-io
        let sql = """
        INSERT INTO blobs (reference, collection_ref, data, hash, salt, created_on, metadata_blob, key_ref, type, path, encrypted, hash_type, encryption_type)
        VALUES (@ref, @collectionRef, zeroblob(@len), @hash, @salt, @now, zeroblob(1), @keyRef, @type, @path, @encrypted, @hashType, @encryptionType);
        SELECT last_insert_rowid();
        """

        use comm =
            new SqliteCommand(sql, connection)

        comm.Parameters.AddWithValue("@ref", newBlob.Reference.ToString())
        |> ignore
        comm.Parameters.AddWithValue("@collectionRef", newBlob.CollectionReference.ToString())
        |> ignore // Note - don't remove .ToString() here or it fails!
        comm.Parameters.AddWithValue("@len", newBlob.Data.Length)
        |> ignore
        comm.Parameters.AddWithValue("@hash", hash)
        |> ignore
        comm.Parameters.AddWithValue("@salt", HashTypes.toHex(FUtil.Passwords.generateSalt 16))
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
        
    let getByReference connection (reference: Guid) =
        
        let sql = """
SELECT * FROM blobs
WHERE reference = @ref
"""
        use comm = new SqliteCommand(sql, connection)
        
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