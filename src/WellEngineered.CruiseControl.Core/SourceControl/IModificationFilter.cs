namespace WellEngineered.CruiseControl.Core.SourceControl
{
    /// <title>Modification Filters</title>
	public interface IModificationFilter
	{
        /// <summary>
        /// Accepts the specified modification.	
        /// </summary>
        /// <param name="modification">The modification.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		bool Accept(Modification modification);
	}
}