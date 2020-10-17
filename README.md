# FBlob

FBlob is a portable blob store based around (currently) `Sqlite`.

The goal is to offer a simple yet flexible way to store blobs in your application.

# Architecture

`FBlob` uses a 3 tier architecture:

* Database 
    * Currently a `Sqlite` database.

* DAL

* Store
    * The part users interact with.