using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.DeleteProject
{
	public class DeleteProjectModel
	{
		private readonly IProjectSpecifier projectSpecifier;
		private readonly bool purgeWorkingDirectory;
		private readonly bool purgeArtifactDirectory;
		private readonly bool purgeSourceControlEnvironment;
		private readonly bool allowDelete;
		private readonly string message;

		public DeleteProjectModel(IProjectSpecifier projectSpecifier, string message, bool allowDelete,
			bool purgeWorkingDirectory, bool purgeArtifactDirectory, bool purgeSourceControlEnvironment)
		{
			this.message = message;
			this.allowDelete = allowDelete;
			this.purgeSourceControlEnvironment = purgeSourceControlEnvironment;
			this.purgeArtifactDirectory = purgeArtifactDirectory;
			this.purgeWorkingDirectory = purgeWorkingDirectory;
			this.projectSpecifier = projectSpecifier;
		}

		public bool AllowDelete
		{
			get { return this.allowDelete; }
		}

		public string Message
		{
			get { return this.message; }
		}

		public string ProjectName
		{
			get { return this.projectSpecifier.ProjectName; }
		}

		public string ServerName
		{
			get { return this.projectSpecifier.ServerSpecifier.ServerName; }
		}

		public bool PurgeWorkingDirectory
		{
			get { return this.purgeWorkingDirectory; }
		}

		public bool PurgeArtifactDirectory
		{
			get { return this.purgeArtifactDirectory; }
		}

		public bool PurgeSourceControlEnvironment
		{
			get { return this.purgeSourceControlEnvironment; }
		}

		public override bool Equals(object obj)
		{
			if (obj != null && obj is DeleteProjectModel)
			{
				DeleteProjectModel other = (DeleteProjectModel) obj;
				return (
					this.ServerName == other.ServerName
					&& this.ProjectName == other.ProjectName
					&& this.Message == other.Message
					&& this.AllowDelete == other.AllowDelete
					&& this.PurgeArtifactDirectory == other.PurgeArtifactDirectory
					&& this.PurgeWorkingDirectory == other.PurgeWorkingDirectory
					&& this.PurgeSourceControlEnvironment == other.PurgeSourceControlEnvironment
				);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return this.ServerName.GetHashCode() + this.ProjectName.GetHashCode() + this.Message.GetHashCode() + this.AllowDelete.GetHashCode()
				+ this.PurgeArtifactDirectory.GetHashCode() + this.PurgeWorkingDirectory.GetHashCode() + this.PurgeSourceControlEnvironment.GetHashCode();
		}
	}
}
