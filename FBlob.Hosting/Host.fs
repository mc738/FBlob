module FBlob.Hosting

open FBlob.Core.Store
open FBlob.Hosting.Engine
open Peeps

type HostType =
    | WebHost
    | REPL
    
and Host = {
    HostType: HostType
    Logger: Logger
    Instance: Instance
    Run: (RunType -> Instance)
}

// let createHost hostType logger instance