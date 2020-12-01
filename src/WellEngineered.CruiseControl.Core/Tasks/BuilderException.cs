using System;

using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core.Tasks
{
    /// <summary>
    /// 	
    /// </summary>
	public class BuilderException : CruiseControlException
	{
		private readonly ITask _runner;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderException" /> class.	
        /// </summary>
        /// <param name="runner">The runner.</param>
        /// <param name="message">The message.</param>
        /// <remarks></remarks>
		public BuilderException(ITask runner, string message) 
			: base(message) 
		{
			this._runner = runner;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderException" /> class.	
        /// </summary>
        /// <param name="runner">The runner.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <remarks></remarks>
		public BuilderException(ITask runner, string message, Exception innerException) 
			: base(message, innerException)
		{
			this._runner = runner;
		}

        /// <summary>
        /// Gets the builder.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public ITask Builder
		{
			get { return this._runner; }
		}

        /// <summary>
        /// Toes the string.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override string ToString()
		{
			return base.ToString() + this._runner;
		}
	}
}
