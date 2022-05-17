using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.Remote.Messages;

namespace WellEngineered.CruiseControl.Core.Security
{
    /// <summary>
    /// Stores a user name - authentication will come from Active Directory.
    /// </summary>
    /// <title>LDAP User Authentication</title>
    /// <version>1.5</version>
    /// <example>
    /// <code title="Simple Example">
    /// &lt;ldapUser name="johndoe" domain="somewhere.com"/&gt;
    /// </code>
    /// <code title="Wildcard Example">
    /// &lt;ldapUser name="*" domain="somewhere.com"/&gt;
    /// </code>
    /// </example>
    [ReflectorType("ldapUser")]
    public class ActiveDirectoryAuthentication
        : IAuthentication
    {
        private const string userNameCredential = "username";

        private string userName;
        private string domainName;
        private ISecurityManager manager;
        private ILdapService ldapService;


        /// <summary>
        /// Start a new blank authentication.
        /// </summary>
        public ActiveDirectoryAuthentication() 
        {
            //ldapService = new Util.LdapHelper();
        }

        /// <summary>
        /// Start a new authentication with a user name.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="ldap"></param>
        public ActiveDirectoryAuthentication(string userName, ILdapService ldap)
        {
            this.UserName = userName;
            this.ldapService = ldap;
        }

        /// <summary>
        /// A unique identifier for an authentication instance.
        /// </summary>
        public string Identifier
        {
            get { return this.userName; }
        }

        /// <summary>
        /// The user name for this user.
        /// </summary>
        /// <version>1.5</version>
        /// <default>n/a</default>
        [ReflectorProperty("name")]
        public string UserName
        {
            get { return this.userName; }
            set { this.userName = value; }
        }

        /// <summary>
        /// The display name for this user.
        /// </summary>
        public string DisplayName
        {
            get { return null; }
        }

        /// <summary>
        /// The name of the authentication type.
        /// </summary>
        public string AuthenticationName
        {
            get { return "LDAP"; }
        }

        /// <summary>
        /// The AD domain to use.
        /// </summary>
        /// <version>1.5</version>
        /// <default>n/a</default>
        [ReflectorProperty("domain")]
        public string DomainName
        {
            get { return this.domainName; }
            set { this.domainName = value; }
        }

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

        /// <summary>
        /// Attempts to authenticate a user from the credentials.
        /// </summary>
        /// <param name="credentials">The credentials.</param>
        /// <returns>True if the credentials are valid, false otherwise.</returns>
        public bool Authenticate(LoginRequest credentials)
        {
            string retrievedUserName = this.GetUserName(credentials);
            string retrievedPassword = this.GetPassword(credentials);

            // We can't authenticate a user that does not exist.
            if (string.IsNullOrEmpty(retrievedUserName))
                return false;

            return this.ldapService.Authenticate(retrievedUserName, retrievedPassword, this.DomainName);

        }

        /// <summary>
        /// Retrieves the user name from the credentials.
        /// </summary>
        /// <param name="credentials">The credentials.</param>
        /// <returns>The name of the user from the credentials. If the credentials not not exist in the system
        /// then null will be returned.</returns>
        public string GetUserName(LoginRequest credentials)
        {
            Log.Trace("Getting username from credentials"); 
            string dummy = NameValuePair.FindNamedValue(credentials.Credentials, LoginRequest.UserNameCredential);
            
            Log.Trace("found username {0}", dummy);             
            return dummy;
        }


        /// <summary>                                                                                          
        /// Retrieves the password from the credentials.                                                       
        /// </summary>                                                                                         
        /// <param name="credentials">The credentials.</param>                                                 
        /// <returns>The users password from the credentials. If the credentials do not exist in the system    
        /// then null will be returned.</returns>                                                              
        public string GetPassword(LoginRequest credentials)
        {
            return NameValuePair.FindNamedValue(credentials.Credentials, LoginRequest.PasswordCredential);
        }                                                                                                      
                                                                                                           



        /// <summary>
        /// Retrieves the display name from the credentials.
        /// </summary>
        /// <param name="credentials">The credentials.</param>
        /// <returns>The name of the user from the credentials. If the credentials do not exist in the system
        /// then null will be returned.</returns>
        public string GetDisplayName(LoginRequest credentials)
        {
            
            string userName = this.GetUserName(credentials);
            string nameToReturn = userName;

            this.ldapService.DomainName = this.DomainName;
            LdapUserInfo lu = this.ldapService.RetrieveUserInformation(userName);


            if (!string.IsNullOrEmpty(lu.DisplayName))
            {
                nameToReturn = lu.DisplayName;
            }

            return nameToReturn;
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="newPassword"></param>
        public void ChangePassword(string newPassword)
        {
            // We do not allow the user to change LDAP passwords.
        }


    }
}
