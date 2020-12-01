﻿

using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.Tasks.Conditions
{
	/// <summary>
    /// Provides a base implementation for conditions that provides some common functionality.
    /// </summary>
    public abstract class ConditionBase
        : ITaskCondition
    {
        #region Public properties
        #region Description
        /// <summary>
        /// A description of the condition.
        /// </summary>
        /// <version>1.6</version>
        /// <default>none</default>
        [ReflectorProperty("description", Required = false)]
        public string Description { get; set; }
        #endregion

        #region Logger
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public ILogger Logger { get; set; }
        #endregion
        #endregion

        #region Public methods
        #region Eval()
        /// <summary>
        /// Evals the specified result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>
        /// <c>true</c> if the condition is true; <c>false</c> otherwise.
        /// </returns>
        public virtual bool Eval(IIntegrationResult result)
        {
            var evaluation = this.Evaluate(result);
            return evaluation;
        }
        #endregion
        #endregion

        #region Protected methods
        #region Evaluate()
        /// <summary>
        /// Performs the actual evaluation.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>
        /// <c>true</c> if the condition is true; <c>false</c> otherwise.
        /// </returns>
        protected abstract bool Evaluate(IIntegrationResult result);
        #endregion

        #region RetrieveLogger()
        /// <summary>
        /// Retrieves the logger instance.
        /// </summary>
        /// <returns>The <see cref="ILogger"/> to use.</returns>
        protected ILogger RetrieveLogger()
        {
            return this.Logger ?? (this.Logger = new DefaultLogger());
        }

        #endregion

        #region LogDescriptionOrMessage()
        /// <summary>
        /// Logs the description or a message.
        /// </summary>
        /// <param name="message">The message to use if there is no description.</param>
        protected void LogDescriptionOrMessage(string message)
        {
            var logger = this.RetrieveLogger();
            if (string.IsNullOrEmpty(this.Description))
            {
                logger.Debug(message);
            }
            else
            {
                logger.Info(this.Description);
            }
        }
        #endregion
        #endregion
    }
}
