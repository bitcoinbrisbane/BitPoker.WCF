using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace BitPoker.Crypto
{
    public class Bitcoin : IWallet
    {
        //TODO: SECURE STRING
        private readonly ExtKey _privateKey;
        private readonly String _wifKey;

        private NBitcoin.Network _network;

        private NBitcoin.BitcoinAddress _address;

        public String Address { get { return _address.ToString(); } }

        public String PublicKey { get; private set; }

        public IRandom RandomProvider { get; set; }

        public UInt64 Balance { get; private set; }

        public Bitcoin(Boolean testnet = true)
        {
            _network = testnet == true ? Network.TestNet : Network.Main;

            RandomUtils.Random = new UnsecureRandom();
            Key privateKey = new Key(); //Create private key
            BitcoinSecret secret = privateKey.GetBitcoinSecret(_network);

            _address = privateKey.PubKey.GetAddress(_network);
        }

        public Bitcoin(String wifKey, Boolean testnet = true)
        {
            _network = testnet == true ? Network.TestNet : Network.Main;

            NBitcoin.Key key = NBitcoin.Key.Parse(wifKey, _network);
            var secret = key.GetBitcoinSecret(_network); //NBitcoin.BitcoinAddress.Create(wifKey.Trim(), _network);
            _address = secret.GetAddress();
        }

        public Bitcoin(String[] words)
        {
            //Mnemonic mnemo = new Mnemonic(words, Wordlist.English);
            //_privateKey = mnemo.DeriveExtKey("password");

            //Address =_privateKey.Neuter().PubKey.GetAddress(Network.TestNet).ToString();
        }


        public void NewAddress()
        {
            throw new NotImplementedException();
        }

        public void NewAddress(String wifKey)
        {
            throw new NotImplementedException();
        }

        public void NewAddress(String[] words)
        {
            throw new NotImplementedException();
        }

        public String CreateMultiSig(String[] publicKeys, int m)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateBalance(Int32 confirmations = 6)
        {

        }
    }
}
