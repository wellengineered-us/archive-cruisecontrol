

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core.Tasks.Conditions
{
	/// <title>Status Condition</title>
    /// <version>1.6</version>
    /// <summary>
    /// Checks if the current status matches a value.
    /// </summary>
    /// <example>
    /// <code title="Basic example">
    /// <![CDATA[
    /// <statusCondition value="Success" />
    /// ]]>
    /// </code>
    /// <code title="Example in context">
    /// <![CDATA[
    /// <conditional>
    /// <conditions>
    /// <statusCondition>
    /// <value>Failure</value>
    /// </statusCondition>
    /// </conditions>
    /// <tasks>
    /// <!-- Tasks to perform if condition passed -->
    /// </tasks>
    /// <elseTasks>
    /// <!-- Tasks to perform if condition failed -->
    /// </elseTasks>
    /// </conditional>
    /// ]]>
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// This condition has been kindly supplied by Lasse Sjørup. The original project is available from
    /// <link>http://ccnetconditional.codeplex.com/</link>.
    /// </para>
    /// </remarks>
    [ReflectorType("statusCondition")]
    public class StatusTaskCondition
        : ConditionBase
    {
        #region Public properties
        #region Status
        /// <summary>
        /// The status to match.
        /// </summary>
        /// <version>1.6</version>
        /// <default>n/a</default>
        [ReflectorProperty("value", Required = true)]
        public IntegrationStatus Status { get; set; }
        #endregion
        #endregion

        #region Protected methods
        #region Evaluate()
        /// <summary>
        /// Performs the actual evaluation.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>
        /// <c>true</c> if the condition is true; <c>false</c> otherwise.
        /// </returns>
        protected override bool Evaluate(IIntegrationResult result)
        {
            this.LogDescriptionOrMessage("Checking status - matching to " + this.Status);
            return this.Status == result.Status;
        }
        #endregion
        #endregion
    }
}
