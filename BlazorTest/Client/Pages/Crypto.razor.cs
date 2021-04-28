using System;
using System.Diagnostics;
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
        private string _keyLabel;
        private string _ivLabel;

        private SubtleCryptoManager GetSubtleCrypto()
        {
            return _subtleCrypto ??= new SubtleCryptoManager(JS);
        }

        private async Task<AesCbc> GetKey()
        {
            if (_key == null)
            {
                _key = await GetSubtleCrypto().ImportAesCbcKeyAsync(new byte[16]);
                _keyLabel = Convert.ToHexString(await _key.ExportKey());
            }

            return _key;
        }

        private async Task Encrypt()
        {
            AesCbc key = await GetKey();

            byte[] output = await key.EncryptAsync(
                Encoding.UTF8.GetBytes(_input ?? ""),
                new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

            _output = Convert.ToBase64String(output);
        }

        private async Task Decrypt()
        {
            AesCbc key = await GetKey();

            try
            {
                byte[] output = await key.DecryptAsync(
                    Convert.FromBase64String(_input ?? ""),
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
                _keyLabel = Convert.ToHexString(await _key.ExportKey());
            }
            catch (Exception e)
            {
                _output = e.ToString();
            }
        }
    }
}