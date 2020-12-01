using System;

using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core
{
    /// <summary>
    /// 	
    /// </summary>
	public interface ICruiseServerFactory
	{
        /// <summary>
        /// Creates the specified remote.	
        /// </summary>
        /// <param name="remote">The remote.</param>
        /// <param name="configFile">The config file.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		ICruiseServer Create(bool remote, string configFile);
	}
}
