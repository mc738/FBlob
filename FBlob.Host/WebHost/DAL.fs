module FBlob.Host.WebHost.DAL

open System.IO
open Microsoft.Data.Sqlite



module Content =
    
    type StoredContent = {
        Content: byte array
        ContentTypeLiteral: string
    }
    
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
    
    let i = 0
    
    
module Settings =
    
    let i = 0