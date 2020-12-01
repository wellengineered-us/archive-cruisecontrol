﻿using System;

namespace WellEngineered.CruiseControl.Core.Util
{
    /// <summary>
    /// Handles the shadow copying of files.
    /// </summary>
    public interface IShadowCopier
    {
        #region Methods
        #region RetrieveFilePath()
        /// <summary>
        /// Retrieves the path to a file that has been shadow copied.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The full path to the shadow copied file, if it exists, null otherwise.</returns>
        string RetrieveFilePath(string fileName);
        #endregion
        #endregion
    }
}
