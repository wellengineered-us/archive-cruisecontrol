using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core.Tasks
{
    /// <summary>
    /// 	
    /// </summary>
	public class NUnitArgument
	{
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public  string[] assemblies;
		private readonly string outputfile;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public string[] IncludedCategories;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public string[] ExcludedCategories;

        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitArgument" /> class.	
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="outputfile">The outputfile.</param>
        /// <remarks></remarks>
		public NUnitArgument(string[] assemblies, string outputfile)
		{
			if (assemblies == null || assemblies.Length == 0)
				throw new CruiseControlException(
					"No unit test assemblies are specified. Please use the <assemblies> element to specify the test assemblies to run.");

			this.assemblies = assemblies;
			this.outputfile = outputfile;
		}

        /// <summary>
        /// Toes the string.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override string ToString()
		{
			ProcessArgumentBuilder argsBuilder = new ProcessArgumentBuilder();
			argsBuilder.AddArgument("/xml", "=", this.outputfile);
			argsBuilder.AddArgument("/nologo");
            this.AppendCategoriesArg(argsBuilder);

			foreach (string assemblyName in this.assemblies)
			{
				argsBuilder.AddArgument(assemblyName);
			}
			return argsBuilder.ToString();
		}

        /// <summary>
        /// Appends the categories, with value not an empty string nor a whitespace,
        /// to the excluded or included categories lists.
        /// </summary>
        /// <param name="argsBuilder">The args builder.</param>
        private void AppendCategoriesArg(ProcessArgumentBuilder argsBuilder)
        {
            if (this.ExcludedCategories != null && this.ExcludedCategories.Length != 0)
            {
                string[] excludedCategories = System.Array.FindAll(this.ExcludedCategories, IsNotWhitespace);
                argsBuilder.AddArgument("/exclude", "=", string.Join(",", excludedCategories));
            }
            if (this.IncludedCategories != null && this.IncludedCategories.Length != 0)
            {
                string[] includedCategories = System.Array.FindAll(this.IncludedCategories, IsNotWhitespace);
                argsBuilder.AddArgument("/include", "=", string.Join(",", includedCategories));
            }
        }

        /// <summary>
        /// Determines whether the specified input is not white space.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>
        /// 	<c>true</c> if the specified input is not white space; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsNotWhitespace(string input)
        {
            return !StringUtil.IsWhitespace(input);
        }
	}
}
