// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Nouns.cs" company="The CruiseControl.NET Team">
//   Copyright (C) 2011 by The CruiseControl.NET Team
// 
//   Permission is hereby granted, free of charge, to any person obtaining a copy
//   of this software and associated documentation files (the "Software"), to deal
//   in the Software without restriction, including without limitation the rights
//   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//   copies of the Software, and to permit persons to whom the Software is
//   furnished to do so, subject to the following conditions:
//   
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WellEngineered.CruiseControl.PowerShell.Cmdlets
{
    /// <summary>
    /// Defines the common nouns.
    /// </summary>
    public static class Nouns
    {
        #region Public constants
        /// <summary>
        /// A connection to a CruiseControl.NET server.
        /// </summary>
        public const string Connection = "CCConnection";

        /// <summary>
        /// A project.
        /// </summary>
        public const string Project = "CCProject";

        /// <summary>
        /// A queue.
        /// </summary>
        public const string Queue = "CCQueue";

        /// <summary>
        /// A build for a project.
        /// </summary>
        public const string Build = "CCBuild";

        /// <summary>
        /// A log from the server.
        /// </summary>
        public const string Log = "CCLog";

        /// <summary>
        /// A package in a project.
        /// </summary>
        public const string Package = "CCPackage";
        #endregion
    }
}
