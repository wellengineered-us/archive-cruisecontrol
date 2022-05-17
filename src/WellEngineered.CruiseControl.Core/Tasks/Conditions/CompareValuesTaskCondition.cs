

using System;
using System.Globalization;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.Tasks.Conditions
{
	/// <title>Compare Values Condition</title>
    /// <version>1.6</version>
    /// <summary>
    /// <para>
    /// Checks if two values are the same.
    /// </para>
    /// <para>
    /// This is typically used with dynamic values.
    /// </para>
    /// </summary>
    /// <example>
    /// <code title="Basic example">
    /// <![CDATA[
    /// <compareCondition>
    /// <value1>${value1}</value1>
    /// <value2>ToMatch</value2>
    /// <evaluation>notEqual</evaluation>
    /// <ignoreCase>true</ignoreCase>
    /// </compareCondition>
    /// ]]>
    /// </code>
    /// <code title="Example in context">
    /// <![CDATA[
    /// <conditional>
    /// <conditions>
    /// <compareCondition>
    /// <value1>${value1}</value1>
    /// <value2>ToMatch</value2>
    /// <evaluation>equal</evaluation>
    /// </compareCondition>
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
    [ReflectorType("compareCondition")]
    public class CompareValuesTaskCondition
        : ConditionBase
    {
        #region Public properties
        #region Value1
        /// <summary>
        /// The first value to evaluate.
        /// </summary>
        /// <version>1.6</version>
        /// <default>n/a</default>
        [ReflectorProperty("value1", Required = true)]
        public string Value1 { get; set; }
        #endregion

        #region Value2
        /// <summary>
        /// The second value to evaluate.
        /// </summary>
        /// <version>1.6</version>
        /// <default>n/a</default>
        [ReflectorProperty("value2", Required = true)]
        public string Value2 { get; set; }
        #endregion

        #region Evaluation
        /// <summary>
        /// The type of evaluation.
        /// </summary>
        /// <version>1.6</version>
        /// <default>n/a</default>
        [ReflectorProperty("evaluation", Required = true)]
        public Evaluation EvaluationType { get; set; }
        #endregion

        #region IgnoreCase
        /// <summary>
        /// Whether to ignore any case differences or not.
        /// </summary>
        /// <version>1.6</version>
        /// <default>false</default>
        [ReflectorProperty("ignoreCase", Required = false)]
        public bool IgnoreCase { get; set; }
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
            this.LogDescriptionOrMessage("Checking value comparison condition - " + this.Value1 + " with " + this.Value2);

            switch (this.EvaluationType)
            {
                case Evaluation.Equal:
                    return string.Compare(this.Value1, this.Value2, this.IgnoreCase, CultureInfo.InvariantCulture) == 0;

                case Evaluation.NotEqual:
                    return string.Compare(this.Value1, this.Value2, this.IgnoreCase, CultureInfo.InvariantCulture) != 0;

                default:
                    throw new InvalidOperationException("Unhandled evaluation type");
            }
        }
        #endregion
        #endregion

        #region Evaluations enum
        /// <summary>
        /// The type of evaluation to perform.
        /// </summary>
        public enum Evaluation
        {
            /// <summary>
            /// Are the two values equal.
            /// </summary>
            Equal,

            /// <summary>
            /// Are the two values not equal.
            /// </summary>
            NotEqual,
        }
        #endregion
    }
}
