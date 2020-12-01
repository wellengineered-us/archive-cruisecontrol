using System.Collections.Generic;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard.GenericPlugins
{
    public abstract class ProjectConfigurableBuildPlugin : IBuildPlugin
	{
        private List<string> includedProjects = new List<string>();
        private List<string> excludedProjects = new List<string>();

		public bool IsDisplayedForProject(IProjectSpecifier project)
		{
			string projectName = project.ProjectName;

			if (this.includedProjects.Count > 0)
			{
				return this.includedProjects.Contains(project.ProjectName);
			}
			else if (this.excludedProjects.Count > 0)
			{
				return !this.excludedProjects.Contains(projectName);
			}
			else
			{
				return true;
			}
		}

        /// <summary>
        /// The projects to include in this plug-in.
        /// </summary>
        /// <version>1.0</version>
        /// <default>None</default>
        /// <remarks>
        /// This is currently not implemented.
        /// </remarks>
        [ReflectorProperty("includedProjects", Required = false)]
		public string[] IncludedProjects
		{
			get { return this.includedProjects.ToArray(); }
			set
			{
				this.CheckOtherPropertyNotAlreadySet(value, this.excludedProjects);
                this.includedProjects = new List<string>(value);
			}
		}

        /// <summary>
        /// The projects to exclude from this plug-in.
        /// </summary>
        /// <version>1.0</version>
        /// <default>None</default>
        /// <remarks>
        /// This is currently not implemented.
        /// </remarks>
        [ReflectorProperty("excludedProjects", Required = false)]
		public string[] ExcludedProjects
		{
			get { return this.excludedProjects.ToArray(); }
			set
			{
				this.CheckOtherPropertyNotAlreadySet(value, this.includedProjects);
                this.excludedProjects = new List<string>(value);
			}
		}

        private void CheckOtherPropertyNotAlreadySet(string[] newList, List<string> otherList)
		{
			if (otherList.Count > 0 && newList.Length > 0)
			{
				throw new CruiseControlException("Invalid configuration - cannot set both Included and Excluded Projects for a Build Plugin");
			}
		}

		public abstract INamedAction[] NamedActions { get; }
		public abstract string LinkDescription { get; }
	}
}
