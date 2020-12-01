using WellEngineered.CruiseControl.WebDashboard.IO;

namespace WellEngineered.CruiseControl.WebDashboard.MVC
{
	public interface IActionFactory
	{
		IAction Create(IRequest request);
	    IConditionalGetFingerprintProvider CreateFingerprintProvider(IRequest request);
	}
}
