using System;
using System.IO;
using System.Xml;

using WellEngineered.CruiseControl.Core.SourceControl;
using WellEngineered.CruiseControl.Core.Tasks;
using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core.Publishers
{
    /// <summary>
    /// 	
    /// </summary>
    public class XmlIntegrationResultWriter : IDisposable
    {
        private XmlFragmentWriter writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlIntegrationResultWriter" /> class.	
        /// </summary>
        /// <param name="textWriter">The text writer.</param>
        /// <remarks></remarks>
        public XmlIntegrationResultWriter(TextWriter textWriter)
        {
            this.writer = new XmlFragmentWriter(textWriter);
        }

        /// <summary>
        /// Writes the specified result.	
        /// </summary>
        /// <param name="result">The result.</param>
        /// <remarks></remarks>
        public void Write(IIntegrationResult result)
        {
            this.writer.WriteStartElement(Elements.CRUISE_ROOT);
            this.writer.WriteAttributeString("project", result.ProjectName);
            this.WriteRequest(result.IntegrationRequest);
            this.WriteModifications(result.Modifications);
            this.WriteIntegrationProperties(result);
            this.WriteBuildElement(result);
            this.WriteException(result);
            this.writer.WriteEndElement();
        }

        private void WriteRequest(IntegrationRequest request)
        {
            if (request == null) return;
            this.writer.WriteStartElement(Elements.Request);
            this.writer.WriteAttributeString("source", request.Source);
            this.writer.WriteAttributeString("buildCondition", request.BuildCondition.ToString());
            this.writer.WriteString(request.ToString());
            this.writer.WriteEndElement();

            // Output the parameters
            if ((request.BuildValues != null) && (request.BuildValues.Count > 0))
            {
                this.writer.WriteStartElement(Elements.Parameters);
                foreach (string key in request.BuildValues.Keys)
                {
                    this.writer.WriteStartElement(Elements.Parameter);
                    this.writer.WriteAttributeString("name", key);
                    this.writer.WriteAttributeString("value", request.BuildValues[key]);
                    this.writer.WriteEndElement();
                }
                this.writer.WriteEndElement();
            }
        }

        private void WriteTaskResults(IIntegrationResult result)
        {
            foreach (ITaskResult taskResult in result.TaskResults)
            {
                this.WriteOutput(taskResult.Data);

                var fileTaskResult = taskResult as FileTaskResult;
                if(fileTaskResult != null)
                {
                    // check whether the file should be deleted after merging
                    if(fileTaskResult.DeleteAfterMerge)
                    {
                        Log.Info("Delete merged file '{0}'.", fileTaskResult.File.FullName);
                        //TODO: enqueue for deleting
                    }
                }
            }
        }

        /// <summary>
        /// Writes the build element.	
        /// </summary>
        /// <param name="result">The result.</param>
        /// <remarks></remarks>
        public void WriteBuildElement(IIntegrationResult result)
        {
            this.writer.WriteStartElement(Elements.BUILD);
            this.writer.WriteAttributeString("date", DateUtil.FormatDate(result.StartTime));

            // hide the milliseconds
            TimeSpan time = result.TotalIntegrationTime;
            this.writer.WriteAttributeString("buildtime", string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0:d2}:{1:d2}:{2:d2}", time.Hours, time.Minutes, time.Seconds));
            if (result.Failed)
            {
                this.writer.WriteAttributeString("error", "true");
            }
            this.writer.WriteAttributeString("buildcondition", result.BuildCondition.ToString());
            this.WriteTaskResults(result);
            this.writer.WriteEndElement();
        }

        private void WriteOutput(string output)
        {
            this.writer.WriteNode(output);
        }

        private void WriteException(IIntegrationResult result)
        {
            if (result.ExceptionResult == null)
            {
                return;
            }

            this.writer.WriteStartElement(Elements.EXCEPTION);
            this.writer.WriteCData(result.ExceptionResult.ToString());
            this.writer.WriteEndElement();
        }

        void IDisposable.Dispose()
        {
            this.writer.Close();
        }

        /// <summary>
        /// Writes the modifications.	
        /// </summary>
        /// <param name="mods">The mods.</param>
        /// <remarks></remarks>
        public void WriteModifications(Modification[] mods)
        {
            this.writer.WriteStartElement(Elements.MODIFICATIONS);
            if (mods == null)
            {
                return;
            }
            foreach (Modification mod in mods)
            {
                mod.ToXml(this.writer);
            }
            this.writer.WriteEndElement();
        }

        private void WriteIntegrationProperties(IIntegrationResult result)
        {

            this.writer.WriteStartElement(Elements.IntegrationProps);
            
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetArtifactDirectory],
                                                            IntegrationPropertyNames.CCNetArtifactDirectory);
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetBuildCondition],
                                                            IntegrationPropertyNames.CCNetBuildCondition);
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetBuildDate],
                                                            IntegrationPropertyNames.CCNetBuildDate);
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetBuildTime],
                                                            IntegrationPropertyNames.CCNetBuildTime);
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetFailureUsers],
                                                            IntegrationPropertyNames.CCNetFailureUsers);
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetFailureTasks],
                                                            IntegrationPropertyNames.CCNetFailureTasks, "task");
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetIntegrationStatus],
                                                            IntegrationPropertyNames.CCNetIntegrationStatus);
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetLabel],
                                                            IntegrationPropertyNames.CCNetLabel);
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetLastIntegrationStatus],
                                                            IntegrationPropertyNames.CCNetLastIntegrationStatus);
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetListenerFile],
                                                            IntegrationPropertyNames.CCNetListenerFile);
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetModifyingUsers],
                                                            IntegrationPropertyNames.CCNetModifyingUsers);
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetNumericLabel],
                                                            IntegrationPropertyNames.CCNetNumericLabel);
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetProject],
                                                            IntegrationPropertyNames.CCNetProject);
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetProjectUrl],
                                                            IntegrationPropertyNames.CCNetProjectUrl);
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetRequestSource],
                                                            IntegrationPropertyNames.CCNetRequestSource);
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetWorkingDirectory],
                                                            IntegrationPropertyNames.CCNetWorkingDirectory);
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetUser],
                                                            IntegrationPropertyNames.CCNetUser);
            this.WriteIntegrationProperty(result.IntegrationProperties[IntegrationPropertyNames.CCNetBuildId],
                                                            IntegrationPropertyNames.CCNetBuildId);
            this.WriteIntegrationProperty(result.LastChangeNumber, "LastChangeNumber");
            this.WriteIntegrationProperty(result.LastIntegrationStatus, "LastIntegrationStatus");
            this.WriteIntegrationProperty(result.LastSuccessfulIntegrationLabel, "LastSuccessfulIntegrationLabel");
            this.WriteIntegrationProperty(result.LastModificationDate, "LastModificationDate");

            var buildParameters = NameValuePair.ToDictionary(result.Parameters);
            if (buildParameters.ContainsKey("CCNetForceBuildReason"))
            {
                this.WriteIntegrationProperty(buildParameters["CCNetForceBuildReason"], "CCNetForceBuildReason");
            }

            this.writer.WriteEndElement();
        }

        private void WriteIntegrationProperty(object value, string propertyName)
        {
            this.WriteIntegrationProperty(value, propertyName, "user");
        }

        private void WriteIntegrationProperty(object value, string propertyName, string arrayElementName)
        {
            if (value == null) return ;


            this.writer.WriteStartElement(propertyName);

            if ((value is string) || (value is int) || (value is Enum) || value is DateTime)
            {
                this.writer.WriteString(value.ToString());
            }
            else
            {
                if (value is Guid)
                {
                    var x = (Guid)value;
                    this.writer.WriteString(x.ToString("N"));
                }
                else
                {
                    var dummy = value as System.Collections.ArrayList;
                    if (dummy != null)
                    {
                        string[] tmp = (string[])dummy.ToArray(typeof(string));

                        foreach (string s in tmp)
                        {
                            this.WriteIntegrationProperty(s, arrayElementName);
                        }
                    }
                    else
                    {
                        throw new ArgumentException(
                                        string.Format(System.Globalization.CultureInfo.CurrentCulture, "The IntegrationProperty type {0} is not supported yet", value.GetType()));
                    }

                }
            }
            this.writer.WriteEndElement();
        }


        private class Elements
        {
            private  Elements()
            {}

            public const string BUILD = "build";
            public const string CRUISE_ROOT = "cruisecontrol";
            public const string MODIFICATIONS = "modifications";
            public const string EXCEPTION = "exception";
            public const string Request = "request";
            public const string Parameters = "parameters";
            public const string Parameter = "parameter";
            public const string IntegrationProps = "integrationProperties";
        }

        /// <summary>
        /// Sets the formatting.	
        /// </summary>
        /// <value>The formatting.</value>
        /// <remarks></remarks>
        public Formatting Formatting
        {
            set { this.writer.Formatting = value; }
        }
    }
}