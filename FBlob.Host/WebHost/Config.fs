namespace FBlob.Host.WebHost.Config


open FBlob.Core.StoreConfig

module Initialization =
    let i = ()


/// The tables needed for the web host.
module Tables =
    
    let tables = [
        {
            Name = "__wh_content"
            Sql = """
            CREATE TABLE __wh_content (
                        reference       VARCHAR (36) PRIMARY KEY
                                                     UNIQUE
                                                     NOT NULL,
                        data            BLOB         NOT NULL,
                        hash            TEXT         NOT NULL,
                        salt            TEXT         NOT NULL
                                                     UNIQUE,
                        created_on      DATETIME     NOT NULL,
                        type            TEXT         REFERENCES blob_types (name)
                                                     NOT NULL,
                        path            VARCHAR      NOT NULL,
                        hash_type       TEXT         REFERENCES hash_types (name)
                                                     NOT NULL
                    );
            """
            Order = 100
        }
    ]
    
    
    let i = ()