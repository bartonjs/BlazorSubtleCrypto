using System;
using System.Threading.Tasks;

namespace BlazorTest.Client.SubtleCrypto
{
    public abstract class SymmetricKey : CryptoKey
    {
        internal string AlgorithmName { get; }

        private protected SymmetricKey(SafeCryptoKeyHandle keyHandle, string algorithmName)
            : base(keyHandle)
        {
            AlgorithmName = algorithmName;
        }

        public async Task<byte[]> ExportKeyAsync()
        {
            AnswerOrError response = await KeyHandle.Module.InvokeAsync<AnswerOrError>(
                "exportSecretKey",
                new object[]
                {
                    KeyHandle.Name,
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

            AnswerOrError response = await KeyHandle.Module.InvokeAsync<AnswerOrError>(
                operation,
                new object[]
                {
                    KeyHandle.Name,
                    base64Data,
                    base64Iv,
                    AlgorithmName
                });

            return response.GetAnswerFromBase64();
        }
    }
}