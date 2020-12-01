using System;

using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.Publishers
{
    /// <summary>
    /// Defines a group of users to receive e-mails.
    /// </summary>
    /// <title>Email Group</title>
    /// <version>1.3</version>
    /// <example>
    /// <code>
    /// &lt;group name="developers"&gt;
    /// &lt;notifications&gt;
    /// &lt;notificationType&gt;Failed&lt;/notificationType&gt;
    /// &lt;notificationType&gt;Fixed&lt;/notificationType&gt;
    /// &lt;/notifications&gt;
    /// &lt;/group&gt;
    /// </code>
    /// </example>
    /// <remarks>
    /// <para type="warning">
    /// Up to CC.NET version 1.4.4, notification is a single attribute on the group. Starting with CC.NET 1.5.0, 
    /// this has been changed to an array of notification types. From 1.5.0 onwards, the Failed notification type, 
    /// is just failed, it does not include the Exception anymore. Making it possible to mail Exception to the
    /// buildmaster, and Failed to the developpers. Developers will not get Exception mails, unless configured so.
    /// </para>
    /// </remarks>
    [ReflectorType("group")]
	public class EmailGroup
	{

        private NotificationType[] notifications = { EmailGroup.NotificationType.Always };

        /// <summary>
        /// 	
        /// </summary>
		public enum NotificationType
		{
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
			Always,
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
			Change,
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
			Failed,
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
			Success,
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
            Fixed,
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
            Exception
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailGroup" /> class.	
        /// </summary>
        /// <remarks></remarks>
		public EmailGroup()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailGroup" /> class.	
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="notifications">The notifications.</param>
        /// <remarks></remarks>
		public EmailGroup(string name, NotificationType[] notifications)
		{
			this.Name = name;
			this.Notifications = notifications;
		}

        /// <summary>
        /// The name of the group, which corresponds to the "group" values used in the &lt;user&gt; elements. 
        /// </summary>
        /// <version>1.3</version>
        /// <default>n.a</default>
        [ReflectorProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// A list of notification types, determining when to send email to this group.
        /// </summary>
        /// <version>1.3</version>
        /// <default>n.a</default>
        [ReflectorProperty("notifications", Required = false)]
        public NotificationType[] Notifications
        {
            get { return this.notifications; }
            set { this.notifications = value; }
        }


        /// <summary>
        /// Equalses the specified o.	
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public override bool Equals(Object o)
		{
			if (o == null || o.GetType() != this.GetType())
			{
				return false;
			}
			EmailGroup g = (EmailGroup) o;
			return this.Name == g.Name ;
		}

        /// <summary>
        /// Gets the hash code.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override int GetHashCode()
		{
            return this.Name.GetHashCode() & StringUtil.GetArrayContents(this.Notifications).GetHashCode() ; 
		}

        /// <summary>
        /// Toes the string.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override string ToString()
		{
			return string.Format(System.Globalization.CultureInfo.CurrentCulture,"EmailGroup: [name: {0}, notifications: {1}]", this.Name, StringUtil.GetArrayContents( this.Notifications) );
		}


        /// <summary>
        /// Determines whether the specified to search has notification.	
        /// </summary>
        /// <param name="toSearch">To search.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool HasNotification(NotificationType toSearch)
        {
            bool found = false;

            foreach (NotificationType nt in this.Notifications)
            {
                if (nt == toSearch)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

	}
}