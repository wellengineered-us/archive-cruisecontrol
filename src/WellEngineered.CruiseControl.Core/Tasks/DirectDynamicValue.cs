using System;
using System.Collections.Generic;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote.Parameters;

namespace WellEngineered.CruiseControl.Core.Tasks
{
    /// <title>Direct Dynamic Value</title>
    /// <version>1.5</version>
    /// <summary>
    /// <para>
    /// This will replace the value of a property with the value from a parameter. If the user does not enter a
    /// parameter value, then the default will be used (when set).
    /// </para>
    /// <para type="warning">
    /// This dynamic value does not perform any formatting, it just directly puts the value into the property.
    /// </para>
    /// </summary>
    /// <example>
    /// <code title="Basic example">
    /// &lt;nant&gt;
    /// &lt;!-- Omitted for brevity --&gt;
    /// &lt;dynamicValues&gt;
    /// &lt;buildArgs&gt;Help&lt;/buildArgs&gt;
    /// &lt;directValue property="buildArgs" parameter="CommandToRun" default="Help"/&gt;
    /// &lt;/dynamicValues&gt;
    /// &lt;/nant&gt;
    /// </code>
    /// <code title="Shorthand example">
    /// &lt;nant&gt;
    /// &lt;!-- Omitted for brevity --&gt;
    /// &lt;dynamicValues&gt;
    /// &lt;buildArgs&gt;$[CommandToRun|Help]&lt;/buildArgs&gt;
    /// &lt;/dynamicValues&gt;
    /// &lt;/nant&gt;
    /// </code>
    /// <code title="Example in context">
    /// &lt;project name="Test Project"&gt;
    /// &lt;sourcecontrol type="svn"&gt;
    /// &lt;!-- Omitted for brevity --&gt;
    /// &lt;/sourcecontrol&gt;
    /// &lt;triggers&gt;
    /// &lt;intervalTrigger /&gt;
    /// &lt;/triggers&gt;
    /// &lt;tasks&gt;
    /// &lt;nant&gt;
    /// &lt;!-- Omitted for brevity --&gt;
    /// &lt;buildArgs&gt;Help&lt;/buildArgs&gt;
    /// &lt;dynamicValues&gt;
    /// &lt;directValue property="buildArgs" parameter="CommandToRun" default="Help"/&gt;
    /// &lt;/dynamicValues&gt;
    /// &lt;/nant&gt;
    /// &lt;!-- Omitted for brevity --&gt;
    /// &lt;/tasks&gt;
    /// &lt;publishers&gt;
    /// &lt;!-- Omitted for brevity --&gt;
    /// &lt;/publishers&gt;
    /// &lt;parameters&gt;
    /// &lt;!-- Omitted for brevity --&gt;
    /// &lt;/parameters&gt;
    /// &lt;/project&gt;
    /// </code>
    /// </example>
    [ReflectorType("directValue")]
    public class DirectDynamicValue
        : IDynamicValue
    {
        private string propertyName;
        private string parameterName;
        private string defaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectDynamicValue" /> class.	
        /// </summary>
        /// <remarks></remarks>
        public DirectDynamicValue() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectDynamicValue" /> class.	
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="property">The property.</param>
        /// <remarks></remarks>
        public DirectDynamicValue(string parameter, string property)
        {
            this.propertyName = property;
            this.parameterName = parameter;
        }

        /// <summary>
        /// The name of the property to set. This must be the same name as what is in the task/publisher/trigger
        /// configuration. 
        /// </summary>
        /// <version>1.5</version>
        /// <default>n/a</default>
        [ReflectorProperty("property")]
        public string PropertyName
        {
            get { return this.propertyName; }
            set { this.propertyName = value; }
        }

        /// <summary>
        /// The name of the parameter to use. This must be the same name as what is in the parameters configuration. 
        /// </summary>
        /// <version>1.5</version>
        /// <default>n/a</default>
        [ReflectorProperty("parameter")]
        public string ParameterName
        {
            get { return this.parameterName; }
            set { this.parameterName = value; }
        }

        /// <summary>
        /// The default value to use if nothing is set in the parameters.
        /// </summary>
        /// <version>1.5</version>
        /// <default>n/a</default>
        [ReflectorProperty("default", Required = false)]
        public string DefaultValue
        {
            get { return this.defaultValue; }
            set { this.defaultValue = value; }
        }

        /// <summary>
        /// Applies a dynamc value to an object.
        /// </summary>
        /// <param name="value">The object to apply the value to.</param>
        /// <param name="parameters">The parameters to apply.</param>
        /// <param name="parameterDefinitions">The original parameter definitions.</param>
        public virtual void ApplyTo(object value, Dictionary<string, string> parameters, IEnumerable<ParameterBase> parameterDefinitions)
        {
            DynamicValueUtility.PropertyValue property = DynamicValueUtility.FindProperty(value, this.propertyName);
            if (property != null)
            {
                string parameterValue = this.defaultValue;
                if (parameters.ContainsKey(this.parameterName))
                {
                    parameterValue = parameters[this.parameterName];
                }

                var actualValue = DynamicValueUtility.ConvertValue(this.parameterName, parameterValue, parameterDefinitions);
                property.ChangeProperty(actualValue);
            }
        }
    }
}
