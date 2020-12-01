#region Using directives

using System;
using System.Globalization;
using System.Xml;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

#endregion

namespace WellEngineered.CruiseControl.MSBuild
{
    /// <summary>
    /// Implements an XML logger for MSBuild.
    /// </summary>
    public class XmlLogger : Logger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLogger"/> class.
        /// </summary>
        public XmlLogger()
        { }

        private string outputPath;
        private XmlDocument outputDoc;
        private XmlElement currentElement;

        /// <summary>
        /// Initializes the logger by attaching events and parsing command line.
        /// </summary>
        /// <param name="eventSource">The event source.</param>
        public override void Initialize(IEventSource eventSource)
        {
            this.outputPath = this.Parameters;

            this.outputDoc = new XmlDocument();
            this.outputDoc.AppendChild(this.outputDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
            this.outputDoc.AppendChild(this.outputDoc.CreateElement(XmlLoggerElements.Build));
            this.currentElement = this.outputDoc.DocumentElement;

            eventSource.ErrorRaised += new BuildErrorEventHandler(this.eventSource_ErrorRaised);
            eventSource.WarningRaised += new BuildWarningEventHandler(this.eventSource_WarningRaised);

            eventSource.BuildStarted += new BuildStartedEventHandler(this.eventSource_BuildStartedHandler);
            eventSource.BuildFinished += new BuildFinishedEventHandler(this.eventSource_BuildFinishedHandler);

            if (this.Verbosity != LoggerVerbosity.Quiet) // minimal and above
            {
                eventSource.MessageRaised += new BuildMessageEventHandler(this.eventSource_MessageHandler);
                eventSource.CustomEventRaised += new CustomBuildEventHandler(this.eventSource_CustomBuildEventHandler);

                eventSource.ProjectStarted += new ProjectStartedEventHandler(this.eventSource_ProjectStartedHandler);
                eventSource.ProjectFinished += new ProjectFinishedEventHandler(this.eventSource_ProjectFinishedHandler);

                if (this.Verbosity != LoggerVerbosity.Minimal) // normal and above
                {
                    eventSource.TargetStarted += new TargetStartedEventHandler(this.eventSource_TargetStartedHandler);
                    eventSource.TargetFinished += new TargetFinishedEventHandler(this.eventSource_TargetFinishedHandler);

                    if (this.Verbosity != LoggerVerbosity.Normal) // only detailed and diagnostic
                    {
                        eventSource.TaskStarted += new TaskStartedEventHandler(this.eventSource_TaskStartedHandler);
                        eventSource.TaskFinished += new TaskFinishedEventHandler(this.eventSource_TaskFinishedHandler);
                    }
                }
            }
        }

        public override void Shutdown()
        {
            if (String.IsNullOrEmpty(this.outputPath))
            {
                // Output to console.
                Console.WriteLine(this.outputDoc.OuterXml);
            }
            else
            {
                this.outputDoc.Save(this.outputPath);
            }
        }

        #region Event Handlers

        private void eventSource_BuildStartedHandler(object sender, BuildStartedEventArgs e)
        {
            this.LogStageStarted(XmlLoggerElements.Build, "", "", e.Timestamp);
        }

        private void eventSource_BuildFinishedHandler(object sender, BuildFinishedEventArgs e)
        {
            this.LogStageFinished(e.Succeeded, e.Timestamp);
        }

        private void eventSource_ProjectStartedHandler(object sender, ProjectStartedEventArgs e)
        {
            this.LogStageStarted(XmlLoggerElements.Project, e.TargetNames, e.ProjectFile, e.Timestamp);
        }

        private void eventSource_ProjectFinishedHandler(object sender, ProjectFinishedEventArgs e)
        {
            this.LogStageFinished(e.Succeeded, e.Timestamp);
        }

        private void eventSource_TargetStartedHandler(object sender, TargetStartedEventArgs e)
        {
            this.LogStageStarted(XmlLoggerElements.Target, e.TargetName, "", e.Timestamp);
        }

        private void eventSource_TargetFinishedHandler(object sender, TargetFinishedEventArgs e)
        {
            this.LogStageFinished(e.Succeeded, e.Timestamp);
        }

        private void eventSource_TaskStartedHandler(object sender, TaskStartedEventArgs e)
        {
            this.LogStageStarted(XmlLoggerElements.Task, e.TaskName, e.ProjectFile, e.Timestamp);
        }

        private void eventSource_TaskFinishedHandler(object sender, TaskFinishedEventArgs e)
        {
            this.LogStageFinished(e.Succeeded, e.Timestamp);
        }

        private void eventSource_WarningRaised(object sender, BuildWarningEventArgs e)
        {
            this.LogErrorOrWarning(XmlLoggerElements.Warning, e.Message, e.Code, e.File, e.LineNumber, e.ColumnNumber, e.Timestamp);
        }

        private void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            this.LogErrorOrWarning(XmlLoggerElements.Error, e.Message, e.Code, e.File, e.LineNumber, e.ColumnNumber, e.Timestamp);
        }

