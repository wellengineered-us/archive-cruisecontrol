using System;

namespace WellEngineered.CruiseControl.Core
{
    /// <summary>
    /// 	
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Gets the now.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        DateTime Now { get; }
    }
}