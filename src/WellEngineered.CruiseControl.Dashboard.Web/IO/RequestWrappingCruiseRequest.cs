using System.Web;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.MVC;

namespace WellEngineered.CruiseControl.WebDashboard.IO
{
	public class RequestWrappingCruiseRequest : ICruiseRequest
	{
		private readonly IRequest request;
        private readonly ICruiseUrlBuilder urlBuilder;
        private readonly ISessionRetriever sessionRetriever;

        public RequestWrappingCruiseRequest(IRequest request, 
            ICruiseUrlBuilder urlBuilder,
            ISessionRetriever sessionRetriever)
		{
			this.request = request;
            this.urlBuilder = urlBuilder;
            this.sessionRetriever = sessionRetriever;
		}

        public ICruiseUrlBuilder UrlBuilder
        {
            get { return this.urlBuilder; }
		}

		public string ServerName
		{
			get { return this.FindRESTSpecifiedResource(DefaultCruiseUrlBuilder.ServerRESTSpecifier); }
		}

		public string ProjectName
		{
			get { return this.FindRESTSpecifiedResource(DefaultCruiseUrlBuilder.ProjectRESTSpecifier); }
		}

		public string BuildName
		{
			get { return this.FindRESTSpecifiedResource(DefaultCruiseUrlBuilder.BuildRESTSpecifier); }
		}

		private string FindRESTSpecifiedResource(string specifier)
		{
			string[] subFolders = this.request.SubFolders;

			for (int i = 0; i < subFolders.Length; i += 2)
			{
				if (subFolders[i] == specifier)
				{
					if (i < subFolders.Length)
					{
						return HttpUtility.UrlDecode(subFolders[i + 1]);
					}
					else
					{
						throw new CruiseControlException(
							string.Format(System.Globalization.CultureInfo.CurrentCulture,"unexpected URL format - found {0} REST Specifier, but no following value", specifier));
					}
				}
			}

			return string.Empty;
		}

		public IServerSpecifier ServerSpecifier
		{
			get { return new DefaultServerSpecifier(this.ServerName); }
		}

		public IProjectSpecifier ProjectSpecifier
		{
			get { return new DefaultProjectSpecifier(this.ServerSpecifier, this.ProjectName); }
		}

		public IBuildSpecifier BuildSpecifier
		{
			get { return new DefaultBuildSpecifier(this.ProjectSpecifier, this.BuildName); }
		}

		public IRequest Request
		{
			get { return this.request; }
		}

        
        /// <summary>
        /// Attempt to retrieve a session token
        /// </summary>
        /// <returns></returns>
        public virtual string RetrieveSessionToken()
        {
            return this.RetrieveSessionToken(this.sessionRetriever);
        }

        /// <summary>
        /// Attempt to retrieve a session token
        /// </summary>
        /// <returns></returns>
        public virtual string RetrieveSessionToken(ISessionRetriever sessionRetriever)
        {
            // Attempt to find a session token
            string sessionToken = this.request.GetText("sessionToken");
            if (string.IsNullOrEmpty(sessionToken) && (sessionRetriever != null))
            {
                sessionToken = sessionRetriever.RetrieveSessionToken(this.request);
	}
            return sessionToken;
        }
	}
}
