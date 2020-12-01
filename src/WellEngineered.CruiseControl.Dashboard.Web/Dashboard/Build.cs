using WellEngineered.CruiseControl.Core;
using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public class Build
	{
		private readonly IBuildSpecifier buildSpecifier;
		private readonly string log;

		public Build(IBuildSpecifier buildSpecifier, string log)
		{
			this.log = log;
			this.buildSpecifier = buildSpecifier;
		}

		public string Log
		{
			get { return this.log; }
		}

		public IBuildSpecifier BuildSpecifier
		{
			get { return this.buildSpecifier; }
		}

		public bool IsSuccessful
		{
			get { return new LogFile(this.buildSpecifier.BuildName).Succeeded; }
		}
	}
}
