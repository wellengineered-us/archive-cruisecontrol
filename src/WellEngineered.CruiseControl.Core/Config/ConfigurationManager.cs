using System.Collections;
using System.Collections.Generic;

namespace WellEngineered.CruiseControl.Core.Config
{
	internal class ConfigurationManager
	{
		private readonly static Dictionary<string, string> appSettings = new Dictionary<string, string>();

		public static Dictionary<string, string> AppSettings
		{
			get
			{
				return appSettings;
			}
		}

		public static IList GetSection(string xslfiles)
		{
			return null;
		}
	}
}