using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.Security
{
    /// <title>Simple Security Plugin</title>
    /// <version>1.5</version>
    /// <summary>
    /// Allows the user to send a user name and password to the server for authentication.
    /// </summary>
    /// <example>
    /// <code title="Minimalist Example">
    /// &lt;simpleSecurity /&gt;
    /// </code>
    /// <code title="Full Example">
    /// &lt;simpleSecurity hidePassword="true" /&gt;
    /// </code>
    /// </example>
    /// <remarks>
    /// </remarks>
    [ReflectorType("simpleSecurity")]
    public class UserNameSecurityPlugin
        : ISecurityPlugin
    {
        #region Private constants
        private const string actionName = "SimpleUserLogin";
        #endregion

        #region Private fields
        private readonly IFarmService farmService;
        private readonly IVelocityViewGenerator viewGenerator;
        private ISessionStorer storer;
        private bool hidePassword;
        #endregion

        #region Constructors
        public UserNameSecurityPlugin(IFarmService farmService, IVelocityViewGenerator viewGenerator,
            ISessionStorer storer)
        {
            this.farmService = farmService;
            this.viewGenerator = viewGenerator;
            this.storer = storer;
        }
        #endregion

        #region Public properties
        public INamedAction[] NamedActions
        {
            get { return new INamedAction[] { new ImmutableNamedAction(actionName, new UserNameSecurityAction(this.farmService, this.viewGenerator, this.storer, this.hidePassword)) }; }
        }

        public string LinkDescription
        {
            get { return "Simple Login"; }
        }

        public ISessionStorer SessionStorer
        {
            get { return this.storer; }
            set { this.storer = value; }
        }

        /// <summary>
        /// Whether to hide the password field or not.
        /// </summary>
        /// <version>1.5</version>
        /// <default>false</default>
        [ReflectorProperty("hidePassword", Required = false)]
        public bool HidePassword
        {
            get { return this.hidePassword; }
            set { this.hidePassword = value; }
        }
        #endregion

        #region Public methods
        public bool IsAllowedForServer(IServerSpecifier serviceSpecifier)
        {
            return true;
        }
        #endregion
    }
}
