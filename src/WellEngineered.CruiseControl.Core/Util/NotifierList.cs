using System;
using System.Collections;

namespace WellEngineered.CruiseControl.Core.Util
{
    /// <summary>
    /// 	
    /// </summary>
    /// <param name="value">The value.</param>
	public delegate void NotifierDelegate(object value);

    /// <summary>
    /// 	
    /// </summary>
	public class NotifierList : ArrayList
	{
		private event NotifierDelegate _addEvent;
		private event NotifierDelegate _removeEvent;

        /// <summary>
        /// Adds the delegate for add event.	
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <remarks></remarks>
		public virtual void AddDelegateForAddEvent(NotifierDelegate handler)
		{
			this._addEvent += handler;
		}

        /// <summary>
        /// Adds the delegate for remove event.	
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <remarks></remarks>
		public virtual void AddDelegateForRemoveEvent(NotifierDelegate handler)
		{
			this._removeEvent += handler;
		}

        /// <summary>
        /// Adds the specified value.	
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public override int Add(object value)
		{
			int index = base.Add(value);
			this.SendNotification(this._addEvent, value);
			return index;
		}

        /// <summary>
        /// Removes the specified value.	
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks></remarks>
		public override void Remove(object value)
		{
			base.Remove(value);
			this.SendNotification(this._removeEvent, value);
		}

		private void SendNotification(NotifierDelegate handler, object value)
		{
			if (handler != null) 
			{
				handler(value);
			}
		}

	}
}
