using NUnit.Framework;
using System;
using PE = PokerEngine;

namespace BitPoker.Engine.Tests
{
    [TestFixture()]
    public class PotTests
    {
        [Test()]
        public void Should_Raise_Pot()
        {
            PE.Player alice = new PE.Player("msPJhg9GPzMN6twknwmSQvrUKZbZnk51Tv");
            alice.Money = 100;
            
            PE.Betting.Pot pot = new PE.Betting.Pot();
            pot.Raise(alice, 10);
            
            Assert.AreEqual(10, pot.CurrentRaise);
        }
        
        [Test()]
        public void Should_Call_Pot()
        {
            PE.Player alice = new PE.Player("msPJhg9GPzMN6twknwmSQvrUKZbZnk51Tv");
            alice.Money = 100;
            
            PE.Betting.Pot pot = new PE.Betting.Pot();
            pot.Call(alice);
            
            Assert.AreEqual(10, pot.CurrentRaise);
        }
    }
}
