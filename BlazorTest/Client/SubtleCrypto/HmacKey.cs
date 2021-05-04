using System;
using System.Threading.Tasks;

namespace BlazorTest.Client.SubtleCrypto
{
    public sealed class HmacKey : CryptoKey
    {
        public SubtleHashAlgorithm DigestAlgorithm { get; }
        public int OutputLength { get; }

        internal HmacKey(
            SafeCryptoKeyHandle keyHandle,
            string algorithmName,
            int outputLength)
            : base(keyHandle)
        {
            DigestAlgorithm = new SubtleHashAlgorithm(algorithmName);
            OutputLength = outputLength;
        }

        public Task<byte[]> ComputeHashAsync(byte[] data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            return ComputeHashAsyncCore(data);
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

        private async Task<byte[]> ComputeHashAsyncCore(byte[] data)
        {
            string base64Data = Convert.ToBase64String(data);
            
            AnswerOrError response = await KeyHandle.Module.InvokeAsync<AnswerOrError>(
                "computeHmac",
                new object[]
                {
                    KeyHandle.Name,
                    base64Data,
                });

            return response.GetAnswerFromBase64();
        }
    }
}