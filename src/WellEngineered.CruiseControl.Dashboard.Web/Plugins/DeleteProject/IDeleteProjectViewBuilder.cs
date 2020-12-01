using WellEngineered.CruiseControl.WebDashboard.MVC;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.DeleteProject
{
	public interface IDeleteProjectViewBuilder
	{
		IResponse BuildView(DeleteProjectModel model);
	}
}
