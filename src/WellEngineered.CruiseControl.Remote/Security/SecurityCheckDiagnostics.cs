using System;
using System.Xml.Serialization;

namespace WellEngineered.CruiseControl.Remote.Security
{
    /// <summary>
    /// Provides diagnostics on a security check.
    /// </summary>
    [Serializable]
    public class SecurityCheckDiagnostics
    {
        private string permissionName;
        private string projectName;
        private string userName;
        private bool isAllowed;

        /// <summary>
        /// The name of the permission being diagnosed.
        /// </summary>
        [XmlAttribute("permission")]
        public string Permission
        {
            get { return this.permissionName; }
            set { this.permissionName = value; }
        }

        /// <summary>
        /// The name of the project the permission is being checked against.
        /// </summary>
        [XmlAttribute("project")]
        public string Project
        {
            get { return this.projectName; }
            set { this.projectName = value; }
        }

        /// <summary>
        /// The name of the user being the permission is being checked for.
        /// </summary>
        [XmlAttribute("user")]
        public string User
        {
            get { return this.userName; }
            set { this.userName = value; }
        }

        /// <summary>
        /// Whether this permission is allowed.
        /// </summary>
        [XmlAttribute("allowed")]
        public bool IsAllowed
        {
            get { return this.isAllowed; }
            set { this.isAllowed = value; }
        }
    }
}
