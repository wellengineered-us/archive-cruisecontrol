using System;
using System.Collections;

using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core
{
    /// <summary>
    /// 	
    /// </summary>
	public class IntegrationSummary
	{
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public static IntegrationSummary Initial = new IntegrationSummary(IntegrationStatus.Unknown, IntegrationResult.InitialLabel, IntegrationResult.InitialLabel, DateTime.MinValue);
		private IntegrationStatus status;
		private string label;
		private string lastSuccessfulIntegrationLabel;
		private DateTime startTime;
        private ArrayList failureUsers = new ArrayList();
        private ArrayList failureTasks = new ArrayList();

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationSummary" /> class.	
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="label">The label.</param>
        /// <param name="lastSuccessfulIntegrationLabel">The last successful integration label.</param>
        /// <param name="startTime">The start time.</param>
        /// <remarks></remarks>
		public IntegrationSummary(IntegrationStatus status, string label, string lastSuccessfulIntegrationLabel, DateTime startTime)
		{
			this.status = status;
			this.label = label;
			this.lastSuccessfulIntegrationLabel = lastSuccessfulIntegrationLabel;
			this.startTime = startTime;
		}

        /// <summary>
        /// Equalses the specified obj.	
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public override bool Equals(object obj)
		{
			if (obj == null) return false;
			if (obj.GetType() != this.GetType()) return false;

			IntegrationSummary other = (IntegrationSummary) obj;
			return other.status.Equals(this.status) && other.label == this.label;
		}

        /// <summary>
        /// Gets the hash code.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override int GetHashCode()
		{
			return this.label.GetHashCode();
		}


        /// <summary>
        /// Toes the string.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override string ToString()
		{
			return string.Format(System.Globalization.CultureInfo.CurrentCulture,"Status: {0}, Label: {1}", this.status, this.label);
		}

        /// <summary>
        /// Gets the label.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public string Label
		{
			get { return this.label; }
		}

        /// <summary>
        /// Gets the status.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public IntegrationStatus Status
		{
			get { return this.status; }
		}

        /// <summary>
        /// Gets the last successful integration label.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public string LastSuccessfulIntegrationLabel
		{
			get { return this.lastSuccessfulIntegrationLabel; }
		}

        /// <summary>
        /// Gets the start time.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public DateTime StartTime
		{
			get { return this.startTime; }
		}

        /// <summary>
        /// Determines whether this instance is initial.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public bool IsInitial()
		{
			return this.label == IntegrationResult.InitialLabel;
		}

        /// <summary>
        /// Gets or sets the failure users.	
        /// </summary>
        /// <value>The failure users.</value>
        /// <remarks></remarks>
        public ArrayList FailureUsers
        {
            get { return this.failureUsers; }
            set { this.failureUsers = value; }
        }

        /// <summary>
        /// Gets or sets the failure tasks.	
        /// </summary>
        /// <value>The failure tasks.</value>
        /// <remarks></remarks>
        public ArrayList FailureTasks
        {
            get { return this.failureTasks; }
            set { this.failureTasks = value; }
        }
    }
}