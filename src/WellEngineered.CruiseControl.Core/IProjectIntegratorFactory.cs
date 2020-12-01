
using WellEngineered.CruiseControl.Core.Queues;

namespace WellEngineered.CruiseControl.Core
{
    /// <summary>
    /// 	
    /// </summary>
	public interface IProjectIntegratorListFactory
	{
        /// <summary>
        /// Creates the project integrators.	
        /// </summary>
        /// <param name="projects">The projects.</param>
        /// <param name="integrationQueues">The integration queues.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		IProjectIntegratorList CreateProjectIntegrators(IProjectList projects, IntegrationQueueSet integrationQueues);
	}
}
