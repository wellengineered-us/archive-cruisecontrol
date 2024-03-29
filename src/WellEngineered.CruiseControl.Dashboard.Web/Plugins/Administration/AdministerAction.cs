using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.Configuration;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.MVC.View;
using WellEngineered.CruiseControl.WebDashboard.Resources;

using Microsoft.AspNetCore.Http;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.Administration
{
    /// <summary>
    /// Allows a user to administer the dashboard.
    /// </summary>
    public class AdministerAction
        : ICruiseAction
    {
        #region Public constants
        /// <summary>
        /// The name of the action.
        /// </summary>
        public const string ActionName = "AdministerDashboard";
        #endregion

        #region Private fields
        private readonly IVelocityViewGenerator viewGenerator;
        private readonly PackageManager manager;
        private IRemoteServicesConfiguration servicesConfiguration;
        private readonly IPhysicalApplicationPathProvider physicalApplicationPathProvider;
        private string password;
        private Translations translations;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new <see cref="AdministerAction"/>.
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="viewGenerator"></param>
        /// <param name="physicalApplicationPathProvider"></param>
        /// <param name="servicesConfiguration"></param>
        public AdministerAction(PackageManager manager,
            IVelocityViewGenerator viewGenerator,
            IRemoteServicesConfiguration servicesConfiguration,
            IPhysicalApplicationPathProvider physicalApplicationPathProvider)
        {
            this.manager = manager;
            this.viewGenerator = viewGenerator;
            this.servicesConfiguration = servicesConfiguration;
            this.physicalApplicationPathProvider = physicalApplicationPathProvider;
        }
        #endregion

        #region Public properties
        #region Password
        /// <summary>
        /// The password to lock down the administration plugin.
        /// </summary>
        public string Password
        {
            get { return this.password; }
            set { this.password = value; }
        }
        #endregion

        #endregion

        #region Public methods
        #region Execute()
        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="cruiseRequest"></param>
        /// <returns></returns>
        public IResponse Execute(ICruiseRequest cruiseRequest)
        {
            this.translations = Translations.RetrieveCurrent();
            Hashtable velocityContext = new Hashtable();
            velocityContext["Error"] = string.Empty;
            if (this.ValidateSession(velocityContext))
            {
                // Initialise the default values
                velocityContext["Result"] = string.Empty;
                velocityContext["InstallPackage"] = string.Empty;

                // Retrieve the form values
                string action = cruiseRequest.Request.GetText("Action") ?? string.Empty;
                string type = cruiseRequest.Request.GetText("Type");
                string name = cruiseRequest.Request.GetText("Name");

                // Check to see if there is an action to perform
                if (action == this.translations.Translate("Reload dashboard"))
                {
                    CachingDashboardConfigurationLoader.ClearCache();
                    velocityContext["Result"] = this.translations.Translate("The dashboard configuration has been reloaded");
                }
                else if (action == string.Empty)
                {
                    // Do nothing, the user hasn't asked for anything but this is needed so the 
                    // default doesn't display an error
                }
                else if (action == this.translations.Translate("Save"))
                {
                    this.SaveServer(cruiseRequest.Request, velocityContext);
                }
                else if (action == this.translations.Translate("Delete"))
                {
                    this.DeleteServer(cruiseRequest.Request, velocityContext);
                }
                else if (action == this.translations.Translate("Import"))
                {
                    this.ImportPackage(__Fixup.GetCurrentHttpContext(), velocityContext);
                }
                else if (action == this.translations.Translate("Install"))
                {
                    this.InstallPackage(cruiseRequest, velocityContext);
                }
                else if (action == this.translations.Translate("Uninstall"))
                {
                    this.UninstallPackage(cruiseRequest, velocityContext);
                }
                else if (action == this.translations.Translate("Remove"))
                {
                    this.RemovePackage(cruiseRequest, velocityContext);
                }
                else if (action == this.translations.Translate("Logout"))
                {
                    this.Logout();
                }
                else
                {
                    velocityContext["Error"] = this.translations.Translate("Unknown action '{0}'", action);
                }

                // Retrieve the services and packages
                velocityContext["Servers"] = this.servicesConfiguration.Servers;
                List<PackageManifest> packages = this.manager.ListPackages();
                packages.Sort();
                velocityContext["BuildPackages"] = packages.Where(p => p.Group == PackageManager.PackageGroup.Build.ToString());
                velocityContext["DashboardPackages"] = packages.Where(p => p.Group == PackageManager.PackageGroup.Dashboard.ToString());
                velocityContext["ProjectPackages"] = packages.Where(p => p.Group == PackageManager.PackageGroup.Project.ToString());
                velocityContext["ServerPackages"] = packages.Where(p => p.Group == PackageManager.PackageGroup.Server.ToString());


                // Generate the view
                if (action == "Logout")
                {
                    return this.viewGenerator.GenerateView("AdminLogin.vm", velocityContext);
                }
                else
                {
                    return this.viewGenerator.GenerateView("AdministerDashboard.vm", velocityContext);
                }
            }
            else
            {
                // Generate the view
                return this.viewGenerator.GenerateView("AdminLogin.vm", velocityContext);
            }
        }
        #endregion
        #endregion

        #region Private methods
        #region SaveServer()
        /// <summary>
        /// Save the server details
        /// </summary>
        /// <param name="request"></param>
        /// <param name="velocityContext"></param>
        private void SaveServer(IRequest request, Hashtable velocityContext)
        {
            // Retrieve the details
            string newName = request.GetText("newName");
            string oldName = request.GetText("oldName");
            string serverUri = request.GetText("serverUri");
            bool serverForceBuild = request.GetChecked("serverForceBuild");
            bool serverStartStop = request.GetChecked("serverStartStop");
            bool backwardsCompatible = request.GetChecked("serverBackwardsCompatible");

            // Validate the details
            if (string.IsNullOrEmpty(newName))
            {
                velocityContext["Error"] = this.translations.Translate("Name is a compulsory value");
                return;
            }
            if (string.IsNullOrEmpty(serverUri))
            {
                velocityContext["Error"] = this.translations.Translate("URI is a compulsory value");
                return;
            }

            // Find the server
            XmlDocument configFile = this.LoadConfig();
            XmlElement serverEl = configFile.SelectSingleNode(
                string.Format(
                    "/dashboard/remoteServices/servers/server[@name='{0}']",
                    oldName)) as XmlElement;
            ServerLocation location = null;
            if (serverEl == null)
            {
                // If the element wasn't found, start a new one
                serverEl = configFile.CreateElement("server");
                configFile.SelectSingleNode("/dashboard/remoteServices/servers")
                    .AppendChild(serverEl);

                // Add the new server
                location = new ServerLocation();
                ServerLocation[] locations = new ServerLocation[this.servicesConfiguration.Servers.Length + 1];
                this.servicesConfiguration.Servers.CopyTo(locations, 0);
                locations[this.servicesConfiguration.Servers.Length] = location;
                this.servicesConfiguration.Servers = locations;
            }
            else
            {
                // Get the existing server config
                foreach (ServerLocation locationToCheck in this.servicesConfiguration.Servers)
                {
                    if (locationToCheck.Name == oldName)
                    {
                        location = locationToCheck;
                        break;
                    }
                }
            }

            // Set all the properties
            serverEl.SetAttribute("name", newName);
            serverEl.SetAttribute("url", serverUri);
            serverEl.SetAttribute("allowForceBuild", serverForceBuild ? "true" : "false");
            serverEl.SetAttribute("allowStartStopBuild", serverStartStop ? "true" : "false");
            serverEl.SetAttribute("backwardsCompatible", backwardsCompatible ? "true" : "false");
            this.SaveConfig(configFile);
            if (location != null)
            {
                location.Name = newName;
                location.Url = serverUri;
                location.AllowForceBuild = serverForceBuild;
                location.AllowStartStopBuild = serverStartStop;
                location.BackwardCompatible = backwardsCompatible;
            }

            velocityContext["Result"] = this.translations.Translate("Server has been saved");
            CachingDashboardConfigurationLoader.ClearCache();
        }
        #endregion

        #region DeleteServer()
        /// <summary>
        /// Deletes a server.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="velocityContext"></param>
        private void DeleteServer(IRequest request, Hashtable velocityContext)
        {
            // Retrieve the details
            string serverName = request.GetText("ServerName");

            // Validate the details
            if (string.IsNullOrEmpty(serverName))
            {
                velocityContext["Error"] = this.translations.Translate("Server name has not been set");
                return;
            }

            // Find the server
            XmlDocument configFile = this.LoadConfig();
            XmlElement serverEl = configFile.SelectSingleNode(
                string.Format(
                    "/dashboard/remoteServices/servers/server[@name='{0}']",
                    serverName)) as XmlElement;
            ServerLocation location = null;
            if (serverEl != null)
            {
                // Get and remove the existing server config
                foreach (ServerLocation locationToCheck in this.servicesConfiguration.Servers)
                {
                    if (locationToCheck.Name == serverName)
                    {
                        location = locationToCheck;
                        break;
                    }
                }
                if (location != null)
                {
                    List<ServerLocation> locations = new List<ServerLocation>(this.servicesConfiguration.Servers);
                    locations.Remove(location);
                    this.servicesConfiguration.Servers = locations.ToArray();
                }

                // Remove it from the config file
                serverEl.ParentNode.RemoveChild(serverEl);
                this.SaveConfig(configFile);

                velocityContext["Result"] = this.translations.Translate("Server has been deleted");
                CachingDashboardConfigurationLoader.ClearCache();
            }
            else
            {
                velocityContext["Error"] = this.translations.Translate("Unable to find server");
                return;
            }
        }
        #endregion

        #region LoadConfig()
        /// <summary>
        /// Load the config file.
        /// </summary>
        /// <returns></returns>
        private XmlDocument LoadConfig()
        {
            string configPath = DashboardConfigurationLoader.CalculateDashboardConfigPath();
            XmlDocument document = new XmlDocument();
            document.Load(configPath);
            return document;
        }
        #endregion

        #region SaveConfig()
        /// <summary>
        /// Save the config file.
        /// </summary>
        /// <param name="configFile"></param>
        private void SaveConfig(XmlDocument configFile)
        {
            string configPath = DashboardConfigurationLoader.CalculateDashboardConfigPath();
            configFile.Save(configPath);
        }
        #endregion

        #region ImportPackage()
        /// <summary>
        /// Imports a package.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="velocityContext"></param>
        private void ImportPackage(HttpContext context, Hashtable velocityContext)
        {
	        IFormFile file = context.Request.Form.Files["package"];
            if (file.Length == 0)
            {
                velocityContext["Error"] = this.translations.Translate("No file selected to import!");
            }
            else
            {
                PackageManifest manifest = this.manager.StorePackage(file.FileName, file.OpenReadStream());
                if (manifest == null)
                {
                    // If there is no manifest, then the package is invalid
                    velocityContext["Error"] = this.translations.Translate("Invalid package - manifest file is missing");
                }
                else
                {
                    // Otherwise pass on the details to the view generator
                    velocityContext["Result"] = this.translations.Translate("Package '{0}' has been loaded",
                        manifest.Name);
                    velocityContext["InstallPackage"] = manifest.FileName;
                }
            }
        }
        #endregion

        #region InstallPackage()
        /// <summary>
        /// Installs a package.
        /// </summary>
        /// <param name="cruiseRequest"></param>
        /// <param name="velocityContext"></param>
        private void InstallPackage(ICruiseRequest cruiseRequest, Hashtable velocityContext)
        {
            // Get the manager to install the package
            List<PackageImportEventArgs> events = this.manager.InstallPackage(
                cruiseRequest.Request.GetText("PackageName"));

            if (events != null)
            {
                // Pass on the details
                velocityContext["Result"] = this.translations.Translate("Package has been installed");
                velocityContext["Install"] = true;
                velocityContext["Events"] = events;

                // Need to reset the cache, otherwise the configuration will not be loaded
                CachingDashboardConfigurationLoader.ClearCache();
            }
            else
            {
                velocityContext["Error"] = this.translations.Translate("Package has been removed");
            }
        }
        #endregion

        #region UninstallPackage()
        /// <summary>
        /// Uninstalls a package.
        /// </summary>
        /// <param name="cruiseRequest"></param>
        /// <param name="velocityContext"></param>
        private void UninstallPackage(ICruiseRequest cruiseRequest, Hashtable velocityContext)
        {
            // Get the manager to install the package
            List<PackageImportEventArgs> events = this.manager.UninstallPackage(
                cruiseRequest.Request.GetText("PackageName"));

            if (events != null)
            {
                // Pass on the details
                velocityContext["Result"] = this.translations.Translate("Package has been uninstalled");
                velocityContext["Install"] = true;
                velocityContext["Events"] = events;

                // Need to reset the cache, otherwise the configuration will not be loaded
                CachingDashboardConfigurationLoader.ClearCache();
            }
            else
            {
                velocityContext["Error"] = this.translations.Translate("Unable to uninstall package");
            }
        }
        #endregion

        #region RemovePackage()
        /// <summary>
        /// Removes a package.
        /// </summary>
        /// <param name="cruiseRequest"></param>
        /// <param name="velocityContext"></param>
        private void RemovePackage(ICruiseRequest cruiseRequest, Hashtable velocityContext)
        {
            // Get the manager to install the package
            string name = this.manager.RemovePackage(cruiseRequest.Request.GetText("PackageName"));

            // Pass on the details
            if (!string.IsNullOrEmpty(name))
            {
                velocityContext["Result"] = string.Format(System.Globalization.CultureInfo.CurrentCulture,"Package '{0}' has been removed",
                            name);
            }
            else
            {
                velocityContext["Error"] = this.translations.Translate("Unable to remove package - has the package already been removed?");
            }
        }
        #endregion

        #region ValidateSession()
        /// <summary>
        /// Validates a session.
        /// </summary>
        /// <param name="velocityContext"></param>
        /// <returns></returns>
        private bool ValidateSession(Hashtable velocityContext)
        {
            bool isValid = false;

            // See if the password needs to be checked
            if (string.IsNullOrEmpty(this.password))
            {
                // password may not be empty
                velocityContext["Error"] = this.translations.Translate("Administration password may not be empty. Update dashboard.config, section administrationPlugin.");
                isValid = false;
            }
            else
            {
                // First see if there is a session token
                HttpContext context = __Fixup.GetCurrentHttpContext();
                string ticket = this.RetrieveSessionCookie(context);
                if (ticket == "webAdmin")
                {
                    // If there is a session token, make sure it is valid
                    isValid = true;
                }
                else
                {
                    // Otherwise, attempt to retrieve a password and validate it
                    string userPassword = context.Request.Form["password"];
                    if (string.Equals(this.password, userPassword))
                    {
                        isValid = true;

                        this.AddSessionCookie(context);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(userPassword))
                        {
                            velocityContext["Error"] = this.translations.Translate("Invalid password");
                        }
                    }
                }
            }

            return isValid;
        }
        #endregion

        #region RetrieveSessionCookie()
        /// <summary>
        /// Retrieves a session cookie.
        /// </summary>
        /// <param name="context"></param>
        private string RetrieveSessionCookie(HttpContext context)
        {
            var cookie = context.Request.GetCookies()["CCNetDashboard"];
            if ((cookie != null) && !string.IsNullOrEmpty(cookie.Value))
            {
                return cookie.Value;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region AddSessionCookie()
        /// <summary>
        /// Adds the session cookie.
        /// </summary>
        /// <param name="context"></param>
        private void AddSessionCookie(HttpContext context)
        {
            string cookieData = "webAdmin";
	        __Fixup.HttpCookie cookie = new __Fixup.HttpCookie("CCNetDashboard", cookieData);
            cookie.HttpOnly = true;
            cookie.Expires = DateTime.Now.AddMinutes(30);
            context.Response.GetCookies().Add(cookie);
        }
        #endregion

        #region Logout()
        /// <summary>
        /// Logs out a session
        /// </summary>
        private void Logout()
        {
            if (__Fixup.GetCurrentHttpContext().Request.Cookies["CCNetDashboard"] != null)
            {
				// Cannot directly delete a cookie - instead, set it to expired
	            __Fixup.HttpCookie cookie = new __Fixup.HttpCookie("CCNetDashboard", string.Empty);
                cookie.HttpOnly = true;
                cookie.Expires = DateTime.Now.AddDays(-1);
                __Fixup.GetCurrentHttpContext().Response.GetCookies().Add(cookie);
            }
        }
        #endregion
        #endregion
    }
}