        private void eventSource_MessageHandler(object sender, BuildMessageEventArgs e)
        {
            this.LogMessage(XmlLoggerElements.Message, e.Message, e.Importance, e.Timestamp);
        }

        private void eventSource_CustomBuildEventHandler(object sender, CustomBuildEventArgs e)
        {
            this.LogMessage(XmlLoggerElements.Custom, e.Message, MessageImportance.Normal, e.Timestamp);
        }

        #endregion

        #region Logging

        private void LogStageStarted(string elementName, string stageName, string file, DateTime timeStamp)
        {
            // use the default root for the build element
            if (elementName != XmlLoggerElements.Build)
            {
                XmlElement stageElement = this.outputDoc.CreateElement(elementName);
                this.currentElement.AppendChild(stageElement);
                this.currentElement = stageElement;
            }

            this.SetAttribute(this.currentElement, stageName, XmlLoggerAttributes.Name);
            this.SetAttribute(this.currentElement, file, XmlLoggerAttributes.File);
            this.SetAttribute(this.currentElement, timeStamp, XmlLoggerAttributes.StartTime);

        }

        private void LogStageFinished(bool succeeded, DateTime timeStamp)
        {
            DateTime startTime = DateTime.Parse(this.currentElement.GetAttribute(XmlLoggerAttributes.StartTime), DateTimeFormatInfo.InvariantInfo);
            this.SetAttribute(this.currentElement, timeStamp - startTime, XmlLoggerAttributes.ElapsedTime);
            this.SetAttribute(this.currentElement, (int)(timeStamp - startTime).TotalSeconds, XmlLoggerAttributes.ElapsedSeconds);

            this.SetAttribute(this.currentElement, succeeded, XmlLoggerAttributes.Success);

            if (this.currentElement.ParentNode is XmlElement)
            {
                XmlElement parentElement = (XmlElement)this.currentElement.ParentNode;

                // don't put element's that don't contain any messages
                if (!this.currentElement.HasChildNodes && this.Verbosity != LoggerVerbosity.Detailed && this.Verbosity != LoggerVerbosity.Diagnostic)
                    parentElement.RemoveChild(this.currentElement);

                this.currentElement = parentElement;
            }
        }

        private void LogErrorOrWarning(string messageType, string message, string code, string file, int lineNumber, int columnNumber, DateTime timestamp)
        {
            XmlElement messageElement = this.outputDoc.CreateElement(messageType);
            this.SetAttribute(messageElement, code, XmlLoggerAttributes.Code);

            this.SetAttribute(messageElement, file, XmlLoggerAttributes.File);
            this.SetAttribute(messageElement, lineNumber, XmlLoggerAttributes.LineNumber);
            this.SetAttribute(messageElement, columnNumber, XmlLoggerAttributes.ColumnNumber);

            this.SetAttribute(messageElement, timestamp, XmlLoggerAttributes.TimeStamp);

            // Escape < and > if this is not a "Properties" message.  This is because in a Properties
            // message, we want the ability to insert legal XML, but otherwise we can get malformed
            // XML that will cause the parser to fail.
            this.WriteMessage(messageElement, message, code != "Properties");

            this.currentElement.AppendChild(messageElement);
        }

