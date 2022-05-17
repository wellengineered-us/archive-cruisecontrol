using System.Collections;

using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard.Actions
{
    public class HtmlReportAction
        : ICruiseAction, IConditionalGetFingerprintProvider
    {
        #region Private fields
	    private readonly IFingerprintFactory fingerprintFactory;
        private readonly IFarmService farmService;
        private readonly IVelocityViewGenerator viewGenerator;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialise a new <see cref="HtmlReportAction"/>.
        /// </summary>
        /// <param name="fingerprintFactory"></param>
        /// <param name="farmService"></param>
        /// <param name="viewGenerator"></param>
        public HtmlReportAction(IFingerprintFactory fingerprintFactory, IFarmService farmService,
            IVelocityViewGenerator viewGenerator)
        {
            this.fingerprintFactory = fingerprintFactory;
            this.farmService = farmService;
            this.viewGenerator = viewGenerator;
        }
        #endregion

        #region Public properties
        #region HtmlFileName
        /// <summary>
        /// The name of the file to display.
        /// </summary>
        public string HtmlFileName { get; set; }
        #endregion
        #endregion

        #region Public methods
        #region Execute()
        /// <summary>
        /// Execute the action.
        /// </summary>
        /// <param name="cruiseRequest"></param>
        /// <returns></returns>
        public IResponse Execute(ICruiseRequest cruiseRequest)
		{
            var velocityContext = new Hashtable();
            velocityContext["url"] = string.Format(System.Globalization.CultureInfo.CurrentCulture,"RetrieveBuildFile.aspx?file={0}", this.HtmlFileName);
            return this.viewGenerator.GenerateView("HtmlReport.vm", velocityContext);
        }
        #endregion

        #region GetFingerprint()
        /// <summary>
        /// Generate a fingerprint for this action.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ConditionalGetFingerprint GetFingerprint(IRequest request)
	    {
            return this.fingerprintFactory.BuildFromFileNames(this.HtmlFileName);
	    }
        #endregion
        #endregion
    }
}
