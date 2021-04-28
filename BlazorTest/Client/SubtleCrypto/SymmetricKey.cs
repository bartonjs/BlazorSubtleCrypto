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

        public async Task<byte[]> ExportKey()
        {
            AnswerOrError response = await _keyHandle.Module.InvokeAsync<AnswerOrError>(
                "exportSecretKey",
                new object[]
                {
                    _keyHandle.Name,
                });

            return response.GetAnswerFromBase64();
        }

        protected Task<byte[]> EncryptAsyncCore(byte[] data, byte[] iv)
        {
            return EncryptOrDecryptAsync("symmetricEncrypt", data, iv);
        }

        protected Task<byte[]> DecryptAsyncCore(byte[] data, byte[] iv)
        {
            return EncryptOrDecryptAsync("symmetricDecrypt", data, iv);
        }

        private async Task<byte[]> EncryptOrDecryptAsync(string operation, byte[] data, byte[] iv)
        {
            string base64Data = Convert.ToBase64String(data);
            string base64Iv = null;

            if (iv != null)
            {
                base64Iv = Convert.ToBase64String(iv);
            }

            AnswerOrError response = await _keyHandle.Module.InvokeAsync<AnswerOrError>(
                operation,
                new object[]
                {
                    _keyHandle.Name,
                    base64Data,
                    base64Iv,
                    AlgorithmName
                });

            return response.GetAnswerFromBase64();
        }
    }
}