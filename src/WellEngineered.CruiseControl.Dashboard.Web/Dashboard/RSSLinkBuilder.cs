using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
    class RSSLinkBuilder
    {

        private RSSLinkBuilder()
        {}

        public static GeneralAbsoluteLink CreateRSSLink(ILinkFactory linkFactory, IProjectSpecifier projectSpecifier)
        {
            string MachineName;

            if (__Fixup.GetCurrentHttpContext() == null ) 
            {
                MachineName = "localhost";
            }
            else
            {
                MachineName = __Fixup.GetCurrentHttpContext().Request.Host.Host;
                if (__Fixup.GetCurrentHttpContext().Request.Host.Port != 80)
                    MachineName = MachineName + ":" + __Fixup.GetCurrentHttpContext().Request.Host.Port;
            }

            return  new GeneralAbsoluteLink("RSS",string.Format(System.Globalization.CultureInfo.CurrentCulture,"http://{0}{1}",
                         MachineName,  
                         linkFactory.CreateProjectLink(projectSpecifier, WebDashboard.Plugins.RSS.RSSFeed.ACTION_NAME).Url));
       }

    }
}
