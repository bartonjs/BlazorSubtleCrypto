using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorTest.Client.SubtleCrypto
{
    public sealed class SubtleCryptoManager : IAsyncDisposable
    {
        private readonly IJSRuntime _runtime;
        private IJSObjectReference _module;

        public SubtleCryptoManager(IJSRuntime runtime)
        {
            if (runtime is null)
                throw new ArgumentNullException(nameof(runtime));

            _runtime = runtime;
        }

        public Task<AesCbc> ImportAesCbcKeyAsync(byte[] key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            switch (key.Length)
            {
                case 16:
                case 24:
                case 32:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(key));
            }

            return ImportSymmetricKeyAsync(key, "AES-CBC", (handle, name) => new AesCbc(handle, name));
        }

        public ValueTask DisposeAsync()
        {
            IJSObjectReference module = _module;
            _module = null;

            if (module != null)
            {
                return module.DisposeAsync();
            }

            return ValueTask.CompletedTask;
        }

        private async ValueTask<IJSObjectReference> GetModule()
        {
            if (_module == null)
            {
                _module = await _runtime.InvokeAsync<IJSObjectReference>("import", "./subtleCryptoSupport.js");
            }

            return _module;
        }

        private async Task<T> ImportSymmetricKeyAsync<T>(
            byte[] key,
            string algorithmName,
            Func<SafeCryptoKeyHandle, string, T> ctor)
        {
            bool? result;
            IJSObjectReference module = await GetModule();
            string keyId;

            do
            {
                keyId = Guid.NewGuid().ToString("N");
                result = await module.InvokeAsync<bool?>(
                    "importSecretKey",
                    Convert.ToBase64String(key),
                    keyId,
                    algorithmName);
            } while (!result.HasValue);

            if (result.GetValueOrDefault())
            {
                SafeCryptoKeyHandle handle = new SafeCryptoKeyHandle(keyId, module);
                return ctor(handle, algorithmName);
            }

            throw new Exception("?");
        }
    }
}
