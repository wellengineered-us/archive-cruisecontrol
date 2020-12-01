using System;
using System.Collections;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.Objection;
using WellEngineered.CruiseControl.WebDashboard.Configuration;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard.ActionDecorators
{
    // ToDo - test - I think doing so will change the design a bit - will probably get more in on
    // the constructor - should do this after 1.0
    public class SiteTemplateActionDecorator : IAction, IConditionalGetFingerprintProvider
    {
        private const string TEMPLATE_NAME = "SiteTemplate.vm";
        private readonly IAction decoratedAction;
        private readonly IVelocityViewGenerator velocityViewGenerator;
        private readonly ObjectSource objectSource;
        private readonly IVersionProvider versionProvider;
        private readonly IFingerprintFactory fingerprintFactory;
        private readonly IUrlBuilder urlBuilder;
        private readonly IPluginConfiguration configuration;
        private TopControlsViewBuilder topControlsViewBuilder;
        private SideBarViewBuilder sideBarViewBuilder;
        private LoginViewBuilder loginViewBuilder;
        private readonly ICruiseRequest cruiseRequest;

        public SiteTemplateActionDecorator(IAction decoratedAction, IVelocityViewGenerator velocityViewGenerator,
                                           ObjectSource objectSource, IVersionProvider versionProvider,
                                           IFingerprintFactory fingerprintFactory, IUrlBuilder urlBuilder,
                                            IPluginConfiguration configuration, ICruiseRequest cruiseRequest)
        {
            this.decoratedAction = decoratedAction;
            this.velocityViewGenerator = velocityViewGenerator;
            this.objectSource = objectSource;
            this.versionProvider = versionProvider;
            this.fingerprintFactory = fingerprintFactory;
            this.urlBuilder = urlBuilder;
            this.configuration = configuration;
            this.cruiseRequest = cruiseRequest;
        }

        private TopControlsViewBuilder TopControlsViewBuilder
        {
            get
            {
                if (this.topControlsViewBuilder == null)
                {
                    this.topControlsViewBuilder =
                        (TopControlsViewBuilder)this.objectSource.GetByType(typeof(TopControlsViewBuilder));
                }
                return this.topControlsViewBuilder;
            }
        }

        private SideBarViewBuilder SideBarViewBuilder
        {
            get
            {
                if (this.sideBarViewBuilder == null)
                {
                    this.sideBarViewBuilder =
                        (SideBarViewBuilder)this.objectSource.GetByType(typeof(SideBarViewBuilder));
                }
                return this.sideBarViewBuilder;
            }
        }

        private LoginViewBuilder LoginViewBuilder
        {
            get
            {
                if (this.loginViewBuilder == null)
                {
                    this.loginViewBuilder =
                        (LoginViewBuilder)this.objectSource.GetByType(typeof(LoginViewBuilder));
                }
                return this.loginViewBuilder;
            }
        }

        public IResponse Execute(IRequest request)
        {
            Hashtable velocityContext = new Hashtable();
            IResponse decoratedActionResponse = this.decoratedAction.Execute(request);
            if (decoratedActionResponse is HtmlFragmentResponse)
            {
                velocityContext["breadcrumbs"] = (this.TopControlsViewBuilder.Execute()).ResponseFragment;
                velocityContext["sidebar"] = (this.SideBarViewBuilder.Execute(this.cruiseRequest)).ResponseFragment;
                velocityContext["mainContent"] = ((HtmlFragmentResponse)decoratedActionResponse).ResponseFragment;
                velocityContext["dashboardversion"] = this.versionProvider.GetVersion();
                if (request.ApplicationPath == "/")
                    velocityContext["applicationPath"] = string.Empty;
                else
                    velocityContext["applicationPath"] = request.ApplicationPath;
                velocityContext["renderedAt"] = DateUtil.FormatDate(DateTime.Now);
                velocityContext["loginView"] = this.LoginViewBuilder.Execute().ResponseFragment;

                // set to no refresh if refresh interval lower than 5 seconds
                Int32 refreshIntervalInSeconds = Int32.MaxValue;
                if (request.RefreshInterval >= 5) refreshIntervalInSeconds = request.RefreshInterval;

                velocityContext["refreshinterval"] = refreshIntervalInSeconds;

                string headerSuffix = string.Empty;
                if (!string.IsNullOrEmpty(this.loginViewBuilder.BuildServerName)) headerSuffix = this.LoginViewBuilder.BuildServerName;
                if (!string.IsNullOrEmpty(this.LoginViewBuilder.ProjectName)) headerSuffix = string.Concat(headerSuffix, " - ", this.loginViewBuilder.ProjectName);

                velocityContext["headersuffix"] = headerSuffix;

                this.GeneratejQueryLinks(velocityContext);

                return this.velocityViewGenerator.GenerateView(TEMPLATE_NAME, velocityContext);
            }
            else
            {
                return decoratedActionResponse;
            }
        }

        public ConditionalGetFingerprint GetFingerprint(IRequest request)
        {
            ConditionalGetFingerprint fingerprint = this.CalculateLocalFingerprint(request);
            return fingerprint.Combine(((IConditionalGetFingerprintProvider)this.decoratedAction).GetFingerprint(request));
        }

        private void GeneratejQueryLinks(Hashtable velocityContext)
        {
            string extension = this.urlBuilder.Extension;
            this.urlBuilder.Extension = "js";
            velocityContext["jquery"] = this.urlBuilder.BuildUrl("javascript/jquery");
            velocityContext["jqueryui"] = this.urlBuilder.BuildUrl("javascript/jquery-ui");
            this.urlBuilder.Extension = extension;
        }

        private ConditionalGetFingerprint CalculateLocalFingerprint(IRequest request)
        {
            return this.fingerprintFactory.BuildFromFileNames(TEMPLATE_NAME)
                .Combine(this.TopControlsViewBuilder.GetFingerprint(request))
                .Combine(this.SideBarViewBuilder.GetFingerprint(request));
        }
    }
}
