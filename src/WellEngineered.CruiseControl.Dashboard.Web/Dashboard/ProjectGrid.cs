using System.Collections.Generic;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.WebDashboard.Plugins.ProjectReport;
using WellEngineered.CruiseControl.WebDashboard.Resources;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
    public class ProjectGrid : IProjectGrid
	{
		public ProjectGridRow[] GenerateProjectGridRows(ProjectStatusOnServer[] statusList, string forceBuildActionName,
		                                                ProjectGridSortColumn sortColumn, bool sortIsAscending, string categoryFilter,
                                                        ICruiseUrlBuilder urlBuilder, Translations translations) 
		{
			var rows = new List<ProjectGridRow>();
			foreach (ProjectStatusOnServer statusOnServer in statusList)
			{
				ProjectStatus status = statusOnServer.ProjectStatus;
				IServerSpecifier serverSpecifier = statusOnServer.ServerSpecifier;
				DefaultProjectSpecifier projectSpecifier = new DefaultProjectSpecifier(serverSpecifier, status.Name);
				
				if ((categoryFilter != string.Empty) && (categoryFilter != status.Category))
					continue;

				rows.Add(
					new ProjectGridRow(status,
					                   serverSpecifier,
                                       urlBuilder.BuildProjectUrl(ProjectReportProjectPlugin.ACTION_NAME, projectSpecifier),
                                       urlBuilder.BuildProjectUrl(ProjectParametersAction.ActionName, projectSpecifier),
                                       translations));
			}

			rows.Sort(this.GetComparer(sortColumn, sortIsAscending));

			return rows.ToArray();
		}

		private IComparer<ProjectGridRow> GetComparer(ProjectGridSortColumn column, bool ascending)
		{
			return new ProjectGridRowComparer(column, ascending);
		}

		private class ProjectGridRowComparer 
            : IComparer<ProjectGridRow>
		{
			private readonly ProjectGridSortColumn column;
			private readonly bool ascending;

			public ProjectGridRowComparer(ProjectGridSortColumn column, bool ascending)
			{
				this.column = column;
				this.ascending = ascending;
			}

            public int Compare(ProjectGridRow x, ProjectGridRow y)
			{
				if (this.column == ProjectGridSortColumn.Name)
				{
					return x.Name.CompareTo(y.Name)*(this.@ascending ? 1 : -1);
				}
				else if (this.column == ProjectGridSortColumn.LastBuildDate)
				{
					return x.LastBuildDate.CompareTo(y.LastBuildDate)*(this.@ascending ? 1 : -1);
				}
				else if (this.column == ProjectGridSortColumn.BuildStatus)
				{
					return x.BuildStatus.CompareTo(y.BuildStatus)*(this.@ascending ? 1 : -1);
				}
				else if (this.column == ProjectGridSortColumn.ServerName)
				{
					return x.ServerName.CompareTo(y.ServerName)*(this.@ascending ? 1 : -1);
				}
				else if (this.column == ProjectGridSortColumn.Category)
                {
                    return x.Category.CompareTo(y.Category)*(this.@ascending ? 1 : -1);
                } 
				else
				{
					return 0;
				}
			}
		}
	}
}
