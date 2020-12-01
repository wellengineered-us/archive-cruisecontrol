using System.Collections;

namespace WellEngineered.CruiseControl.WebDashboard.MVC.View
{
	public interface IVelocityViewGenerator
	{
		HtmlFragmentResponse GenerateView(string templateName, Hashtable velocityContext);
	}
}
