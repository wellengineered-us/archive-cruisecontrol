using System;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote.Security;

namespace WellEngineered.CruiseControl.Core.Security.Auditing
{
    /// <summary>
    /// A base class for developer audit loggers.
    /// </summary>
    public abstract class AuditLoggerBase
    {
        private bool logSuccessfulEvents = true;
        private bool logFailureEvents = true;

        /// <summary>
        /// Whether to log successful events or not.
        /// </summary>
        /// <version>1.5</version>
        /// <default>true</default>
        [ReflectorProperty("success", Required = false)]
        public bool LogSuccessfulEvents
        {
            get { return this.logSuccessfulEvents; }
            set { this.logSuccessfulEvents = value; }
        }

        /// <summary>
        /// Whether to log failed events or not. 
        /// </summary>
        /// <version>1.5</version>
        /// <default>true</default>
        [ReflectorProperty("failure", Required = false)]
        public bool LogFailureEvents
        {
            get { return this.logFailureEvents; }
            set { this.logFailureEvents = value; }
        }

        /// <summary>
        /// Logs a security event.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="userName">The name of the user.</param>
        /// <param name="eventType">The type of event.</param>
        /// <param name="eventRight">The right of the event.</param>
        /// <param name="message">Any security message.</param>
        public virtual void LogEvent(string projectName, string userName, SecurityEvent eventType, SecurityRight eventRight, string message)
        {
            if ((eventRight == SecurityRight.Allow) && this.logSuccessfulEvents)
            {
                this.DoLogEvent(projectName, userName, eventType, eventRight, message);
            }
            else if ((eventRight == SecurityRight.Deny) && this.logFailureEvents)
            {
                this.DoLogEvent(projectName, userName, eventType, eventRight, message);
            }
            else if (eventRight == SecurityRight.Inherit)
            {
                this.DoLogEvent(projectName, userName, eventType, eventRight, message);
            }
        }

        /// <summary>
        /// Logs an audit record.
        /// </summary>
        /// <param name="record">The record to log.</param>
        public virtual void LogEvent(AuditRecord record)
        {
            this.LogEvent(record.ProjectName,
                record.UserName,
                record.EventType,
                record.SecurityRight,
                record.Message);
            record.TimeOfEvent = DateTime.Now;
        }

        /// <summary>
        /// Performs the actual logging of a security event
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="userName">The name of the user.</param>
        /// <param name="eventType">The type of event.</param>
        /// <param name="eventRight">The right of the event.</param>
        /// <param name="message">Any security message.</param>
        protected abstract void DoLogEvent(string projectName, string userName, SecurityEvent eventType, SecurityRight eventRight, string message);
    }
}
