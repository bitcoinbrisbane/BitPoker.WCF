using System;

namespace BitPoker.Crypto
{
    public interface IChannel
    {
        Boolean IsOpen { get; }

        void Open();

        void Fund();

        void Close();
    }
}
