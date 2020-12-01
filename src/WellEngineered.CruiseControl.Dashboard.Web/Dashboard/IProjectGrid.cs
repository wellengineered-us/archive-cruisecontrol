using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.Resources;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public interface IProjectGrid
	{
        ProjectGridRow[] GenerateProjectGridRows(ProjectStatusOnServer[] statusList, 
            string forceBuildActionName, 
            ProjectGridSortColumn sortColumn, 
            bool sortIsAscending, 
            string categoryFilter, 
            ICruiseUrlBuilder urlBuilder, 
            Translations translations);
	}
}
