using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorTest.Client
{
    public sealed class SubtleCryptoTake1
    {
        public const string AesCbc = "AES-CBC";
        public const string RsaSsaPkcs1 = "RSASSA-PKCS1-v1_5";

        private readonly IJSRuntime _runtime;
        private IJSObjectReference _module;

        public SubtleCryptoTake1(IJSRuntime runtime)
        {
            if (runtime is null)
                throw new ArgumentNullException(nameof(runtime));

            _runtime = runtime;
        }

        private async ValueTask<IJSObjectReference> GetModule()
        {
            if (_module == null)
            {
                _module = await _runtime.InvokeAsync<IJSObjectReference>("import", "./subtleCryptoSupport.js");
            }

            return _module;
        }

        public async ValueTask<string> AesCbcEncrypt(byte[] key, byte[] iv, byte[] data)
        {
            IJSObjectReference module = await GetModule();
            return await module.InvokeAsync<string>(
                "symmetricEncrypt",
                Base64UrlEncode(data),
                Base64UrlEncode(key),
                Base64UrlEncode(iv),
                "AES-CBC");
        }

        public async Task<CryptoKey> ImportSecretKey(byte[] keyData, Algorithm algorithm)
        {
            bool? result = null;
            IJSObjectReference module = await GetModule();
            string keyId;

            do
            {
                keyId = Guid.NewGuid().ToString("N");
                result = await module.InvokeAsync<bool?>(
                    "importSecretKey",
                    Base64UrlEncode(keyData),
                    keyId,
                    algorithm);
            } while (!result.HasValue);

            if (result.GetValueOrDefault())
            {
                return new CryptoKey(keyId);
            }

            throw new Exception("?");
        }

        public async Task<CryptoKey> ImportSecretKey2(byte[] keyData, Algorithm algorithm)
        {
            IJSObjectReference obj = await _runtime.InvokeAsync<IJSObjectReference>(
                "window.crypto.subtle.importKey",
                "jwk",
                new JsonWebKey(keyData),
                algorithm,
                false,
                new[] {"encrypt", "decrypt"});

            return new CryptoKey(obj);
        }

        public async Task<string> Encrypt(byte[] data, byte[] iv, CryptoKey key, Algorithm algorithm)
        {
            IJSObjectReference module = await GetModule();

            string base64 = await module.InvokeAsync<string>(
                "encrypt",
                key.KeyId,
                Base64UrlEncode(data),
                Base64UrlEncode(iv),
                algorithm.Name);

            return base64;
        }

        public async Task<string> Encrypt2(byte[] data, byte[] iv, CryptoKey key, Algorithm algorithm)
        {
            IJSObjectReference realKey = key.JsObject;

            IJSObjectReference obj = await _runtime.InvokeAsync<IJSObjectReference>(
                "window.crypto.subtle.encrypt",
                new AlgorithmWithIv(algorithm, iv),
                key,
                Base64UrlEncode(data));

            return obj.ToString();
        }

        private static string Base64UrlEncode(byte[] data)
        {
            return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_').Replace("=", "");
        }

        private sealed class JsonWebKey
        {
            public string kty { get; set; }
            public string k { get; set; }
            //public string alg { get; set; }
            public bool ext { get; set; }

            public JsonWebKey(byte[] octets)
            {
                kty = "oct";
                k = Base64UrlEncode(octets);
                ext = true;
            }
        }

        public sealed class Algorithm
        {
            public static Algorithm AesCbc { get; } = new Algorithm("AES-CBC");
            public static Algorithm RsaSsaPkcs1 { get; } = new Algorithm("RSASSA-PKCS1-v1_5");

            public string Name { get; }

            public Algorithm(string name)
            {
                Name = name;
            }
        }

        private sealed class AlgorithmWithIv
        {
            public string Name { get; set; }
            public string Iv { get; set; }

            public AlgorithmWithIv()
            {
            }

            public AlgorithmWithIv(Algorithm alg, byte[] iv)
            {
                Name = alg.Name;
                Iv = Base64UrlEncode(iv);
            }
        }

        public sealed class CryptoKey
        {
            internal IJSObjectReference JsObject { get; }
            internal string KeyId { get; }

            internal CryptoKey(IJSObjectReference jsObject)
            {
                JsObject = jsObject;
            }

            internal CryptoKey(string keyId)
            {
                KeyId = keyId;
            }
        }
    }
}
