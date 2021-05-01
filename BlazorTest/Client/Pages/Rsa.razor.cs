using System;
using System.Text;
using System.Threading.Tasks;
using BlazorTest.Client.SubtleCrypto;

namespace BlazorTest.Client.Pages
{
    partial class Rsa
    {
        private string _input;
        private string _input2;
        private string _output;

        private SubtleCryptoManager _subtleCrypto;
        private RsaPkcs1SignatureKey _rsaPkcs1Key;
        private string _keyLabel;

        private SubtleCryptoManager GetSubtleCrypto()
        {
            return _subtleCrypto ??= new SubtleCryptoManager(JS);
        }

        private async Task Encrypt()
        {
            //AesCbc key = await GetKey();

            //byte[] output = await key.EncryptAsync(
            //    Encoding.UTF8.GetBytes(_input ?? ""),
            //    new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

            //_output = Convert.ToBase64String(output);
            _output = "NYI";
        }

        private async Task Decrypt()
        {
            //AesCbc key = await GetKey();

            //try
            //{
            //    byte[] output = await key.DecryptAsync(
            //        Convert.FromBase64String(_input ?? ""),
            //        new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15});

            //    _output = Encoding.UTF8.GetString(output);
            //}
            //catch (Exception e)
            //{
            //    _output = e.ToString();
            //}
            _output = "NYI";
        }

        private async Task GenerateKey()
        {
            try
            {
                int keySize = int.Parse(_input);

                RsaPkcs1SignatureKey newKey = await GetSubtleCrypto().CreateRsaPkcs1SignatureKey(
                    keySize,
                    SubtleHashAlgorithm.Sha256);

                if (_rsaPkcs1Key != null)
                {
                    await _rsaPkcs1Key.DisposeAsync();
                }

                _rsaPkcs1Key = newKey;
                _keyLabel = Convert.ToHexString(await newKey.ExportPrivateKeyAsync());
            }
            catch (Exception e)
            {
                _output = e.ToString();
            }
        }
    }
}