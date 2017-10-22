using System;
using System.Threading.Tasks;

namespace BitPoker.Crypto
{
    public interface IWallet
    {
        String PublicKey { get; }

        String Address { get; }

        UInt64 Balance { get; }
    }
}
