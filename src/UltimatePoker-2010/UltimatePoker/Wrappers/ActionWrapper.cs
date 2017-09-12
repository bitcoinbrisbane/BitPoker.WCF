using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace UltimatePoker
{
	public class ActionWrapper : INotifyPropertyChanged
	{
		public ActionWrapper(GuiActions action)
		{
			this.action = action;
		}

		private GuiActions action;
		public GuiActions Action
		{
			get { return action; }
		}

		private bool isOn = true;

		public bool IsOn
		{
			get { return isOn; }
			set 
			{ 
				isOn = value;
				RaiseChangedEvent("IsOn");
			}
		}


        private object tag;

        public object Tag
        {
            get
            {
                return (tag);
            }
            set
            {
                if (tag != value)
                {
                    tag = value;
                    RaiseChangedEvent("Tag");
                }
            }
        }
            
		private void RaiseChangedEvent(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
