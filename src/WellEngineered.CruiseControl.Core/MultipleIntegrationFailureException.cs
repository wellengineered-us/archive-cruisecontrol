using System;
using System.Collections.Generic;

namespace WellEngineered.CruiseControl.Core
{
    /// <summary>
    /// An integration has had multiple failures.
    /// </summary>
    public class MultipleIntegrationFailureException
        : Exception
    {
        #region Private fields
        private List<Exception> failures = new List<Exception>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initialise a blank exception.
        /// </summary>
        public MultipleIntegrationFailureException(Exception initialFailure)
            : base("There has been multiple integration failures")
        {
            if (initialFailure == null) throw new ArgumentNullException("initialFailure");
            this.failures.Add(initialFailure);
        }
        #endregion

        #region Public properties
        #region Failures
        /// <summary>
        /// The failure exceptions.
        /// </summary>
        public Exception[] Failures
        {
            get { return this.failures.ToArray(); }
        }
        #endregion
        #endregion

        #region Public method
        #region AddFailure()
        /// <summary>
        /// Adds another failure to the list.
        /// </summary>
        /// <param name="failure">The failure to add.</param>
        public void AddFailure(Exception failure)
        {
            if (failure == null) throw new ArgumentNullException("failure");
            this.failures.Add(failure);
        }
        #endregion
        #endregion
    }
}
