module FBlob.Core.Store

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open FBlob.Core.DAL
open FBlob.Core.Models

let getBlob (context:Context) (reference:Guid) =
    None
    
let addBlob (context:Context) (blob:Blob) =
    
    Error "Not implement"