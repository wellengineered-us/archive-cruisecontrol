using System;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.Publishers
{
    /// <summary>
    /// This element  allows to set specific subject messages according to the state of the build. When a certain state
    /// is not specified, a default will be entered.
    /// </summary>
    /// <title>Email Subject</title>
    /// <version>1.0</version>
    /// <example>
    /// <code>
    /// &lt;subject buildResult="StillBroken" value="Build is still broken for {CCNetProject}" /&gt;
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// It is also possible to use <link>Integration Properties</link> in this section. For example:
    /// </para>
    /// <code>
    /// &lt;subjectSettings&gt;
    /// &lt;subject buildResult="StillBroken" value="Build is still broken for ${CCNetProject}, please check again" /&gt;
    /// &lt;/subjectSettings&gt;
    /// </code>
    /// <para>
    /// or:
    /// </para>
    /// <code>
    /// &lt;subjectSettings&gt;
    /// &lt;subject buildResult="StillBroken" value="Build is still broken for ${CCNetProject}, the fix failed." /&gt;
    /// &lt;subject buildResult="Broken" value="{CCNetProject} broke at ${CCNetBuildDate} ${CCNetBuildTime } , last checkin(s) by ${CCNetFailureUsers}" /&gt;
    /// &lt;subject buildResult="Exception" value="Serious problem for ${CCNetProject}, it is now in Exception! Check status of network / sourcecontrol" /&gt;
    /// &lt;/subjectSettings&gt;
    /// </code>
    /// </remarks>
    [ReflectorType("subject")]
    public class EmailSubject
    {

        /// <summary>
        /// 	
        /// </summary>
        public enum BuildResultType
        {
            /// <summary>
            /// Build is ok
            /// </summary>
            Success, 
            /// <summary>
            /// Build has failed
            /// </summary>
            Broken, 
            /// <summary>
            /// Build has failed, and previous one was also failed
            /// </summary>
            StillBroken, 
            /// <summary>
            /// Build is ok, but previous one was failed
            /// </summary>
            Fixed, 
            /// <summary>
            /// An unforeseen exception occured during the build (source control error for example)
            /// </summary>
            Exception
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public EmailSubject()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSubject" /> class.	
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="value">The value.</param>
        /// <remarks></remarks>
        public EmailSubject(BuildResultType result, string value )
		{
            this.Value = value;
			this.BuildResult = result;
		}

        /// <summary>
        /// The value of the subject line, the text to be used for the subject. This may contain variables, see below. 
        /// </summary>
        /// <version>1.0</version>
        /// <default>n/a</default>
        [ReflectorProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// A build result state, see below for the possible values.
        /// </summary>
        /// <version>1.0</version>
        /// <default>n/a</default>
        [ReflectorProperty("buildResult")]
        public BuildResultType BuildResult { get; set; }

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
            EmailSubject g = (EmailSubject)o;
            return this.BuildResult == g.BuildResult;
        }

        /// <summary>
        /// Gets the hash code.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public override int GetHashCode()
        {
            return this.BuildResult.ToString().GetHashCode();
        }

        /// <summary>
        /// Toes the string.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture,"EmailSubject: [BuildResult: {0}, subject: {1}]", this.BuildResult, this.Value);
        }

    }
}
