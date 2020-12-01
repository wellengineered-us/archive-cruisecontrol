using System.Collections;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public interface IBuildLogTransformer
	{
        string Transform(IBuildSpecifier build, string[] transformerFileNames, Hashtable xsltArgs, string sessionToken);
	}
}