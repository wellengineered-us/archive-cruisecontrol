using System;

namespace WellEngineered.CruiseControl.Remote.Monitor
{
    /// <summary>
    /// Arguments for a build queue request change event.
    /// </summary>
    public class BuildQueueRequestChangedArgs
        : EventArgs
    {
        #region Constructors
        /// <summary>
        /// Initialise a new <see cref="BuildQueueRequestChangedArgs"/>.
        /// </summary>
        /// <param name="buildQueue">The build queue request that changed.</param>
        public BuildQueueRequestChangedArgs(BuildQueueRequest buildQueue)
        {
            this.BuildQueueRequest = buildQueue;
        }
        #endregion

        #region Public properties
        #region BuildQueueRequest
        /// <summary>
        /// The build queue request that has been changed.
        /// </summary>
        public BuildQueueRequest BuildQueueRequest { get; protected set; }
        #endregion
        #endregion
    }
}
