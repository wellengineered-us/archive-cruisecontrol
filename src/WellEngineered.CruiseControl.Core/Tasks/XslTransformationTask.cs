

using System;
using System.Collections;
using System.IO;
using System.Text;

using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core.Tasks
{
	/// <summary>
    /// <para>
    /// The XSL Transformation Task is a task that allows to do XSL transformation during the build.
    /// </para>
    /// </summary>
    /// <title>XSL Transformation Task</title>
    /// <version>1.7</version>
    /// <example>
    /// <code title="Simple example">
    /// &lt;xslt&gt;
    /// &lt;xmlfile&gt;XMLFile&lt;/xmlfile&gt;
    /// &lt;xslfile&gt;XSLFile&lt;/xslfile&gt;
    /// &lt;outputfile&gt;OutputFile&lt;/outputfile&gt;
    /// &lt;/xslt&gt;
    /// </code>
    /// <code title="Full example">
    /// &lt;xslt&gt;
    /// &lt;xmlfile&gt;XMLFile&lt;/xmlfile&gt;
    /// &lt;xslfile&gt;XSLFile&lt;/xslfile&gt;
    /// &lt;outputfile&gt;OutputFile&lt;/outputfile&gt;
    /// &lt;xsltArgs&gt;&lt;namedValue name="BuildDate" value="$[$CCNetBuildDate]" /&gt;&lt;/xsltArgs&gt;
    /// &lt;/xslt&gt;
    /// </code>
    /// </example>
    [ReflectorType("xslt")]
    public class XslTransformationTask : TaskBase
    {
        private ITransformer transformer;
        private IFileSystem fileSystem;

        /// <summary>
        /// The name of the XML file that acts as the source of the transformation.
        /// </summary>
        /// <default>None</default>
        /// <version>1.7</version>
        [ReflectorProperty("xmlfile", Required = true)]
        public string XMLFile { get; set; }

        /// <summary>
        /// The name of the XSL file that is used to apply the transformation.
        /// </summary>
        /// <default>None</default>
        /// <version>1.7</version>
        [ReflectorProperty("xslfile", Required = true)]
        public string XSLFile { get; set; }

        /// <summary>
        /// The name of the output file into which the transformation result will be written
        /// </summary>
        /// <default>None</default>
        /// <version>1.7</version>
        [ReflectorProperty("outputfile", Required = true)]
        public string OutputFile { get; set; }

        /// <summary>
        /// The arguments to give to the XSL transformation.
        /// You could use this to send an integration property (build date, build time, reason)
        /// to the XSL stylesheet so that it can use it to generate the output.
        /// Please see http://msdn.microsoft.com/en-us/library/dfktf882.aspx for detailed usage informations
        /// </summary>
        /// <version>1.7</version>
        /// <default>n/a</default>
        [ReflectorProperty("xsltArgs", Required = false)]
        public NameValuePair[] XsltArgs { get; set; }

        public XslTransformationTask() : this(new XslTransformer(), new SystemIoFileSystem()) { }

        public XslTransformationTask(ITransformer transformer, IFileSystem fileSystem) 
        {
            this.transformer = transformer;
            this.fileSystem = fileSystem;
            this.XsltArgs = new NameValuePair[] {};
        }

        /// <summary>
        /// Executes the specified result.	
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override bool Execute(IIntegrationResult result)
        {
            result.BuildProgressInformation.SignalStartRunTask(!string.IsNullOrEmpty(this.Description) ? this.Description : "Transforming");

            string input = null;
            using (Stream inputStream = this.fileSystem.OpenInputStream(this.XMLFile))
            {
                using (StreamReader reader = new StreamReader(inputStream))
                {
                    input = reader.ReadToEnd();
                }
            }

            Hashtable table = new Hashtable();
            foreach (NameValuePair pair in this.XsltArgs)
                table.Add(pair.Name, pair.Value);

            string output = this.transformer.Transform(input, this.XSLFile, table);

            using (Stream outputStream = this.fileSystem.OpenOutputStream(this.OutputFile))
            {
                using (StreamWriter writer = new StreamWriter(outputStream, Encoding.UTF8))
                {
                    writer.Write(output);
                }
            }
            return true;
        }
    }
}
