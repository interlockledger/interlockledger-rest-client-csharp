// ******************************************************************************************************************************
//
// Copyright (c) 2018-2021 InterlockLedger Network
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of the copyright holder nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES, LOSS OF USE, DATA, OR PROFITS, OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// ******************************************************************************************************************************

namespace InterlockLedger.Rest.Client.V6_0;

public static class EncryptedTextModelExtensions
{
    public static string DecodedWith(this EncryptedTextModel model, X509Certificate2 certificate) {
        if (certificate is null)
            return "ERROR: No key provided to decode EncryptedText";
        if (!certificate.HasPrivateKey)
            return "ERROR: Certificate has no private key to be able to decode EncryptedText";
        string certKeyId = certificate.ToKeyId();
        string pubKeyHash = certificate.ToPubKeyHash();
        if (pubKeyHash is null)
            return "ERROR: Non-RSA certificate is not currently supported";
        if (model.ReadingKeys.SkipNulls().None())
            return "ERROR: No reading keys able to decode EncryptedText";
        var authorizedKey = model.ReadingKeys.FirstOrDefault(rk => rk.PublicKeyHash == pubKeyHash && rk.ReaderId == certKeyId);
        if (authorizedKey is null)
            return "ERROR: Your key does not match one of the authorized reading keys";
        string cipher = model.Cipher.WithDefault("AES256").ToUpperInvariant();
        if (cipher != "AES256")
            return $"ERROR: Cipher {cipher} is not currently supported";
        if (model.CipherText.None())
            return null;
        using var rsaAlgo = certificate.GetRSAPrivateKey();
        var aesKey = RSADecrypt(rsaAlgo, authorizedKey.EncryptedKey);
        var aesIV = RSADecrypt(rsaAlgo, authorizedKey.EncryptedIV);
        var jsonBytes = AES256Decrypt(model.CipherText, aesKey, aesIV);
        if (jsonBytes[0] != 17)
            return "ERROR: Something went wrong while decrypting the content. Unexpected initial bytes";
        var skipTagAndSize = jsonBytes[1..].ILIntDecode().ILIntSize() + 1;
        return jsonBytes[skipTagAndSize..].AsUTF8String();

        static byte[] RSADecrypt(RSA rsaAlgo, byte[] data, int maxRetries = 3) {
            int retries = maxRetries;
            while (true)
                try {
                    try {
                        return rsaAlgo.Decrypt(data, RSAEncryptionPadding.OaepSHA1);
                    } catch (CryptographicException) {
                        return rsaAlgo.Decrypt(data, RSAEncryptionPadding.CreateOaep(HashAlgorithmName.MD5));
                    }
                } catch (CryptographicException e) {
                    if (retries-- <= 0)
                        throw new InvalidOperationException($"Failed to decrypt data with current parameters after {maxRetries} retries", e);
                }
        }

        static byte[] AES256Decrypt(byte[] cipherData, byte[] key, byte[] iv) {
            using var source = new MemoryStream(cipherData.Required());
            using var algorithm = Aes.Create();
            algorithm.KeySize = 256;
            algorithm.BlockSize = 128;
            algorithm.Mode = CipherMode.CBC;
            algorithm.Key = key;
            algorithm.IV = iv;
            algorithm.Padding = PaddingMode.Zeros;
            using var cs = new CryptoStream(source, algorithm.CreateDecryptor(), CryptoStreamMode.Read);
            using var dest = new MemoryStream();
            cs.CopyTo(dest);
            return dest.ToArray();
        }
    }
}
