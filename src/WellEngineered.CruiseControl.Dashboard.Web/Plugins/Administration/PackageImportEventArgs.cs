using System;
using System.Diagnostics;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.Administration
{
    /// <summary>
    /// The arguments for an import event while installing a package.
    /// </summary>
    public class PackageImportEventArgs
        : EventArgs
    {
        #region Private fields
        private readonly TraceLevel level;
        private readonly string message;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialises a new <see cref="PackageImportEventArgs"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        public PackageImportEventArgs(string message, TraceLevel level)
        {
            this.message = message;
            this.level = level;
        }
        #endregion

        #region Public proeprties
        #region Message
        /// <summary>
        /// The import message.
        /// </summary>
        public string Message
        {
            get { return this.message; }
        }
        #endregion

        #region Level
        /// <summary>
        /// The trace level of the message.
        /// </summary>
        public TraceLevel Level
        {
            get { return this.level; }
        }
        #endregion
        #endregion
    }
}
