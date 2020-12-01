using System.ComponentModel;

namespace WellEngineered.CruiseControl.Core.State
{
	/// <summary>
	/// Manages the state of continuous integration for a single project.
	/// State must be persisted between shutdown/startup of the CruiseControl.NET
	/// server, as modification dates and label numbers must follow sequence.
	/// </summary>
    /// <title>State Manager Blocks</title>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public interface IStateManager
	{
		/// <summary>
		/// Loads the state of the project.
		/// </summary>
		/// <returns></returns>
		IIntegrationResult LoadState(string project);

		/// <summary>
		/// Persists the state of the project.
		/// </summary>
		/// <param name="result"></param>
		void SaveState(IIntegrationResult result);

        /// <summary>
        /// Determines whether [has previous state] [the specified project].	
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		bool HasPreviousState(string project);
	}
}
