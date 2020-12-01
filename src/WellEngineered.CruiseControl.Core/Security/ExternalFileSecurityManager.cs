﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

using WellEngineered.CruiseControl.Core.Config;
using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.Remote.Messages;
using WellEngineered.CruiseControl.Remote.Security;

namespace WellEngineered.CruiseControl.Core.Security
{
    /// <summary>
    /// Defines a security manager implementation that implements security with configuration
    /// in external files.
    /// </summary>
    /// <title>External File Server Security</title>
    /// <version>1.5</version>
    /// <example>
    /// <code>
    /// &lt;externalFileSecurity&gt;
    /// &lt;cache type="inMemoryCache" duration="10" mode="sliding"/&gt;
    /// &lt;files&gt;
    /// &lt;file&gt;users.xml&lt;/file&gt;
    /// &lt;file&gt;permissions.xml&lt;/file&gt;
    /// &lt;/files&gt;
    /// &lt;/externalFileSecurity&gt;
    /// </code>
    /// </example>
    /// <remarks>
    /// <heading>External File Format</heading>
    /// <para>
    /// The elementsin the external file uses the standard user (<link>Security Users</link>) and permission definitions 
    /// (<link>Security Permissions</link>).
    /// </para>
    /// <para>
    /// It is possible to define multiple external security files. Each file can define the users and/or permissions for different areas (e.g.
    /// different departments).
    /// </para>
    /// <includePage>General Security Permissions</includePage>
    /// </remarks>
    [ReflectorType("externalFileSecurity")]
    public class ExternalFileSecurityManager
        : SecurityManagerBase, IConfigurationValidation
    {
        #region Private consts
        private const string CONFIG_ASSEMBLY_PATTERN = "ccnet.*.plugin.dll";
        #endregion

        #region Private fields
        private string[] files;
        private Dictionary<string, IAuthentication> loadedUsers;
        private List<IAuthentication> wildCardUsers;
        private Dictionary<string, IPermission> loadedPermissions;
        private bool isInitialised/* = false*/;
        private NetReflectorTypeTable typeTable;
        private NetReflectorReader reflectionReader;
        private Dictionary<string, string> settingFileMap;
        private readonly IExecutionEnvironment executionEnvironment;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalFileSecurityManager" /> class.	
        /// </summary>
        /// <remarks></remarks>
        public ExternalFileSecurityManager()
            : this(new ExecutionEnvironment())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalFileSecurityManager" /> class.	
        /// </summary>
        /// <param name="executionEnvironment">The execution environment.</param>
        /// <remarks></remarks>
        public ExternalFileSecurityManager(IExecutionEnvironment executionEnvironment)
        {
            this.executionEnvironment = executionEnvironment;
        }

        #region Public properties
        #region Files
        /// <summary>
        /// The files to load.
        /// </summary>
        /// <version>1.5</version>
        /// <default>n/a</default>
        [ReflectorProperty("files")]
        public string[] Files
        {
            get { return this.files; }
            set { this.files = value; }
        }
        #endregion
        #endregion

        #region Public methods
        #region Initialise()
        /// <summary>
        /// Initialise the security manager.
        /// </summary>
        public override void Initialise()
        {
            if (!this.isInitialised)
            {
                // Initialise the reader
                this.typeTable = new NetReflectorTypeTable();
                this.typeTable.Add(AppDomain.CurrentDomain);
                this.typeTable.Add(Directory.GetCurrentDirectory(), CONFIG_ASSEMBLY_PATTERN);
                this.typeTable.InvalidNode += delegate(InvalidNodeEventArgs args)
                {
                    throw new CruiseControlException(args.Message);
                };
                this.reflectionReader = new NetReflectorReader(this.typeTable);

                // Initialise the local caches
                this.SessionCache.Initialise();
                this.loadedUsers = new Dictionary<string, IAuthentication>();
                this.wildCardUsers = new List<IAuthentication>();
                this.loadedPermissions = new Dictionary<string, IPermission>();

                // Load each file
                this.settingFileMap = new Dictionary<string, string>();
                foreach (string fileName in this.files)
                {
                    this.LoadFile(fileName);
                }
            }

            this.isInitialised = true;
        }
        #endregion

        #region RetrieveUser()
        /// <summary>
        /// Retrieves a user from the store.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public override IAuthentication RetrieveUser(string identifier)
        {
            IAuthentication setting = null;
            if (!string.IsNullOrEmpty(identifier))
            {
                // If initialised then use the loaded dictionaries
                if (this.isInitialised)
                {
                    identifier = identifier.ToLower(CultureInfo.InvariantCulture);

                    if ((setting == null) && (this.loadedUsers.ContainsKey(identifier)))
                    {
                        setting = this.loadedUsers[identifier];
                    }

                    if (setting == null)
                    {
                        // Attempt to find a matching wild-card
                        foreach (IAuthentication wildCard in this.wildCardUsers)
                        {
                            if (SecurityHelpers.IsWildCardMatch(wildCard.Identifier, identifier))
                            {
                                setting = wildCard;
                                break;
                            }
                        }
                    }
                }
            }
            return setting;
        }
        #endregion

        #region RetrievePermission()
        /// <summary>
        /// Retrieves a permission from the store.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public override IPermission RetrievePermission(string identifier)
        {
            IPermission setting = null;
            if (!string.IsNullOrEmpty(identifier))
            {
                // If initialised then use the loaded dictionaries
                if (this.isInitialised)
                {
                    identifier = identifier.ToLower(CultureInfo.InvariantCulture);

                    if (this.loadedPermissions.ContainsKey(identifier))
                    {
                        setting = this.loadedPermissions[identifier];
                    }
                }
            }
            return setting;
        }
        #endregion

        #region ListAllUsers()
        /// <summary>
        /// Lists all the users who have been defined in the system.
        /// </summary>
        /// <returns>
        /// A list of <see cref="UserDetails"/> containing the details on all the users
        /// who have been defined.
        /// </returns>
        public override List<UserDetails> ListAllUsers()
        {
            List<UserDetails> usersList = new List<UserDetails>();
            foreach (IAuthentication userDetails in this.loadedUsers.Values)
            {
                // Generate the details to return
                UserDetails user = new UserDetails();
                user.UserName = userDetails.UserName;
                user.DisplayName = userDetails.DisplayName;
                user.Type = userDetails.AuthenticationName;
                usersList.Add(user);

            }
            return usersList;
        }
        #endregion

        #region CheckServerPermission()
        /// <summary>
        /// Checks whether the user can perform the specified action at the server level.
        /// </summary>
        /// <param name="userName">The name of the user that is being checked.</param>
        /// <param name="permission">The permission to check.</param>
        /// <returns>True if the permission is valid, false otherwise.</returns>
        public override bool CheckServerPermission(string userName, SecurityPermission permission)
        {
            SecurityRight currentRight = SecurityRight.Inherit;

            // Iterate through the permissions stopping when we hit the first non-inherited permission
            foreach (IPermission permissionToCheck in this.loadedPermissions.Values)
            {
                if (permissionToCheck.CheckUser(this, userName)) currentRight = permissionToCheck.CheckPermission(this, permission);
                if (currentRight != SecurityRight.Inherit) break;
            }

            // If we don't have a result, then use the default right
            if (currentRight == SecurityRight.Inherit) currentRight = this.GetDefaultRight(permission);
            return (currentRight == SecurityRight.Allow);
        }
        #endregion

        #region Validate()
        /// <summary>
        /// Checks the internal validation of the item.
        /// </summary>
        /// <param name="configuration">The entire configuration.</param>
        /// <param name="parent">The parent item for the item being validated.</param>
        /// <param name="errorProcesser"></param>
        public virtual void Validate(IConfiguration configuration, ConfigurationTrace parent, IConfigurationErrorProcesser errorProcesser)
        {
            var isInitialised = false;
            try
            {
                this.Initialise();
                isInitialised = true;
            }
            catch (Exception error)
            {
                errorProcesser.ProcessError(error);
            }

            if (isInitialised)
            {
                foreach (IAuthentication user in this.loadedUsers.Values)
                {
                    var config = user as IConfigurationValidation;
                    if (config != null)
                    {
                        config.Validate(configuration, parent.Wrap(this), errorProcesser);
                    }
                }

                foreach (IPermission permission in this.loadedPermissions.Values)
                {
                    var config = permission as IConfigurationValidation;
                    if (config != null)
                    {
                        config.Validate(configuration, parent.Wrap(this), errorProcesser);
                    }
                }
            }
        }
        #endregion

        #region ChangePassword()
        /// <summary>
        /// Changes the password of the user.
        /// </summary>
        /// <param name="sessionToken">The session token for the current user.</param>
        /// <param name="oldPassword">The person's old password.</param>
        /// <param name="newPassword">The person's new password.</param>
        public override void ChangePassword(string sessionToken, string oldPassword, string newPassword)
        {
            // Retrieve the user
            string userName = this.GetUserName(sessionToken);
            if (string.IsNullOrEmpty(userName)) throw new SessionInvalidException();
            IAuthentication user = this.RetrieveUser(userName);
            if (user == null) throw new SessionInvalidException();

            // Validate the old password
            LoginRequest credientals = new LoginRequest(userName);
            credientals.AddCredential(LoginRequest.PasswordCredential, oldPassword);
            if (!user.Authenticate(credientals))
            {
                this.LogEvent(null, userName, SecurityEvent.ChangePassword, SecurityRight.Deny, "Old password is incorrect");
                throw new SecurityException("Old password is incorrect");
            }

            // Change the password
            this.LogEvent(null, userName, SecurityEvent.ChangePassword, SecurityRight.Allow, null);
            user.ChangePassword(newPassword);

            // Update the file
            this.UpdateSetting(user);
        }
        #endregion

        #region ResetPassword()
        /// <summary>
        /// Resets the password for a user.
        /// </summary>
        /// <param name="sessionToken">The session token for the current user.</param>
        /// <param name="userName">The user name to reset the password for.</param>
        /// <param name="newPassword">The person's new password.</param>
        public override void ResetPassword(string sessionToken, string userName, string newPassword)
        {
            // Retrieve the user and make sure they have the right permission
            string currentUser = this.GetUserName(sessionToken);
            if (string.IsNullOrEmpty(currentUser)) throw new SessionInvalidException();
            if (!this.CheckServerPermission(currentUser, SecurityPermission.ModifySecurity))
            {
                this.LogEvent(null, currentUser, SecurityEvent.ResetPassword, SecurityRight.Deny, null);
                throw new PermissionDeniedException("Reset password");
            }

            // Change the password
            this.LogEvent(null, currentUser, SecurityEvent.ResetPassword, SecurityRight.Allow,
                string.Format(System.Globalization.CultureInfo.CurrentCulture,"Reset password for '{0}'", userName));
            IAuthentication user = this.RetrieveUser(userName);
            if (user == null) throw new SessionInvalidException();
            user.ChangePassword(newPassword);

            // Update the file
            this.UpdateSetting(user);
        }
        #endregion
        #endregion

        #region Private methods
        #region LoadFile()
        /// <summary>
        /// Loads all the settings from a file.
        /// </summary>
        /// <param name="fileName"></param>
        private void LoadFile(string fileName)
        {
            XmlDocument sourceDocument = new XmlDocument();
            sourceDocument.Load(this.executionEnvironment.EnsurePathIsRooted(fileName));

            foreach (XmlElement setting in sourceDocument.DocumentElement.SelectNodes("*"))
            {
                object loadedItem = this.reflectionReader.Read(setting);

                IPermission permission = loadedItem as IPermission;
                if (permission != null)
                {
                    permission.Manager = this;
                    string identifier = permission.Identifier.ToLower(CultureInfo.InvariantCulture);
                    if (this.loadedPermissions.ContainsKey(identifier)) this.loadedPermissions.Remove(identifier);
                    this.loadedPermissions.Add(identifier, permission);
                    this.LinkIdentifierWithFile(fileName, identifier);
                }
                else
                {
                    IAuthentication authentication = loadedItem as IAuthentication;
                    if (authentication != null)
                    {
                        authentication.Manager = this;
                        string identifier = authentication.Identifier.ToLower(CultureInfo.InvariantCulture);
                        if (this.loadedUsers.ContainsKey(identifier)) this.loadedUsers.Remove(identifier);
                        if (authentication.Identifier.Contains("*"))
                        {
                            this.wildCardUsers.Add(authentication);
                        }
                        else
                        {
                            this.loadedUsers.Add(identifier, authentication);
                        }
                        this.LinkIdentifierWithFile(fileName, identifier);
                    }
                    else
                    {
                        throw new CruiseControlException("Unknown security item: " + setting.OuterXml);
                    }
                }
            }
        }
        #endregion

        #region LinkIdentifierWithFile()
        /// <summary>
        /// Links an identifier with the file it came from.
        /// </summary>
        /// <param name="fileName">The source file.</param>
        /// <param name="identifier">The identifier.</param>
        private void LinkIdentifierWithFile(string fileName, string identifier)
        {
            if (this.settingFileMap.ContainsKey(identifier))
            {
                this.settingFileMap[identifier] = fileName;
            }
            else
            {
                this.settingFileMap.Add(identifier, fileName);
            }
        }
        #endregion

        #region UpdateSetting()
        /// <summary>
        /// Updates the file that a setting is in.
        /// </summary>
        /// <param name="setting">The setting that has been changed.</param>
        private void UpdateSetting(ISecuritySetting setting)
        {
            // Load the file that the setting is in
            string fileName = this.settingFileMap[setting.Identifier];
            XmlDocument sourceDocument = new XmlDocument();
            sourceDocument.Load(this.executionEnvironment.EnsurePathIsRooted(fileName));

            // Find the item that is being updated
            foreach (XmlElement settingEl in sourceDocument.DocumentElement.SelectNodes("*"))
            {
                object loadedItem = this.reflectionReader.Read(settingEl);

                var dummy = loadedItem as ISecuritySetting;
                if (dummy != null)
                {
                    string identifier = dummy.Identifier;
                    if (identifier == setting.Identifier)
                    {
                        // Update the item with the new setting
                        StringWriter buffer = new StringWriter();
                        new ReflectorTypeAttribute(settingEl.Name).Write(new XmlTextWriter(buffer), setting);
                        XmlElement element = sourceDocument.CreateElement("changed");
                        element.InnerXml = buffer.ToString();
                        settingEl.ParentNode.ReplaceChild(element.FirstChild, settingEl);
                        break;
                    }
                }
            }

            // Save the updated document
            sourceDocument.Save(fileName);
        }
        #endregion
        #endregion
    }
}
