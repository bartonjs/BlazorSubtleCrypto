using System;
using System.Threading.Tasks;

namespace BlazorTest.Client.SubtleCrypto
{
    public sealed class RsaPkcs1SignatureKey : AsymmetricKey
    {
        public SubtleHashAlgorithm HashAlgorithm { get; }

        internal RsaPkcs1SignatureKey(
            SafeCryptoKeyHandle keyHandle,
            string algorithmName,
            string hashAlg)
            : base(keyHandle, algorithmName)
        {
            HashAlgorithm = new SubtleHashAlgorithm(hashAlg);
        }

        public Task<byte[]> SignDataAsync(byte[] data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            return SignDataAsyncCore(data);
        }

        public Task<bool> VerifyDataAsync(byte[] data, byte[] signature)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));
            if (signature is null)
                throw new ArgumentNullException(nameof(signature));

            return VerifyDataAsyncCore(data, signature);
        }
    }
}