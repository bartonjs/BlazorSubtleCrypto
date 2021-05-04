using System;
using System.Threading.Tasks;

namespace BlazorTest.Client.SubtleCrypto
{
    public abstract class AsymmetricKey : CryptoKey
    {
        public string AlgorithmName { get; }

        private protected AsymmetricKey(SafeCryptoKeyHandle keyHandle, string algorithmName)
            : base(keyHandle)
        {
            AlgorithmName = algorithmName;
        }

        public async Task<byte[]> ExportPublicKeyAsync()
        {
            AnswerOrError response = await KeyHandle.Module.InvokeAsync<AnswerOrError>(
                "exportPublicKey",
                new object[]
                {
                    KeyHandle.Name,
                });

            return response.GetAnswerFromBase64();
        }

        public async Task<byte[]> ExportPrivateKeyAsync()
        {
            AnswerOrError response = await KeyHandle.Module.InvokeAsync<AnswerOrError>(
                "exportPrivateKey",
                new object[]
                {
                    KeyHandle.Name,
                });

            return response.GetAnswerFromBase64();
        }

        protected async Task<byte[]> SignDataAsyncCore(ReadOnlyMemory<byte> data)
        {
            string base64Data = Convert.ToBase64String(data.Span);

            AnswerOrError response = await KeyHandle.Module.InvokeAsync<AnswerOrError>(
                "signData",
                new object[]
                {
                    KeyHandle.Name,
                    AlgorithmName,
                    base64Data,
                });

            return response.GetAnswerFromBase64();
        }

        protected async Task<bool> VerifyDataAsyncCore(
            ReadOnlyMemory<byte> data,
            ReadOnlyMemory<byte> signature)
        {
            string base64Data = Convert.ToBase64String(data.Span);
            string base64Signature = Convert.ToBase64String(signature.Span);

            AnswerOrError response = await KeyHandle.Module.InvokeAsync<AnswerOrError>(
                "verifyData",
                new object[]
                {
                    KeyHandle.Name,
                    AlgorithmName,
                    base64Data,
                    base64Signature,
                });

            return response.GetAnswerFromBoolean();
        }
    }
}