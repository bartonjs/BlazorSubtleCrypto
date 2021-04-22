using System;
using System.Threading.Tasks;

namespace BlazorTest.Client.SubtleCrypto
{
    public abstract class SymmetricKey : IDisposable, IAsyncDisposable
    {
        private SafeCryptoKeyHandle _keyHandle;

        internal string AlgorithmName { get; }

        private protected SymmetricKey(SafeCryptoKeyHandle keyHandle, string algorithmName)
        {
            _keyHandle = keyHandle;
            AlgorithmName = algorithmName;
        }

        public void Dispose()
        {
            _keyHandle?.Dispose();
            _keyHandle = null;
        }

        public ValueTask DisposeAsync()
        {
            if (_keyHandle == null)
            {
                return ValueTask.CompletedTask;
            }

            return new ValueTask(_keyHandle.ReleaseHandleAsync());
        }

        protected async Task<byte[]> EncryptAsyncCore(byte[] data, byte[] iv)
        {
            string base64Data = Convert.ToBase64String(data);
            string base64Iv = null;

            if (iv != null)
            {
                base64Iv = Convert.ToBase64String(iv);
            }

            string base64Answer = await _keyHandle.Module.InvokeAsync<string>(
                "encrypt",
                new object[]
                {
                    _keyHandle.Name,
                    base64Data,
                    base64Iv,
                    AlgorithmName
                });

            return Convert.FromBase64String(base64Answer);
        }
    }

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
    }
}