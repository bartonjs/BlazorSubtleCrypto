using System;
using System.Threading.Tasks;

namespace BlazorTest.Client.SubtleCrypto
{
    public abstract class CryptoKey : IDisposable, IAsyncDisposable
    {
        private protected SafeCryptoKeyHandle KeyHandle { get; private set; }

        private protected CryptoKey(SafeCryptoKeyHandle keyHandle)
        {
            KeyHandle = keyHandle;
        }

        public void Dispose()
        {
            KeyHandle?.Dispose();
            KeyHandle = null;
        }

        public ValueTask DisposeAsync()
        {
            if (KeyHandle == null)
            {
                return ValueTask.CompletedTask;
            }

            return new ValueTask(KeyHandle.ReleaseHandleAsync());
        }
    }
}