using System;
using System.Text;
using System.Threading.Tasks;
using BlazorTest.Client.SubtleCrypto;

namespace BlazorTest.Client.Pages
{
    partial class Crypto
    {
        private string _input;
        private string _output;

        private SubtleCryptoManager _subtleCrypto;
        private AesCbc _key;
        private HmacKey _hmacKey;
        private string _hmacKeyLabel;
        private string _keyLabel;
        private string _ivLabel = Convert.ToHexString(new byte[16]);

        private bool DisableHmacButton => string.IsNullOrEmpty(_hmacKeyLabel);

        private SubtleCryptoManager GetSubtleCrypto()
        {
            return _subtleCrypto ??= new SubtleCryptoManager(JS);
        }

        private async Task<AesCbc> GetKey()
        {
            if (_key == null)
            {
                _key = await GetSubtleCrypto().ImportAesCbcKeyAsync(new byte[16]);
                _keyLabel = Convert.ToHexString(await _key.ExportKeyAsync());
            }

            return _key;
        }

        private async Task Encrypt()
        {
            AesCbc key = await GetKey();

            byte[] output = await key.EncryptAsync(
                GetInputBytes(),
                new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

            _output = Convert.ToBase64String(output);
        }

        private async Task Decrypt()
        {
            AesCbc key = await GetKey();

            try
            {
                byte[] output = await key.DecryptAsync(
                    GetInputBytes(),
                    new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15});

                _output = Encoding.UTF8.GetString(output);
            }
            catch (Exception e)
            {
                _output = e.ToString();
            }
        }

        private async Task GenerateKey()
        {
            try
            {
                int keySize = int.Parse(_input);

                AesCbc newKey = await GetSubtleCrypto().CreateAesCbcKey(keySize);

                if (_key != null)
                {
                    await _key.DisposeAsync();
                }

                _key = newKey;
                _keyLabel = Convert.ToHexString(await _key.ExportKeyAsync());
            }
            catch (Exception e)
            {
                _output = e.ToString();
            }
        }

        private async Task Sha1()
        {
            _output = Convert.ToHexString(
                await GetSubtleCrypto().ComputeHashSha1Async(GetInputBytes()));
        }

        private async Task Sha256()
        {
            _output = Convert.ToHexString(
                await GetSubtleCrypto().ComputeHashSha256Async(GetInputBytes()));
        }

        private byte[] GetInputBytes()
        {
            return Encoding.UTF8.GetBytes(_input ?? "");
        }

        private async Task ImportHmacSha256()
        {
            byte[] keyBytes = GetInputBytes();

            try
            {
                HmacKey key = await GetSubtleCrypto().ImportHmacKeySha256Async(keyBytes);
                await ChangeHmacKey(key);
            }
            catch (Exception e)
            {
                _output = e.ToString();
            }
        }

        private async Task ImportHmacSha384()
        {
            byte[] keyBytes = GetInputBytes();

            try
            {
                HmacKey key = await GetSubtleCrypto().ImportHmacKeySha384Async(keyBytes);
                await ChangeHmacKey(key);
            }
            catch (Exception e)
            {
                _output = e.ToString();
            }
        }

        private async Task Hmac()
        {
            try
            {
                _output = Convert.ToHexString(await _hmacKey.ComputeHashAsync(GetInputBytes()));
            }
            catch (Exception e)
            {
                _output = e.ToString();
            }
        }

        private async Task ChangeHmacKey(HmacKey newKey)
        {
            if (_hmacKey != null)
            {
                await _hmacKey.DisposeAsync();
            }

            _hmacKey = newKey;
            byte[] keyBytes = await newKey.ExportKeyAsync();
            _hmacKeyLabel = $"({newKey.DigestAlgorithm.Name}) {Convert.ToHexString(keyBytes)}";
        }
    }
}