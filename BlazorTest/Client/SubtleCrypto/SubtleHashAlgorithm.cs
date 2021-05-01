using System;

namespace BlazorTest.Client.SubtleCrypto
{
    public readonly struct SubtleHashAlgorithm
    {
        public static readonly SubtleHashAlgorithm Sha1 = new SubtleHashAlgorithm("SHA-1");
        public static readonly SubtleHashAlgorithm Sha256 = new SubtleHashAlgorithm("SHA-256");
        public static readonly SubtleHashAlgorithm Sha384 = new SubtleHashAlgorithm("SHA-384");
        public static readonly SubtleHashAlgorithm Sha512 = new SubtleHashAlgorithm("SHA-512");

        public string Name { get; }

        public SubtleHashAlgorithm(string name)
        {
            Name = name;
        }

        internal void Validate(string paramName)
        {
            if (string.IsNullOrEmpty(Name))
            {
                throw new ArgumentOutOfRangeException(paramName);
            }
        }
    }
}