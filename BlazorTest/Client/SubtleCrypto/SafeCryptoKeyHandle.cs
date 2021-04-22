using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorTest.Client.SubtleCrypto
{
    internal sealed class SafeCryptoKeyHandle : SafeHandle
    {
        internal IJSObjectReference Module { get; private set; }
        internal string Name { get; private set; }

        public SafeCryptoKeyHandle(string name, IJSObjectReference module)
            : base(IntPtr.Zero, true)
        {
            Module = module;
            Name = name;
        }

        protected override bool ReleaseHandle()
        {
            // Fire and forget.
            _ = ReleaseHandleAsync();
            return true;
        }

        internal async Task ReleaseHandleAsync()
        {
            await Module.InvokeVoidAsync("freeKey", Name);
            Name = null;
            Module = null;
            SetHandleAsInvalid();
        }

        public override bool IsInvalid => Name is null;
    }
}