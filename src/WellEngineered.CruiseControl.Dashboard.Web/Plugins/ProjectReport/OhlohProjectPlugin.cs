using System.Collections;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.ProjectReport
{
    /// <title>Ohloh Stats Display Plugin</title>
    /// <version>1.5</version>
    /// <summary>
    /// Display Ohloh stats for a project.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;ohlohProjectPlugin /&gt;
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// Configuration is done in ccnet.config via the linkedSites property
    /// </para>
    /// <para type="tip">
    /// This can be installed using the "Ohloh" package.
    /// </para>
    /// </remarks>
    [ReflectorType("ohlohProjectPlugin")]
    public class OhlohProjectPlugin
        : ICruiseAction, IPlugin
    {
        #region Private fields
        private const string ActionName = "ViewOhlohProjectStats";
        private readonly IFarmService farmService;
        private readonly IVelocityViewGenerator viewGenerator;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="OhlohProjectPlugin"/> class.
        /// </summary>
        /// <param name="farmService">The farm service.</param>
        /// <param name="viewGenerator">The view generator.</param>
        public OhlohProjectPlugin(IFarmService farmService, IVelocityViewGenerator viewGenerator)
		{
			this.farmService = farmService;
			this.viewGenerator = viewGenerator;
        }
        #endregion

        #region Public properties
        #region LinkDescription
        /// <summary>
        /// Gets the link description.
        /// </summary>
        /// <value>The link description.</value>
        public string LinkDescription
        {
            get { return "View Ohloh Stats"; }
        }
        #endregion

        #region NamedActions
        /// <summary>
        /// Gets the named actions.
        /// </summary>
        /// <value>The named actions.</value>
        public INamedAction[] NamedActions
        {
            get { return new INamedAction[] { new ImmutableNamedAction(ActionName, this) }; }
        }
        #endregion
        #endregion

        #region Public methods
        #region Execute()
        /// <summary>
        /// Executes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The response for the request.</returns>
        public IResponse Execute(ICruiseRequest request)
		{
            var ohloh = this.farmService.GetLinkedSiteId(request.ProjectSpecifier,
                request.RetrieveSessionToken(),
                "ohloh");

            if (string.IsNullOrEmpty(ohloh))
            {
                return new HtmlFragmentResponse("<div>This project has not been linked to a project in Ohloh</div>");
            }
            else
            {
                var velocityContext = new Hashtable();
                velocityContext["ohloh"] = ohloh;
                velocityContext["projectName"] = request.ProjectName;

                return this.viewGenerator.GenerateView(@"OhlohStats.vm", velocityContext);
            }
		}
        #endregion
        #endregion
    }
}
