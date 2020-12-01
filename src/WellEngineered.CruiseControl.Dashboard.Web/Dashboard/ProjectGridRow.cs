using System;
using System.Drawing;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.WebDashboard.Resources;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
    public class ProjectGridRow
    {
        private readonly ProjectStatus status;
        private readonly IServerSpecifier serverSpecifier;
        private readonly string url;
        private readonly string parametersUrl;

        public ProjectGridRow(ProjectStatus status, IServerSpecifier serverSpecifier,
            string url, string parametersUrl, Translations translations)
        {
            this.status = status;
            this.serverSpecifier = serverSpecifier;
            this.url = url;
            this.parametersUrl = parametersUrl;
        }

        public string Name
        {
            get { return this.status.Name; }
        }

        public string Description
        {
            get
            {
                if (String.IsNullOrEmpty(this.status.Description)) return "";

                return this.status.Description;
            }
        }


        public string ServerName
        {
            get { return this.serverSpecifier.ServerName; }
        }

        public string Category
        {
            get { return this.status.Category; }
        }

        public string BuildStatus
        {
            get { return this.status.BuildStatus.ToString(); }
        }

        public string BuildStatusHtmlColor
        {
            get { return this.CalculateHtmlColor(this.status.BuildStatus); }
        }

        public string LastBuildDate
        {
            get { return DateUtil.FormatDate(this.status.LastBuildDate); }
        }

        public string NextBuildTime
        {
            get
            {
                if (this.status.NextBuildTime == System.DateTime.MaxValue)
                {
                    return "Force Build Only";
                }
                else
                {
                    return DateUtil.FormatDate(this.status.NextBuildTime);
                }
            }
        }

        public string LastBuildLabel
        {
            get { return (this.status.LastBuildLabel != null ? this.status.LastBuildLabel : "no build available"); }
        }

        public string Status
        {
            get { return this.status.Status.ToString(); }
        }

        public string Activity
        {
            get { return this.status.Activity.ToString(); }
        }

        public string CurrentMessage
        {
            get { return this.status.CurrentMessage; }
        }

        public string Breakers
        {
            get
            {
                return this.GetMessageText(Message.MessageKind.Breakers);
            }
        }

        public string FailingTasks
        {
            get
            {
                return this.GetMessageText(Message.MessageKind.FailingTasks);
            }
        }

        public string Fixer
        {
            get
            {
                return this.GetMessageText(Message.MessageKind.Fixer);
            }
        }

        public string Url
        {
            get { return this.url; }
        }


        public string Queue
        {
            get { return this.status.Queue; }
        }


        public int QueuePriority
        {
            get { return this.status.QueuePriority; }
        }


        public string StartStopButtonName
        {
            get { return (this.status.Status == ProjectIntegratorState.Running) ? "StopBuild" : "StartBuild"; }
        }

        public string StartStopButtonValue
        {
            get { return (this.status.Status == ProjectIntegratorState.Running) ? "Stop" : "Start"; }
        }

        public string ForceAbortBuildButtonName
        {
            get { return (this.status.Activity != ProjectActivity.Building) ? "ForceBuild" : "AbortBuild"; }
        }

        public string ForceAbortBuildButtonValue
        {
            get { return (this.status.Activity != ProjectActivity.Building) ? "Force" : "Abort"; }
        }

        public bool AllowForceBuild
        {
            get { return this.serverSpecifier.AllowForceBuild && this.status.ShowForceBuildButton; }
        }

        public bool AllowStartStopBuild
        {
            get { return this.serverSpecifier.AllowStartStopBuild && this.status.ShowStartStopButton; }
        }

        private string CalculateHtmlColor(IntegrationStatus integrationStatus)
        {
            if (integrationStatus == IntegrationStatus.Success)
            {
                return Color.Green.Name;
            }
            else if (integrationStatus == IntegrationStatus.Unknown)
            {
                return Color.Blue.Name;
            }
            else
            {
                return Color.Red.Name;
            }
        }

        public string BuildStage
        {
            get
            {
                string CurrentBuildStage = this.status.BuildStage;

                if (CurrentBuildStage.Length == 0)
                {
                    return string.Empty;
                }
                else
                {
                    return CurrentBuildStage;
                }
            }
        }

        public string ParametersUrl
        {
            get { return this.parametersUrl; }
        }


        private string GetMessageText(Message.MessageKind messageType)
        {
            foreach (Message m in this.status.Messages)
            {
                if (m.Kind == messageType)
                {
                    return m.Text;
                }
            }
            return string.Empty;

        }
    }
}
