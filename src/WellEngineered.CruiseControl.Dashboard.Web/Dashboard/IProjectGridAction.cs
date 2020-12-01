using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public interface IProjectGridAction
    {
        #region Properties
        #region DefaultSortColumn
        /// <summary>
        /// The default column to sort by.
        /// </summary>
        ProjectGridSortColumn DefaultSortColumn { get; set; }
        #endregion

        #region SuccessIndicatorBarLocation
        /// <summary>
        /// Gets or sets the success indicator bar location.
        /// </summary>
        /// <value>The success indicator bar location.</value>
        IndicatorBarLocation SuccessIndicatorBarLocation { get; set; }
        #endregion
        #endregion

        IResponse Execute(string actionName, ICruiseRequest request);
        IResponse Execute(string actionName, IServerSpecifier serverSpecifer, ICruiseRequest request);
	}
}
