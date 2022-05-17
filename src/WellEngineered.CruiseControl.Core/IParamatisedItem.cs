using System.Collections.Generic;

using WellEngineered.CruiseControl.Remote.Parameters;

namespace WellEngineered.CruiseControl.Core
{
    /// <summary>
    /// Marks an item as having input parameters.
    /// </summary>
    public interface IParamatisedItem
    {
        #region Public methods
        #region ApplyParameters()
        /// <summary>
        /// Applies the input parameters to the item.
        /// </summary>
        /// <param name="parameters">The parameters to apply.</param>
        /// <param name="parameterDefinitions">The original parameter definitions.</param>
        void ApplyParameters(Dictionary<string, string> parameters, IEnumerable<ParameterBase> parameterDefinitions);
        #endregion
        #endregion
    }
}
