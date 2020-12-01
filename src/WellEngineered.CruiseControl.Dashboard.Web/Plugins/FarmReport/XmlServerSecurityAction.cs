using System.IO;
using System.Xml.Serialization;

using WellEngineered.CruiseControl.Remote.Messages;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.ServerConnection;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.FarmReport
{
    public class XmlServerSecurityAction : IAction
    {
        public const string ACTION_NAME = "XmlSecurity";

        private readonly IFarmService farmService;

        public XmlServerSecurityAction(IFarmService farmService)
        {
            this.farmService = farmService;
        }

        public IResponse Execute(IRequest request)
        {
            string action = request.GetText("action");
            string result = null;

            switch (action.ToLower())
            {
                case "login":
                    result = this.PerformLogin(request);
                    break;
                case "logout":
                    result = this.PerformLogout(request);
                    break;
                default:
                    result = this.GenerateResult("failure", "Unknown action");
                    break;
            }
            return new XmlFragmentResponse(result);
        }

        private string PerformLogin(IRequest request)
        {
            try
            {
                LoginRequest credentials = this.Deserialise(request.GetText("credentials"));
                string sessionToken = this.farmService.Login(request.GetText("server"), credentials);
                return this.GenerateResult("success", string.Format(System.Globalization.CultureInfo.CurrentCulture,"<session>{0}</session>", sessionToken));
            }
            catch
            {
                return this.GenerateResult("failure", "Login failure");
            }
        }

        private LoginRequest Deserialise(string data)
        {
            XmlSerializer serialiser = new XmlSerializer(typeof(LoginRequest));
            StringReader reader = new StringReader(data);
            LoginRequest request = serialiser.Deserialize(reader) as LoginRequest;
            return request;
        }

        private string PerformLogout(IRequest request)
        {
            try
            {
                string sessionToken = request.GetText("sessionToken");
                this.farmService.Logout(request.GetText("server"), sessionToken);
                return this.GenerateResult("success", string.Empty);
            }
            catch
            {
                return this.GenerateResult("failure", "Login failure");
            }
        }

        private string GenerateResult(string outcome, string contents)
        {
            string result = string.Format(System.Globalization.CultureInfo.CurrentCulture,"<security result=\"{0}\">{1}</security>", outcome, contents);
            return result;
        }
    }
}
