using System;
using System.Threading.Tasks;

namespace BitPoker.Crypto
{
    public interface IWallet : IRandom
    {
        String PublicKey { get; }

        String Address { get; }
    }
}
