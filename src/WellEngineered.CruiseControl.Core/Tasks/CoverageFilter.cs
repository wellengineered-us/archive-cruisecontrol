﻿using System.Globalization;
using System.Text;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.Tasks
{
    /// <summary>
    /// A filter for a coverage report.
    /// </summary>
    /// <title>Coverage Filter</title>
    /// <version>1.5</version>
    [ReflectorType("coverageFilter")]
    public class CoverageFilter
    {
        #region Constructors
        /// <summary>
        /// Initialise a new <see cref="CoverageFilter"/>.
        /// </summary>
        public CoverageFilter()
        {
            this.ItemType = NCoverItemType.Default;
        }
        #endregion

        #region Public properties
        #region Data
        /// <summary>
        /// The pattern to use for matching elements.
        /// </summary>
        /// <version>1.5</version>
        /// <default>n/a</default>
        [ReflectorProperty("data")]
        public string Data { get; set; }
        #endregion

        #region ItemType
        /// <summary>
        /// The type of item.
        /// </summary>
        /// <version>1.5</version>
        /// <default>Default</default>
        [ReflectorProperty("type", Required = false)]
        public NCoverItemType ItemType { get; set; }
        #endregion

        #region IsRegex
        /// <summary>
        /// Whether this is a regex or not.
        /// </summary>
        /// <version>1.5</version>
        /// <default>false</default>
        [ReflectorProperty("regex", Required = false)]
        public bool IsRegex { get; set; }
        #endregion

        #region IsInclude
        /// <summary>
        /// Whether to include or exclude items.
        /// </summary>
        /// <version>1.5</version>
        /// <default>false</default>
        [ReflectorProperty("include", Required = false)]
        public bool IsInclude { get; set; }
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
            builder.Append(this.Data);
            if (this.ItemType != NCoverItemType.Default)
            {
                builder.AppendFormat(CultureInfo.CurrentCulture, ":{0}", this.ItemType);
                if (this.IsRegex || this.IsInclude)
                {
                    builder.AppendFormat(CultureInfo.CurrentCulture, ":{0}", this.IsRegex ? "true" : "false");
                    builder.AppendFormat(CultureInfo.CurrentCulture, ":{0}", this.IsInclude ? "true" : "false");
                }
            }
            return builder.ToString();
        }
        #endregion
        #endregion

        #region Enumerations
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
            Assembly,
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
            Namespace,
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
            Class,
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
            Method,
            /// <summary>
            /// 	
            /// </summary>
            /// <remarks></remarks>
            Document
        }
        #endregion
        #endregion
    }
}
