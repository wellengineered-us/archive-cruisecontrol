using System;

using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core
{
    /// <summary>
    /// 	
    /// </summary>
	public static class IntegrationStatusParser
	{
        /// <summary>
        /// Parses the specified value.	
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public static IntegrationStatus Parse(string value)
		{
			return (IntegrationStatus)Enum.Parse(typeof(IntegrationStatus), value);
		}
	}
}
