﻿using System.Globalization;
using System.Text;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.Tasks
{
    /// <summary>
    /// A threshold for a coverage report.
    /// </summary>
    /// <title>Coverage Threshold</title>
    /// <version>1.5</version>
    [ReflectorType("coverageThreshold")]
    public class CoverageThreshold
    {
        #region Constructors
        /// <summary>
        /// Initialise a new <see cref="CoverageThreshold"/>.
        /// </summary>
        public CoverageThreshold()
        {
            this.ItemType = NCoverItemType.Default;
        }
        #endregion

        #region Public properties
        #region Metric
        /// <summary>
        /// The coverage metric.
        /// </summary>
        /// <version>1.5</version>
        /// <default>n/a</default>
        [ReflectorProperty("metric")]
        public NCoverMetric Metric { get; set; }
        #endregion

        #region MinValue
        /// <summary>
        /// The minimum coverage value.
        /// </summary>
        /// <version>1.5</version>
        /// <default>None</default>
        [ReflectorProperty("value", Required = false)]
        public int MinValue { get; set; }
        #endregion

        #region ItemType
        /// <summary>
        /// The type of item.
        /// </summary>
        /// <version>1.5</version>
        /// <default>None</default>
        [ReflectorProperty("type", Required = false)]
        public NCoverItemType ItemType { get; set; }
        #endregion

        #region Pattern
        /// <summary>
        /// The matching pattern to use.
        /// </summary>
        /// <version>1.5</version>
        /// <default>None</default>
        [ReflectorProperty("pattern", Required = false)]
        public string Pattern { get; set; }
        #endregion
        #endregion

        #region Public methods
        #region ToParamString()
        /// <summary>
        /// Returns a string that can be used an a parameter to the application.
        /// </summary>
        /// <returns></returns>
        public string ToParamString()
        {
            var builder = new StringBuilder();
            builder.Append(this.Metric);
            if (this.MinValue >= 0)
            {
                builder.AppendFormat(CultureInfo.CurrentCulture, ":{0}", this.MinValue);
                if ((this.ItemType != NCoverItemType.Default) || !string.IsNullOrEmpty(this.Pattern))
                {
                    builder.AppendFormat(CultureInfo.CurrentCulture, ":{0}", this.ItemType == NCoverItemType.Default ? NCoverItemType.Default : this.ItemType);
                    if (!string.IsNullOrEmpty(this.Pattern)) builder.AppendFormat(CultureInfo.CurrentCulture, ":{0}", this.Pattern);
                }
            }
            return builder.ToString();
        }
        #endregion
        #endregion

        #region Enumerations
        #region NCoverMetric
        /// <summary>
        /// The coverage metrics.
        /// </summary>
        public enum NCoverMetric
        {
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
            SymbolCoverage,
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
            BranchCoverage,
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
            MethodCoverage,
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
            CyclomaticComplexity
        }
        #endregion

        #region NCoverItemType
        /// <summary>
        /// The item types.
        /// </summary>
        public enum NCoverItemType
        {
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
            Default,
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
            View,
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
            Module,
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
            Namespace,
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
            Class
        }
        #endregion
        #endregion
    }
}
