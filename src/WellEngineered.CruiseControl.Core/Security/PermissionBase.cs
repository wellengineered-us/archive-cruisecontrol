using WellEngineered.CruiseControl.Core.Config;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.Remote.Security;

namespace WellEngineered.CruiseControl.Core.Security
{
    /// <summary>
    /// 	
    /// </summary>
    public abstract class PermissionBase
        : Permissions, IConfigurationValidation
    {
        #region Private fields
        private string refId;
        private ISecurityManager manager;
        #endregion

        #region Public properties
        #region RefId
        /// <summary>
        /// The identifier of the referenced permission.
        /// </summary>
        /// <version>1.5</version>
        /// <default>None</default>
        [ReflectorProperty("ref", Required = false)]
        public string RefId
        {
            get { return this.refId; }
            set { this.refId = value; }
        }
        #endregion

        #region Manager
        /// <summary>
        /// The security manager that loaded this setting.
        /// </summary>
        public ISecurityManager Manager
        {
            get { return this.manager; }
            set { this.manager = value; }
        }
        #endregion
        #endregion

        #region Public methods
        #region CheckUser()
        /// <summary>
        /// Checks if the user should use this permission.
        /// </summary>
        /// <param name="userName">The name of the user that is being checked.</param>
        /// <param name="manager"></param>
        /// <returns>True if the permission is valid for the user, false otherwise.</returns>
        public virtual bool CheckUser(ISecurityManager manager, string userName)
        {
            if (string.IsNullOrEmpty(this.refId))
            {
                return this.CheckUserActual(manager, userName);
            }
            else
            {
                IPermission refPermission = manager.RetrievePermission(this.refId);
                if (refPermission == null)
                {
                    throw new BadReferenceException(this.refId);
                }
                else
                {
                    return refPermission.CheckUser(manager, userName);
                }
            }
        }
        #endregion

        #region CheckPermission()
        /// <summary>
        /// Checks the result of this permission.
        /// </summary>
        /// <param name="permission">The permission to check.</param>
        /// <param name="manager"></param>
        /// <returns>The security right.</returns>
        public virtual SecurityRight CheckPermission(ISecurityManager manager, SecurityPermission permission)
        {
            if (string.IsNullOrEmpty(this.refId))
            {
                return this.CheckPermissionActual(manager, permission);
            }
            else
            {
                IPermission refPermission = manager.RetrievePermission(this.refId);
                if (refPermission == null)
                {
                    throw new BadReferenceException(this.refId);
                }
                else
                {
                    return refPermission.CheckPermission(manager, permission);
                }
            }
        }
        #endregion

        #region Validate()
        /// <summary>
        /// Checks the internal validation of the item.
        /// </summary>
        /// <param name="configuration">The entire configuration.</param>
        /// <param name="parent">The parent item for the item being validated.</param>
        /// <param name="errorProcesser">The error processer to use.</param>
        public virtual void Validate(IConfiguration configuration, ConfigurationTrace parent, IConfigurationErrorProcesser errorProcesser)
        {
            if (!string.IsNullOrEmpty(this.refId))
            {
                IPermission refPermission = configuration.SecurityManager.RetrievePermission(this.refId);
                if (refPermission == null)
                {
                    errorProcesser.ProcessError(new BadReferenceException(this.refId));
                }
            }
        }
        #endregion
        #endregion

        #region Protected methods
        #region CheckUserActual()
        /// <summary>
        /// Checks the user actual.	
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected abstract bool CheckUserActual(ISecurityManager manager, string userName);
        #endregion

        #region CheckPermissionActual()
        /// <summary>
        /// Checks the result of this permission.
        /// </summary>
        /// <param name="permission">The permission to check.</param>
        /// <param name="manager"></param>
        /// <returns>The security right.</returns>
        protected virtual SecurityRight CheckPermissionActual(ISecurityManager manager, SecurityPermission permission)
        {
            return this.GetPermission(permission);
        }
        #endregion
        #endregion
    }
}
