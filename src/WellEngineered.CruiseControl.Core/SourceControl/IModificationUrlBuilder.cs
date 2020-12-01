using System.ComponentModel;

namespace WellEngineered.CruiseControl.Core.SourceControl
{
    /// <summary>
    /// A builder to convert URLs within modifications into links.
    /// </summary>
    /// <title>IssueUrlBuilder</title>
	[TypeConverter(typeof (ExpandableObjectConverter))]
	public interface IModificationUrlBuilder
	{
        /// <summary>
        /// Setups the modification.	
        /// </summary>
        /// <param name="modifications">The modifications.</param>
        /// <remarks></remarks>
		void SetupModification(Modification[] modifications);
	}
}