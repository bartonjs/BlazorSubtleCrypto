using System;

namespace BlazorTest.Client.SubtleCrypto
{
    internal struct AnswerOrError
    {
        public string A { get; set; }
        public bool B { get; set; }
        public string E { get; set; }

        internal byte[] GetAnswerFromBase64()
        {
            if (E != null)
            {
                throw new Exception(E);
            }

            return Convert.FromBase64String(A);
        }

        internal bool GetAnswerFromBoolean()
        {
            if (E != null)
            {
                throw new Exception(E);
            }

            return B;
        }
    }
}