        private void LogMessage(string messageType, string message, MessageImportance importance, DateTime timestamp)
        {
            if (importance == MessageImportance.Low
                && this.Verbosity != LoggerVerbosity.Detailed
                && this.Verbosity != LoggerVerbosity.Diagnostic)
                return;

            if (importance == MessageImportance.Normal
                && (this.Verbosity == LoggerVerbosity.Minimal
                    || this.Verbosity == LoggerVerbosity.Quiet))
                return;

            XmlElement messageElement = this.outputDoc.CreateElement(messageType);

            this.SetAttribute(messageElement, importance, XmlLoggerAttributes.Importance);

            if (this.Verbosity == LoggerVerbosity.Diagnostic)
                this.SetAttribute(messageElement, timestamp, XmlLoggerAttributes.TimeStamp);

            this.WriteMessage(messageElement, message, false);

            this.currentElement.AppendChild(messageElement);
        }

        private void WriteMessage(XmlElement element, string message, bool escapeLtGt)
        {
            if (!string.IsNullOrEmpty(message))
            {
                string temp = message.Replace("&", "&amp;");

                if (escapeLtGt)
                {
                    temp = temp.Replace("<", "&lt;");
                    temp = temp.Replace(">", "&gt;");
                }
                element.AppendChild(this.outputDoc.CreateCDataSection(temp));
            }
        }

        private void SetAttribute(XmlElement element, object obj, string name)
        {
            if (obj == null)
                return;

            Type t = obj.GetType();
            if (t == typeof(int))
            {
                element.SetAttribute(name, obj.ToString());
            }
            else if (t == typeof(DateTime))
            {
                DateTime dateTime = (DateTime)obj;
                element.SetAttribute(name, dateTime.ToString("G", DateTimeFormatInfo.InvariantInfo));
            }
            else if (t == typeof(TimeSpan))
            {
                double seconds = ((TimeSpan)obj).TotalSeconds;
                TimeSpan whole = TimeSpan.FromSeconds(Math.Truncate(seconds));
                element.SetAttribute(name, whole.ToString());
            }
            else if (t == typeof(Boolean))
            {
                element.SetAttribute(name, obj.ToString().ToLower());
            }
            else if (t == typeof(MessageImportance))
            {
                MessageImportance importance = (MessageImportance)obj;
                element.SetAttribute(name, importance.ToString().ToLower());
            }
            else
            {
                if (obj.ToString().Length > 0)
                {
                    element.SetAttribute(name, obj.ToString());
                }
            }
        }

        #endregion

        #region Constants

        internal sealed class XmlLoggerElements
        {
            private XmlLoggerElements()
            { }

            public const string Build = "msbuild";
            public const string Error = "error";
            public const string Warning = "warning";
            public const string Message = "message";
            public const string Project = "project";
            public const string Target = "target";
            public const string Task = "task";
            public const string Custom = "custom";
        }

        internal sealed class XmlLoggerAttributes
        {
            private XmlLoggerAttributes()
            { }

            public const string Name = "name";
            public const string File = "file";
            public const string StartTime = "startTime";
            public const string ElapsedTime = "elapsedTime";
            public const string ElapsedSeconds = "elapsedSeconds";
            public const string TimeStamp = "timeStamp";
            public const string Code = "code";
            public const string LineNumber = "line";
            public const string ColumnNumber = "column";
            public const string Importance = "level";
            public const string Processor = "processor";
            public const string HelpKeyword = "help";
            public const string SubCategory = "category";
            public const string Success = "success";
        }

        #endregion
    }
}
