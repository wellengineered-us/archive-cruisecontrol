using System;
using System.Reflection;
using System.Text;

using WellEngineered.CruiseControl.PrivateBuild.EdtFtpNet.Util.Deebug;

namespace WellEngineered.CruiseControl.PrivateBuild.EdtFtpNet.Util
{
    public class VersionDetails
    {
        private static Logger log = Logger.GetLogger("VersionDetails");

        internal static string GetVersionDetails(Type t)
        {
            StringBuilder str = new StringBuilder();
            try
            {
                str.Append("OS: ").Append(Environment.OSVersion.Version.ToString());
                str.Append(", CLR: ").Append(Environment.Version.ToString());
                string dllVersion = Assembly.GetAssembly(t).GetName().Version.ToString();
                str.Append(", DLL: ").Append(dllVersion);
            }
            catch (Exception ex) {
                log.Warn("Failed to obtain version details", ex);
            }
            return str.ToString();
        }

    }
 
}
