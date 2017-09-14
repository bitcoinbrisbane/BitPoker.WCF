using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPoker.Crypto
{
    public interface IWallet : IRandom
    {
        String Address { get; }
    }
}
