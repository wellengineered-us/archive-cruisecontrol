using System.ComponentModel;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote.Security;

namespace WellEngineered.CruiseControl.Core.Security
{
    /// <summary>
    /// Defines a set of permissions.
    /// </summary>
    /// <title>General Security Permissions</title>
    [ReflectorType("permissions")]
    public class Permissions
    {
        #region Private fields
        private SecurityRight defaultRight = SecurityRight.Inherit;
        private SecurityRight sendMessage = SecurityRight.Inherit;
        private SecurityRight forceAbortBuild = SecurityRight.Inherit;
        private SecurityRight startStopProject = SecurityRight.Inherit;
        private SecurityRight changeProjectConfiguration = SecurityRight.Inherit;
        private SecurityRight viewSecurity = SecurityRight.Inherit;
        private SecurityRight modifySecurity = SecurityRight.Inherit;
        private SecurityRight viewProject = SecurityRight.Inherit;
        private SecurityRight viewConfiguration = SecurityRight.Inherit;
        #endregion

        #region Public properties
        #region DefaultRight
        /// <summary>
        /// The default right to use.
        /// </summary>
        /// <version>1.5</version>
        /// <default>Inherit</default>
        [ReflectorProperty("defaultRight", Required = false)]
        [DefaultValue(SecurityRight.Inherit)]
        public SecurityRight DefaultRight
        {
            get { return this.defaultRight; }
            set { this.defaultRight = value; }
        }
        #endregion

        #region SendMessageRight
        /// <summary>
        /// The right to send messages.
        /// </summary>
        /// <version>1.5</version>
        /// <default>Inherit</default>
        [ReflectorProperty("sendMessage", Required = false)]
        [DefaultValue(SecurityRight.Inherit)]
        public SecurityRight SendMessageRight
        {
            get { return this.sendMessage; }
            set { this.sendMessage = value; }
        }
        #endregion

        #region ForceBuildRight
        /// <summary>
        /// The right for force or abort builds.
        /// </summary>
        /// <version>1.5</version>
        /// <default>Inherit</default>
        [ReflectorProperty("forceBuild", Required = false)]
        [DefaultValue(SecurityRight.Inherit)]
        public SecurityRight ForceBuildRight
        {
            get { return this.forceAbortBuild; }
            set { this.forceAbortBuild = value; }
        }
        #endregion

        #region StartProjectRight
        /// <summary>
        /// The right to stop and start projects.
        /// </summary>
        /// <version>1.5</version>
        /// <default>Inherit</default>
        [ReflectorProperty("startProject", Required = false)]
        [DefaultValue(SecurityRight.Inherit)]
        public SecurityRight StartProjectRight
        {
            get { return this.startStopProject; }
            set { this.startStopProject = value; }
        }
        #endregion

        #region ChangeProjectRight
        /// <summary>
        /// The right to change the configuration of projects.
        /// </summary>
        /// <version>1.5</version>
        /// <default>Inherit</default>
        [ReflectorProperty("changeProject", Required = false)]
        [DefaultValue(SecurityRight.Inherit)]
        public SecurityRight ChangeProjectRight
        {
            get { return this.changeProjectConfiguration; }
            set { this.changeProjectConfiguration = value; }
        }
        #endregion

        #region ViewSecurityRight
        /// <summary>
        /// The right to view security.
        /// </summary>
        /// <version>1.5</version>
        /// <default>Inherit</default>
        [ReflectorProperty("viewSecurity", Required = false)]
        [DefaultValue(SecurityRight.Inherit)]
        public SecurityRight ViewSecurityRight
        {
            get { return this.viewSecurity; }
            set { this.viewSecurity = value; }
        }
        #endregion

        #region ModifySecurityRight
        /// <summary>
        /// The right to modify security.
        /// </summary>
        /// <version>1.5</version>
        /// <default>Inherit</default>
        [ReflectorProperty("modifySecurity", Required = false)]
        [DefaultValue(SecurityRight.Inherit)]
        public SecurityRight ModifySecurityRight
        {
            get { return this.modifySecurity; }
            set { this.modifySecurity = value; }
        }
        #endregion

        #region ViewProjectRight
        /// <summary>
        /// The right to view a project.
        /// </summary>
        /// <version>1.5</version>
        /// <default>Inherit</default>
        [ReflectorProperty("viewProject", Required = false)]
        [DefaultValue(SecurityRight.Inherit)]
        public SecurityRight ViewProjectRight
        {
            get { return this.viewProject; }
            set { this.viewProject = value; }
        }
        #endregion

        #region ViewConfigurationRight
        /// <summary>
        /// The right to view configuration and logs.
        /// </summary>
        /// <version>1.5</version>
        /// <default>Inherit</default>
        [ReflectorProperty("viewConfiguration", Required = false)]
        [DefaultValue(SecurityRight.Inherit)]
        public SecurityRight ViewConfigurationRight
        {
            get { return this.viewConfiguration; }
            set { this.viewConfiguration = value; }
        }
        #endregion
        #endregion

        #region Public methods
        #region GetPermission()
        /// <summary>
        /// Retrieves the actual permission.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public SecurityRight GetPermission(SecurityPermission permission)
        {
            var right = SecurityRight.Inherit;
            switch (permission)
            {
                case SecurityPermission.ViewProject:
                    right = this.ViewProjectRight;
                    break;
                case SecurityPermission.ViewConfiguration:
                    right = this.ViewConfigurationRight;
                    break;
                case SecurityPermission.ForceAbortBuild:
                    right = this.ForceBuildRight;
                    break;
                case SecurityPermission.SendMessage:
                    right = this.SendMessageRight;
                    break;
                case SecurityPermission.StartStopProject:
                    right = this.StartProjectRight;
                    break;
                case SecurityPermission.ChangeProjectConfiguration:
                    right = this.ChangeProjectRight;
                    break;
                case SecurityPermission.ViewSecurity:
                    right = this.ViewSecurityRight;
                    break;
                case SecurityPermission.ModifySecurity:
                    right = this.ModifySecurityRight;
                    break;
            }
            if (right == SecurityRight.Inherit) right = this.DefaultRight;
            return right;
        }
        #endregion
        #endregion
    }
}
