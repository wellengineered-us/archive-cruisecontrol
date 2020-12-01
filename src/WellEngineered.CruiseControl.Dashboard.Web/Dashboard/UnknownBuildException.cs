using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public class UnknownBuildException : CruiseControlException
	{
		private readonly IBuildSpecifier buildSpecifier;

		public UnknownBuildException(IBuildSpecifier buildSpecifier) : base()
		{
			this.buildSpecifier = buildSpecifier;
		}

		public IBuildSpecifier BuildSpecifier
		{
			get { return this.buildSpecifier; }
		}
	}
}
