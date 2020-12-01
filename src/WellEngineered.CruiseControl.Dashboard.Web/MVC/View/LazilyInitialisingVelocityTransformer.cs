using System;
using System.Collections;
using System.IO;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.App;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.WebDashboard.Configuration;
using WellEngineered.CruiseControl.WebDashboard.Resources;

namespace WellEngineered.CruiseControl.WebDashboard.MVC.View
{
    public class LazilyInitialisingVelocityTransformer : IVelocityTransformer
    {
        private readonly IPhysicalApplicationPathProvider physicalApplicationPathProvider;

        private VelocityEngine lazilyInitialisedEngine = null;
        private VelocityEngine lazilyCustomInitialisedEngine = null;
        private System.Collections.Generic.Dictionary<string, TemplateLocation> FoundTemplates = new System.Collections.Generic.Dictionary<string, TemplateLocation>();
        private IDashboardConfiguration configuration;
        private string customTemplateLocation;

        public enum TemplateLocation
        {
            Templates, CustomTemplates
        }



        public LazilyInitialisingVelocityTransformer(IPhysicalApplicationPathProvider physicalApplicationPathProvider, IDashboardConfiguration configuration)
        {
            this.physicalApplicationPathProvider = physicalApplicationPathProvider;
            this.configuration = configuration;
        }

        public string Transform(string transformerFileName, Hashtable transformable)
        {
            // Add a translator to all views
            var translations = Translations.RetrieveCurrent();
            transformable.Add("translations", translations);

            string output = string.Empty;
            using (TextWriter writer = new StringWriter())
            {
                try
                {

                    if (this.DetermineTemplateLocation(transformerFileName) == TemplateLocation.CustomTemplates )
                    {
                        this.VelocityEngineCustom.MergeTemplate(transformerFileName, RuntimeConstants.ENCODING_DEFAULT, new VelocityContext(transformable), writer);

                    }
                    else
                    {
                        this.VelocityEngine.MergeTemplate(transformerFileName, RuntimeConstants.ENCODING_DEFAULT, new VelocityContext(transformable), writer);
                    }
                }
                catch (Exception baseException)
                {
                    throw new CruiseControlException(string.Format(@"Exception calling NVelocity for template: {0}
Template path is {1}", transformerFileName, this.physicalApplicationPathProvider.GetFullPathFor("templates")), baseException);
                }
                output = writer.ToString();
            }
            return output;
        }

        private VelocityEngine VelocityEngine
        {
            get
            {
                lock (this)
                {
                    if (this.lazilyInitialisedEngine == null)
                    {
                        this.lazilyInitialisedEngine = new VelocityEngine();
                        this.lazilyInitialisedEngine.SetProperty(RuntimeConstants.RUNTIME_LOG_LOGSYSTEM_CLASS, "NVelocity.Runtime.Log.NullLogSystem");
                        this.lazilyInitialisedEngine.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, this.physicalApplicationPathProvider.GetFullPathFor("templates"));
                        this.lazilyInitialisedEngine.SetProperty(RuntimeConstants.RESOURCE_MANAGER_CLASS, "NVelocity.Runtime.Resource.ResourceManagerImpl");
                        this.lazilyInitialisedEngine.Init();
                    }
                }
                return this.lazilyInitialisedEngine;
            }
        }



        private VelocityEngine VelocityEngineCustom
        {
            get
            {
                lock (this)
                {
                    if (this.lazilyCustomInitialisedEngine == null)
                    {
                        this.lazilyCustomInitialisedEngine = new VelocityEngine();
                        this.lazilyCustomInitialisedEngine.SetProperty(RuntimeConstants.RUNTIME_LOG_LOGSYSTEM_CLASS, "NVelocity.Runtime.Log.NullLogSystem");
                        this.lazilyCustomInitialisedEngine.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, this.CustomTemplateLocation);
                        this.lazilyCustomInitialisedEngine.SetProperty(RuntimeConstants.RESOURCE_MANAGER_CLASS, "NVelocity.Runtime.Resource.ResourceManagerImpl");
                        this.lazilyCustomInitialisedEngine.Init();
                    }
                }
                return this.lazilyCustomInitialisedEngine;
            }
        }


        private TemplateLocation DetermineTemplateLocation(string transformerFileName)
        {
            if (!this.FoundTemplates.ContainsKey(transformerFileName))
            {
                string filelocation = System.IO.Path.Combine(this.CustomTemplateLocation,transformerFileName);
                if (System.IO.File.Exists(filelocation))
                {
                    this.FoundTemplates.Add(transformerFileName, TemplateLocation.CustomTemplates);
                }
                else
                {
                    this.FoundTemplates.Add(transformerFileName, TemplateLocation.Templates);
                }
            }

            return this.FoundTemplates[transformerFileName];
        }

        private string CustomTemplateLocation
        {
            get
            {
                if (this.customTemplateLocation == null)
                {
                    this.customTemplateLocation = "customtemplates";
                    if (!string.IsNullOrEmpty(this.configuration.PluginConfiguration.TemplateLocation))
                    {
                        if (Path.IsPathRooted(this.configuration.PluginConfiguration.TemplateLocation))
                        {
                            this.customTemplateLocation = this.configuration.PluginConfiguration.TemplateLocation;
                        }
                        else
                        {
                            this.customTemplateLocation = this.physicalApplicationPathProvider.GetFullPathFor(
                                this.configuration.PluginConfiguration.TemplateLocation);
                        }
                    }
                }
                return this.customTemplateLocation;
            }
        }
    }
}
