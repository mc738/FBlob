module FBlob.Core.Common.Encryption

open FUtil.Security

let private encryptor context data = encryptBytesAes context data

let private decryptor context cipher = decryptBytesAes context cipher

/// Encrypt data and append the IV to the front.
/// The IV will be 16 bytes.
let encrypt (keys: Map<string, byte array>) keyRef data =
    match keys.TryFind keyRef with
    | Some k ->
        let context = { Key = k; IV = generateSalt 16 }

        let encrypted = encryptor context data

        let r = Array.append context.IV encrypted

        Ok(r)

    | None -> Error(sprintf "Key `%s` not found." keyRef)

let decrypt (keys: Map<string, byte array>) keyRef data =

    match (keys.TryFind keyRef, Array.length data >= 16) with
    | Some k, true ->
        let (iv, cipher) = Array.splitAt 16 data

        let context = { Key = k; IV = iv }

        Ok(decryptor context cipher)
    | None, _ -> Error(sprintf "Key `%s` not found." keyRef)
    | _, false -> Error "Data length less than 16, no attached iv." 
