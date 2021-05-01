using System;
using System.Threading.Tasks;

namespace BlazorTest.Client.SubtleCrypto
{
    public abstract class AsymmetricKey : IDisposable, IAsyncDisposable
    {
        private SafeCryptoKeyHandle _keyHandle;

        public string AlgorithmName { get; }

        private protected AsymmetricKey(SafeCryptoKeyHandle keyHandle, string algorithmName)
        {
            AlgorithmName = algorithmName;
            _keyHandle = keyHandle;
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

        public async Task<byte[]> ExportPublicKeyAsync()
        {
            AnswerOrError response = await _keyHandle.Module.InvokeAsync<AnswerOrError>(
                "exportPublicKey",
                new object[]
                {
                    _keyHandle.Name,
                });

            return response.GetAnswerFromBase64();
        }

        public async Task<byte[]> ExportPrivateKeyAsync()
        {
            AnswerOrError response = await _keyHandle.Module.InvokeAsync<AnswerOrError>(
                "exportPrivateKey",
                new object[]
                {
                    _keyHandle.Name,
                });

            return response.GetAnswerFromBase64();
        }

        protected async Task<byte[]> SignDataAsyncCore(ReadOnlyMemory<byte> data)
        {
            string base64Data = Convert.ToBase64String(data.Span);

            AnswerOrError response = await _keyHandle.Module.InvokeAsync<AnswerOrError>(
                "signData",
                new object[]
                {
                    _keyHandle.Name,
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

            AnswerOrError response = await _keyHandle.Module.InvokeAsync<AnswerOrError>(
                "verifyData",
                new object[]
                {
                    _keyHandle.Name,
                    AlgorithmName,
                    base64Data,
                    base64Signature,
                });

            return response.GetAnswerFromBoolean();
        }
    }
}