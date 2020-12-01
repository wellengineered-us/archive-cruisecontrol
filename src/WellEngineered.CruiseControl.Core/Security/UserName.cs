using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.Security
{
    /// <summary>
    /// The name of a user.
    /// </summary>
    /// <title>User Name</title>
    /// <version>1.5</version>
    /// <example>
    /// <code>
    /// &lt;userName name="me" /&gt;
    /// </code>
    /// </example>
    [ReflectorType("userName")]
    public class UserName
    {
        private string userName;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserName" /> class.	
        /// </summary>
        /// <remarks></remarks>
        public UserName() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="UserName" /> class.	
        /// </summary>
        /// <param name="name">The name.</param>
        /// <remarks></remarks>
        public UserName(string name)
        {
            this.userName = name;
        }

        /// <summary>
        /// The name of the user.
        /// </summary>
        /// <version>1.5</version>
        /// <default>n/a</default>
        [ReflectorProperty("name")]
        public string Name
        {
            get { return this.userName; }
            set { this.userName = value; }
        }
    }
}
