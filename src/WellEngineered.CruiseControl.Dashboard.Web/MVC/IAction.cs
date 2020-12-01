namespace WellEngineered.CruiseControl.WebDashboard.MVC
{
	public interface IAction
	{
		IResponse Execute(IRequest request);
	}
}