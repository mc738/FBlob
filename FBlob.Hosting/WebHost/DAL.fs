module FBlob.Hosting.WebHost.DAL

open System
open System.IO
open FBlob.Core.Models
open FUtil
open Microsoft.Data.Sqlite

module Content =
    
    type NewContent =
        { Reference: Guid
          Data: byte array
          Type: BlobType
          HashType: HashType }
    
    type StoredContent = {
        Content: byte array
        ContentTypeLiteral: string
    }
    
    let private insertBlob connection newContent hash =
        // https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/blob-io
        let sql = """
        INSERT INTO "__wh_content" (reference, data, hash, created_on, type, path, hash_type)
        VALUES (@ref, zeroblob(@len), @hash, @now, @type, @path, @hashType);
        SELECT last_insert_rowid();
        """

        // TODO Add error handling in here.
        use comm = new SqliteCommand(sql, connection)

        comm.Parameters.AddWithValue("@ref", newContent.Reference.ToString())
        |> ignore
        comm.Parameters.AddWithValue("@len", newContent.Data.Length)
        |> ignore
        comm.Parameters.AddWithValue("@hash", hash)
        |> ignore
        comm.Parameters.AddWithValue("@now", DateTime.UtcNow)
        |> ignore
        comm.Parameters.AddWithValue("@type", newContent.Type.Name)
        |> ignore
        comm.Parameters.AddWithValue("@path", "some/path")
        |> ignore
        comm.Parameters.AddWithValue("@hashType", newContent.HashType.Name)
        |> ignore
        comm.Prepare()
        let rowId = comm.ExecuteScalar() :?> int64

        use ms = new MemoryStream(newContent.Data)

        use wStream =
            new SqliteBlob(connection, "__wh_content", "data", rowId)

        ms.CopyTo(wStream)

        Ok(rowId)

    let add connection (newContent: NewContent) =
        // Prepare the new blob and insert.
        // TODO Make preparation a pipeline.
        match FBlob.Core.Hashing.hashData newContent.HashType newContent.Data with
        | Ok h -> insertBlob connection newContent h
        | Error e -> Error e
    
    let get connection reference =
        
        let sql = """
        SELECT
            c.data,
            bt.content_type
        FROM __wh_content c
        JOIN blob_types bt
        ON c.type = bt.name
        WHERE c.reference = @ref
        """
        
        use comm = new SqliteCommand(sql, connection)
        
        comm.Parameters.AddWithValue("@ref", reference.ToString()) |> ignore
        
        use ms = new MemoryStream()

        use reader = comm.ExecuteReader()

        let r =
            [ while reader.Read() do
                
                let dataStream = reader.GetStream(0)

                dataStream.CopyTo(ms)
                
                yield { Content = ms.ToArray(); ContentTypeLiteral = reader.GetString(1) } ]

        match r.Length with
        | 1 -> Some r.Head
        | 0 -> None
        | _ -> Some r.Head // In the future this should be handled differently

    let i = 0
    
module Routes =
    
    type NewRoute = {
        Route: string
        Settings: byte array
    }
    
    let i = 0
    
module Settings =
    
    let i = 0