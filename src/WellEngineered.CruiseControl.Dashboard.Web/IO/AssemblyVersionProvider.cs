using System.Reflection;

namespace WellEngineered.CruiseControl.WebDashboard.IO
{
    public class AssemblyVersionProvider : IVersionProvider
    {
        public string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetName().Version.ToString();
        }
    }
}