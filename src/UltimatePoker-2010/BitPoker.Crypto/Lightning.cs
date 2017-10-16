using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPoker.Crypto
{
    public class Lightning : Bitcoin
    {
        public IChannel Channel { get; set; }

        public void OpenChannel()
        {
            //TODO:
        }

        public void CloseChannel()
        {
            //TODO:
        }
    }
}
