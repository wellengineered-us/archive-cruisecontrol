using System.Collections;

namespace WellEngineered.CruiseControl.WebDashboard.MVC.View
{
	public interface IVelocityTransformer
	{
		string Transform(string templateName, Hashtable velocityContext);
	}
}
