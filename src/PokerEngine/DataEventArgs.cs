using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerEngine
{
    /// <summary>
    /// A generic event arguments class.
    /// </summary>
    /// <typeparam name="T">The type of the data passed by the arguments</typeparam>
    public class DataEventArgs<T> : EventArgs
    {
        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="DataEventArgs{T}"/> class.</para>
        /// </summary>
        /// <param name="data">The data which this event args hold
        /// </param>
        public DataEventArgs(T data)
        {
            Data = data;
        }

        /// <summary>
        /// Gets the data which was passed by the event
        /// </summary>
        public T Data { get; private set; }
    }

    /// <summary>
    /// A generic event handler
    /// </summary>
    /// <typeparam name="T">The type of the Data Event Args passed</typeparam>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">The argument passed by the event</param>
    public delegate void DataEventHandler<T>(object sender, DataEventArgs<T> e);
}
