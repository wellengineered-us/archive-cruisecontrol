

using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.Tasks
{
	///<title>Comment Task</title>
    ///<version>1.6</version>
    /// <summary>
    /// Adds a comment to the log.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// <commentTask>
    /// <message>Hello World!</message>
    /// </commentTask>
    /// ]]>
    /// </code>
    /// </example>
    [ReflectorType("commentTask")]
    public class CommentTask
        : TaskBase
    {
        #region Public properties
        #region Message
        /// <summary>
        /// The message to add to the log.
        /// </summary>
        /// <version>1.6</version>
        /// <default>n/a</default>
        [ReflectorProperty("message", Required = true)]
        public string Message { get; set; }
        #endregion

        #region FailTask
        /// <summary>
        /// Defines whether to fail the task or not.
        /// </summary>
        /// <version>1.6</version>
        /// <default>false</default>
        [ReflectorProperty("failure", Required = false)]
        public bool FailTask { get; set; }
        #endregion

        #region Logger
        /// <summary>
        /// Gets or sets the logger to use.
        /// </summary>
        /// <value>The logger.</value>
        public ILogger Logger { get; set; }
        #endregion
        #endregion

        #region Protected methods
        #region Execute()
        /// <summary>
        /// Execute the actual task functionality.
        /// </summary>
        /// <param name="result">The result details to use.</param>
        /// <returns>
        /// True if the task was successful, false otherwise.
        /// </returns>
        protected override bool Execute(IIntegrationResult result)
        {
            result.BuildProgressInformation
                .SignalStartRunTask("Adding a comment to the log");
            (this.Logger ?? new DefaultLogger())
                .Debug("Logging " + (this.FailTask ? "error " : string.Empty) + "message: " + this.Message);
            result.AddTaskResult(
                new GeneralTaskResult(!this.FailTask, this.Message));
            return true;
        }
        #endregion
        #endregion
    }
}
