using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorTest.Client.SubtleCrypto
{
    public sealed partial class SubtleCryptoManager : IAsyncDisposable
    {
        private readonly IJSRuntime _runtime;
        private IJSObjectReference _module;

        public SubtleCryptoManager(IJSRuntime runtime)
        {
            if (runtime is null)
                throw new ArgumentNullException(nameof(runtime));

            _runtime = runtime;
        }

        public Task<AesCbc> CreateAesCbcKey(int keySize)
        {
            switch (keySize)
            {
                case 128:
                case 192:
                case 256:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(keySize));
            }

            return GenerateSymmetricKeyAsync(keySize, "AES-CBC", (handle, name) => new AesCbc(handle, name));
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

        public Task<byte[]> ComputeHashSha1Async(byte[] input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            return ComputeHashAsync("SHA-1", input);
        }

        public Task<byte[]> ComputeHashSha256Async(byte[] input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            return ComputeHashAsync("SHA-256", input);
        }

        private async Task<byte[]> ComputeHashAsync(string algorithm, byte[] input)
        {
            AnswerOrError ret = await (await GetModule()).InvokeAsync<AnswerOrError>(
                "computeDigest",
                new object[]
                {
                    algorithm,
                    Convert.ToBase64String(input),
                });

            return ret.GetAnswerFromBase64();
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

        private async Task<T> GenerateSymmetricKeyAsync<T>(
            int keySize,
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
                    "generateSecretKey",
                    keySize,
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
