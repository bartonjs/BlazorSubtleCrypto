using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorTest.Client.SubtleCrypto
{
    partial class SubtleCryptoManager
    {
        public Task<RsaPkcs1SignatureKey> CreateRsaPkcs1SignatureKey(
            int keySize,
            SubtleHashAlgorithm hashAlgorithm)
        {
            if (keySize < 1024)
                throw new ArgumentOutOfRangeException(nameof(keySize));

            hashAlgorithm.Validate(nameof(hashAlgorithm));

            return GenerateRsaKeyAsync(
                keySize,
                "RSASSA-PKCS1-v1_5",
                hashAlgorithm.Name,
                true,
                (handle, alg, hashAlg) => new RsaPkcs1SignatureKey(handle, alg, hashAlg));
        }

        private async Task<T> GenerateRsaKeyAsync<T>(
            int keySize,
            string algorithmName,
            string hashAlgorithmName,
            bool signatureKey,
            Func<SafeCryptoKeyHandle, string, string, T> ctor)
        {
            bool? result;
            IJSObjectReference module = await GetModule();
            string keyId;

            do
            {
                keyId = Guid.NewGuid().ToString("N");
                result = await module.InvokeAsync<bool?>(
                    "generateRsaKey",
                    keySize,
                    keyId,
                    algorithmName,
                    hashAlgorithmName,
                    signatureKey);
            } while (!result.HasValue);

            if (result.GetValueOrDefault())
            {
                SafeCryptoKeyHandle handle = new SafeCryptoKeyHandle(keyId, module);
                return ctor(handle, algorithmName, hashAlgorithmName);
            }

            throw new Exception("?");
        }
    }
}