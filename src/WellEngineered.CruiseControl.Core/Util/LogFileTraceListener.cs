using System;
using System.Diagnostics;
using System.Globalization;

namespace WellEngineered.CruiseControl.Core.Util
{
    /// <summary>
    /// 	
    /// </summary>
	public class LogFileTraceListener : TraceListener
	{
		private TextWriterTraceListener listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFileTraceListener" /> class.	
        /// </summary>
        /// <param name="logfile">The logfile.</param>
        /// <remarks></remarks>
		public LogFileTraceListener(string logfile) : base(logfile) 
		{ 
			this.listener = new TextWriterTraceListener(logfile);
		}

		private string CreateMessage()
		{
			return string.Format(CultureInfo.CurrentCulture,"{0}", DateTime.Now.ToString(CultureInfo.CurrentCulture));
		}

		private string CreateMessage(string category)
		{
			return string.Format(CultureInfo.CurrentCulture,"{0}: {1}", DateTime.Now.ToString(CultureInfo.CurrentCulture), category);
		}

        /// <summary>
        /// Writes the specified message.	
        /// </summary>
        /// <param name="message">The message.</param>
        /// <remarks></remarks>
		public override void Write(string message) 
		{
			this.listener.Write(message, this.CreateMessage());
		}

        /// <summary>
        /// Writes the specified obj.	
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <remarks></remarks>
		public override void Write(object obj) 
		{
			this.listener.Write(obj, this.CreateMessage());
		}

        /// <summary>
        /// Writes the specified message.	
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="category">The category.</param>
        /// <remarks></remarks>
		public override void Write(string message, string category) 
		{
			this.listener.Write(message, this.CreateMessage(category));
		}

        /// <summary>
        /// Writes the specified obj.	
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="category">The category.</param>
        /// <remarks></remarks>
		public override void Write(object obj, string category) 
		{
			this.listener.Write(obj, this.CreateMessage(category));
		}

        /// <summary>
        /// Writes the line.	
        /// </summary>
        /// <param name="message">The message.</param>
        /// <remarks></remarks>
		public override void WriteLine(string message) 
		{
			this.listener.WriteLine(message, this.CreateMessage());
		}

        /// <summary>
        /// Writes the line.	
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <remarks></remarks>
		public override void WriteLine(object obj) 
		{
			this.listener.WriteLine(obj, this.CreateMessage());
		}

        /// <summary>
        /// Writes the line.	
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="category">The category.</param>
        /// <remarks></remarks>
		public override void WriteLine(string message, string category) 
		{
			this.listener.WriteLine(message, this.CreateMessage(category));
		}

        /// <summary>
        /// Writes the line.	
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="category">The category.</param>
        /// <remarks></remarks>
		public override void WriteLine(object obj, string category) 
		{
			this.listener.WriteLine(obj, this.CreateMessage(category));
		}

        /// <summary>
        /// Flushes this instance.	
        /// </summary>
        /// <remarks></remarks>
		public override void Flush()
		{
			base.Flush();
			this.listener.Flush();
		}

        /// <summary>
        /// Closes this instance.	
        /// </summary>
        /// <remarks></remarks>
		public override void Close()
		{
			base.Close();
			this.listener.Close();
		}

        /// <summary>
        /// Disposes the specified disposing.	
        /// </summary>
        /// <param name="disposing">The disposing.</param>
        /// <remarks></remarks>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing) this.listener.Dispose();
		}
	}
}
