using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BitPoker.Crypto.Tests
{
    public class BitcoinTests
    {
        [Fact]
        public void Should_Create_New_Wallet()
        {
            BitPoker.Crypto.Bitcoin wallet = new Bitcoin(true);
            Assert.NotNull(wallet);
        }
    }
}
