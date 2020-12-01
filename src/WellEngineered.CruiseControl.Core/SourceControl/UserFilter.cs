using System;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.SourceControl
{
    /// <summary>
    /// The UserFilter can be used to filter modifications on the basis of the username that committed the changes.
    /// </summary>
    /// <title>UserFilter</title>
    /// <version>1.0</version>
	[ReflectorType("userFilter")]
	public class UserFilter : IModificationFilter
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="UserFilter"/> class.
        /// </summary>
        public UserFilter()
        {
            this.UserNames = new string[0];
        }

        /// <summary>
        /// The user names to filter.
        /// </summary>
        /// <version>1.0</version>
        /// <default>None</default>
        [ReflectorProperty("names")]
        public string[] UserNames { get; set; }

        /// <summary>
        /// Accepts the specified m.	
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public bool Accept(Modification m)
		{
			return Array.IndexOf(this.UserNames, m.UserName) >= 0;
		}

        /// <summary>
        /// Toes the string.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public override string ToString()
        {
            return "UserFilter";
        }
	}
}