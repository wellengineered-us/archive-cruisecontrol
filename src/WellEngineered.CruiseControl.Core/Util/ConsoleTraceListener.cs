using System;
using System.Diagnostics;

namespace WellEngineered.CruiseControl.Core.Util
{
    /// <summary>
    /// 	
    /// </summary>
	public class ConsoleTraceListener : TextWriterTraceListener
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleTraceListener" /> class.	
        /// </summary>
        /// <remarks></remarks>
		public ConsoleTraceListener() : base(Console.Out) { }
	}
}
