using System;

using WellEngineered.CruiseControl.Core.Util;

namespace WellEngineered.CruiseControl.Core.SourceControl.Telelogic
{
    /// <summary>
    /// 	
    /// </summary>
	public interface ISynergyCommand : IDisposable
	{
        /// <summary>
        /// Executes the specified process info.	
        /// </summary>
        /// <param name="processInfo">The process info.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		ProcessResult Execute(ProcessInfo processInfo);
        /// <summary>
        /// Executes the specified process info.	
        /// </summary>
        /// <param name="processInfo">The process info.</param>
        /// <param name="failOnError">The fail on error.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		ProcessResult Execute(ProcessInfo processInfo, bool failOnError);
	}
}