using System;

namespace BitPoker.Crypto
{
    public interface IRandom
    {
        Byte[] GetRandom(Int32 n);
    }
}
