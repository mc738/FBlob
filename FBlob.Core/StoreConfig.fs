module FBlob.Core.StoreConfig

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