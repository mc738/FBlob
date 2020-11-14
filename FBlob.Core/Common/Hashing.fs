module FBlob.Core.Common.Hashing

open Models

let (md5: HashType) = { Name = "MD5" }

let (sha1: HashType) = { Name = "SHA1" }

let (sha256: HashType) = { Name = "SHA256" }

let (sha384: HashType) = { Name = "SHA384" }

let (sha512: HashType) = { Name = "SHA512" }

let hashData (hashType: HashType) data =
    match hashType.Name with
    | "MD5" -> Ok(FUtil.Hashing.md5Hex data)
    | "SHA1" -> Ok(FUtil.Hashing.sha1Hex data)
    | "SHA256" -> Ok(FUtil.Hashing.sha256Hex data)
    | "SHA384" -> Ok(FUtil.Hashing.sha384Hex data)
    | "SHA512" -> Ok(FUtil.Hashing.sha512Hex data)
    | _ -> Error(sprintf "Algorithm `%s` not supported" hashType.Name)