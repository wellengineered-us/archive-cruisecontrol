using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core
{
	/// <summary>
    /// Defines a mechanism for storing data from a project.
    /// </summary>
    public interface IDataStore
    {
        #region Public methods
        #region StoreProjectSnapshot()
        /// <summary>
        /// Stores a snapshot of a project build.
        /// </summary>
        /// <param name="result">The result that the snapshot is for.</param>
        /// <param name="snapshot">The project snapshot.</param>
        void StoreProjectSnapshot(IIntegrationResult result, ItemStatus snapshot);
        #endregion

        #region LoadProjectSnapshot()
        /// <summary>
        /// Loads the project snapshot for a build.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="buildName">Name of the build.</param>
        /// <returns>The project snapshot.</returns>
        ItemStatus LoadProjectSnapshot(IProject project, string buildName);
        #endregion
        #endregion
    }
}
