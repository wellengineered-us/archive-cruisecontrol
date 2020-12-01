using System;
using System.Runtime.InteropServices;

using WellEngineered.CruiseControl.Core.Util;

namespace WellEngineered.CruiseControl.Core
{
	/// <summary>
	/// Intercepts events raised by the command console.
	/// </summary>
	public class ConsoleEventHandler : IDisposable
	{
		private ControlEventHandler handler;
		private ExecutionEnvironment environment = new ExecutionEnvironment();
        /// <summary>
        /// Occurs when [on console event].	
        /// </summary>
        /// <remarks></remarks>
		public event EventHandler OnConsoleEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleEventHandler" /> class.	
        /// </summary>
        /// <remarks></remarks>
		public ConsoleEventHandler()
		{
			if (this.environment.IsRunningOnWindows)
			{
				this.handler = new ControlEventHandler(this.HandleControlEvent);
				SetConsoleCtrlHandler(this.handler, true);
			}
		}


		private void HandleControlEvent(ConsoleEvent consoleEvent)
		{
			lock (this)
			{
				if (this.OnConsoleEvent != null)
				{
					this.OnConsoleEvent(this, EventArgs.Empty);
				}
			}
		}

		void IDisposable.Dispose()
		{
			lock (this)
			{
				this.UnregisterHandler();
			}
		}

		private void UnregisterHandler()
		{
			if (this.handler != null)
			{
				SetConsoleCtrlHandler(this.handler, false);
				this.handler = null;
			}
		}

		private delegate void ControlEventHandler(ConsoleEvent consoleEvent);

		// From wincom.h
		private enum ConsoleEvent
		{
			CTRL_C = 0,
			CTRL_BREAK = 1,
			CTRL_CLOSE = 2,
			CTRL_LOGOFF = 5,
			CTRL_SHUTDOWN = 6
		}

		[DllImport("kernel32.dll")]
		private static extern bool SetConsoleCtrlHandler(ControlEventHandler e, bool add);
	}
}