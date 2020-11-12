# FBlob

`FBlob` is a lightweight blob store written in `F#` using `Sqlite`.

It's aim is to be flexible in terms of data storage, integrations and functionality.

## Features

* **Encryption**

* **Sources**
    * Pull data from sources to populate a `Collections`.
    * Send data from the store to external sources.

* **Integration**
    * You can use a store via a library, host an instance locally or even on a network via a `Http` server.

* **Extensions**
    * Add and extend functionality to the store.

* **More the just a store**
    * Rather than just storing data, `FBlob` looks to help you process that data.
    * `FBlob` looks to support as many blob types as possible (from json to binary files).



## Solution

`FBlob` is split into 3 main projects:

* [FBlob.Core](/docs/FBlob.Core.md)
    * Handles core `FBlob` functionality.

* [FBlob.Host](/docs/FBlob.Host.md)
    * Handles hosting `FBlob` store instances.
    * Provides the building blocks for building hosts.
    * Supports `Http` and local hosting.

* [FBlob.App](/docs/FBlob.App.md)
    * Provides a basic app shell for running a `FBlob` hosted instance.

## Projects

* **Inspired**

* **Release Manager**
    * A tool for managing releases.
    * Scripts can be stored to create consistent build chains.