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

        private SubtleCryptoManager GetSubtleCrypto()
        {
            return _subtleCrypto ??= new SubtleCryptoManager(JS);
        }

        private async Task<AesCbc> GetKey()
        {
            return _key ??= await GetSubtleCrypto().ImportAesCbcKeyAsync(new byte[16]);
        }

        private async Task Encrypt()
        {
            AesCbc key = await GetKey();

            //SubtleCrypto.CryptoKey key = await sc.ImportSecretKey(new byte[16], SubtleCrypto.Algorithm.AesCbc);
            //Debug.Assert(key != null);

            //string output = await sc.Encrypt(
            //    Encoding.UTF8.GetBytes(_input ?? ""),
            //    new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15},
            //    key,
            //    SubtleCrypto.Algorithm.AesCbc);

            //_output = output;
            //_output = await sc.AesCbcEncrypt(
            //    new byte[16],
            //    new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
            //    Encoding.UTF8.GetBytes(_input ?? ""));
            byte[] output = await key.EncryptAsync(
                Encoding.UTF8.GetBytes(_input ?? ""),
                new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

            _output = Convert.ToBase64String(output);
        }
    }
}