
using System.Collections;

namespace WellEngineered.CruiseControl.Core
{
    /// <summary>
    /// 	
    /// </summary>
	public class ProjectList : IProjectList
	{
		private Hashtable projects = new Hashtable();

        /// <summary>
        /// Adds the specified project.	
        /// </summary>
        /// <param name="project">The project.</param>
        /// <remarks></remarks>
		public void Add(IProject project)
		{
			this.projects[project.Name] = project;
		}

        /// <summary>
        /// Gets the <see cref="IProject" /> with the specified project name.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public IProject this[string projectName] 
		{ 
			get { return this.projects[projectName] as IProject; }
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.projects.Values.GetEnumerator();
		}

        /// <summary>
        /// Deletes the specified name.	
        /// </summary>
        /// <param name="name">The name.</param>
        /// <remarks></remarks>
		public void Delete(string name)
		{
			this.projects.Remove(name);
		}
	}
}
