﻿using System;

namespace WellEngineered.CruiseControl.Core.Util
{
    /// <summary>
    /// Mapped functionality for an LDAP service
    /// </summary>
    public interface ILdapService
    {

        /// <summary>
        /// Gets or sets the name of the domain.	
        /// </summary>
        /// <value>The name of the domain.</value>
        /// <remarks></remarks>
        string DomainName { get; set; }


        /// <summary>
        /// Retrieves the information of the specified user
        /// </summary>
        /// <param name="userNameToRetrieveFrom"></param>
        /// <returns></returns>
        LdapUserInfo RetrieveUserInformation(string userNameToRetrieveFrom);


        /// <summary>
        /// Tries to authenticate the user to the specified LDAP service (domain)
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domainName"></param>
        /// <returns></returns>
        bool Authenticate(string userName, string password, string domainName);

    }
}
