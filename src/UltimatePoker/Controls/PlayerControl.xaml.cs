using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using PokerEngine;

namespace UltimatePoker
{
    /// <summary>
    /// A basic control which displays a general player. This control handles no user input.
    /// </summary>
	public partial class PlayerControl
	{
        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="PlayerControl"/> class.</para>
        /// </summary>
		public PlayerControl()
		{
			this.InitializeComponent();
		}
	}
}