using System;
using System.IO;

namespace WellEngineered.CruiseControl.Core.SourceControl
{
    /// <summary>
    /// 	
    /// </summary>
	public interface IHistoryParser
	{
        /// <summary>
        /// Parses the specified history.	
        /// </summary>
        /// <param name="history">The history.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		Modification[] Parse(TextReader history, DateTime from, DateTime to);
	}
}
