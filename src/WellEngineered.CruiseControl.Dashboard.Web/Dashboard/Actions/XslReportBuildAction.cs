using System;
using System.Collections;
using System.Collections.Generic;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard.Actions
{
	[ReflectorType("xslReportBuildAction")]
	public class XslReportBuildAction : ICruiseAction, IConditionalGetFingerprintProvider
	{
		private readonly IBuildLogTransformer buildLogTransformer;
	    private readonly IFingerprintFactory fingerprintFactory;
	    private string xslFileName;

        public XslReportBuildAction(IBuildLogTransformer buildLogTransformer, IFingerprintFactory fingerprintFactory)
        {
            this.buildLogTransformer = buildLogTransformer;
            this.fingerprintFactory = fingerprintFactory;
        }

	    public IResponse Execute(ICruiseRequest cruiseRequest)
		{
			if (this.xslFileName == null)
			{
				throw new ApplicationException("XSL File Name has not been set for XSL Report Action");
			}
			Hashtable xsltArgs = new Hashtable();
            if (cruiseRequest.Request.ApplicationPath == "/")
            {
                xsltArgs["applicationPath"] = string.Empty;
            }
            else
            {
                xsltArgs["applicationPath"] = cruiseRequest.Request.ApplicationPath;
            }

            // Add the input parameters
            if (this.Parameters != null)
            {
                foreach (var parameter in this.Parameters)
                {
                    xsltArgs.Add(parameter.Name, parameter.Value);
                }
            }

			return new HtmlFragmentResponse(this.buildLogTransformer.Transform(cruiseRequest.BuildSpecifier, new string[] {this.xslFileName}, xsltArgs, cruiseRequest.RetrieveSessionToken()));
		}

        /// <summary>
        /// Optional parameters to pass into the XSL-T.
        /// </summary>
        public List<XsltParameter> Parameters { get; set; }

		[ReflectorProperty("xslFileName")]
		public string XslFileName
		{
			get
			{
				return this.xslFileName;
			}
			set
			{
				this.xslFileName = value;
			}
		}

	    public ConditionalGetFingerprint GetFingerprint(IRequest request)
	    {
            return this.fingerprintFactory.BuildFromFileNames(this.XslFileName);
	    }
	}
}
