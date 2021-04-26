using System;
using System.Threading.Tasks;

namespace BlazorTest.Client.SubtleCrypto
{
    public sealed class AesCbc : SymmetricKey
    {
        internal AesCbc(SafeCryptoKeyHandle keyHandle, string algorithmName)
            : base(keyHandle, algorithmName)
        {
        }

        public Task<byte[]> EncryptAsync(byte[] data, byte[] iv)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));
            if (iv is null)
                throw new ArgumentNullException(nameof(iv));
            if (iv.Length != 16)
                throw new ArgumentOutOfRangeException(nameof(iv));

            return EncryptAsyncCore(data, iv);
        }

        public Task<byte[]> DecryptAsync(byte[] data, byte[] iv)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));
            if (iv is null)
                throw new ArgumentNullException(nameof(iv));
            if (iv.Length != 16)
                throw new ArgumentOutOfRangeException(nameof(iv));

            return DecryptAsyncCore(data, iv);
        }
    }
}