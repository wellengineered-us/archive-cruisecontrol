using System;
using System.Globalization;
using System.IO;
using System.Text;

using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core.Util
{
    /// <summary>
    /// 	
    /// </summary>
    public class BuildProgressInformation
    {
        private string _listenerFile;
        private string _buildInformation = string.Empty;
        private DateTime _lastTimeQueried;
        private const Int32 buildStageCheckIntervalInSeconds = 5;
        private readonly object lockObject = new object();
        private System.Collections.Generic.List<BuildProgressInformationData> Progress;
        private const Int32 MaxItemsInQueue = 10;
        private OnStartupInformationUpdatedDelegate _OnStartupInformationUpdated = null;


        private void DoStartupInformationUpdated(string information)
        {
            this._lastTimeQueried = DateTime.Now.AddYears(-10);
            if (this._OnStartupInformationUpdated != null)
                this.OnStartupInformationUpdated(information, this.OnStartupInformationUpdatedUserObject);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BuildProgressInformation" /> class.	
        /// </summary>
        /// <param name="artifactDirectory">The artifact directory.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <remarks></remarks>
        public BuildProgressInformation(string artifactDirectory, string projectName)
        {
            this._listenerFile = System.IO.Path.Combine(artifactDirectory, StringUtil.RemoveInvalidCharactersFromFileName(projectName) + "_ListenFile.xml");
        }

        /// <summary>
        /// Returns the location of the listenerfile to be used by external programs
        /// </summary>
        public string ListenerFile
        {
            get { return this._listenerFile; }
        }


        /// <summary>
        /// Signals the start of a new task, so initialise all needed actions for monitoring this tasks progress
        /// </summary>
        /// <param name="information"></param>
        public virtual void SignalStartRunTask(string information)
        {
            lock (this.lockObject)
            {
                this.RemoveListenerFile();

                this.Progress = new System.Collections.Generic.List<BuildProgressInformationData>();
                this.AddToInternalQueue(information);

                this._buildInformation = this.GetQueueDataAsXml();
                this._lastTimeQueried = DateTime.Now.AddYears(-10);

                this.DoStartupInformationUpdated(information);
            }
        }

        public delegate void OnStartupInformationUpdatedDelegate(string information, object UserObject);
        public OnStartupInformationUpdatedDelegate OnStartupInformationUpdated { get { return this._OnStartupInformationUpdated; } set { this._OnStartupInformationUpdated += value; } }
        public object OnStartupInformationUpdatedUserObject { get; set; }

        /// <summary>
        /// Adds the task information.	
        /// </summary>
        /// <param name="information">The information.</param>
        /// <remarks></remarks>
        public virtual void AddTaskInformation(string information)
        {
            lock (this.lockObject)
            {
                this.AddToInternalQueue(information);
            }
        }


        public void UpdateStartupInformation(string information)
        {
            lock (this.lockObject)
            {
                this.Progress[0] = new BuildProgressInformationData(information);
                this.DoStartupInformationUpdated(information);
            }
        }

        /// <summary>
        /// Gets the build progress information.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual string GetBuildProgressInformation()
        {
            lock (this.lockObject)
            {
            if (DateTime.Now.AddSeconds(-buildStageCheckIntervalInSeconds) <= this._lastTimeQueried)
                return this._buildInformation;

            if (File.Exists(this._listenerFile))
            {
                try
                {
                    using (StreamReader fileReader = new StreamReader(this._listenerFile))
                    {
                        this._buildInformation = fileReader.ReadToEnd();
                    }
                }
                catch
                { }
            }
            else
            {
                this._buildInformation = this.GetQueueDataAsXml();
            }

            this._lastTimeQueried = DateTime.Now;
            return this._buildInformation;
            }
        }


        /// <summary>
        /// Deletes the listenerfile
        /// </summary>        
        private void RemoveListenerFile()
        {
            const int maxAmountOfRetries = 10;
            int retryCounter = 0;


            while (System.IO.File.Exists(this._listenerFile) && (retryCounter <= maxAmountOfRetries))
            {
                try
                {
                    System.IO.File.Delete(this._listenerFile);
                }
                catch (Exception e)
                {
                    retryCounter += 1;
                    System.Threading.Thread.Sleep(200);

                    if (retryCounter > maxAmountOfRetries)
                        throw new CruiseControlException(
                            string.Format(System.Globalization.CultureInfo.CurrentCulture,"Failed to delete {0} after {1} attempts", this._listenerFile, retryCounter), e);
                }
            }
        }
      
        private void AddToInternalQueue(string info)
        {
            this.Progress.Add(new BuildProgressInformationData(info));

            if (this.Progress.Count > MaxItemsInQueue) this.Progress.RemoveAt(1); // keep the first 1 because this contains the taks name (signal start run)

        }

        private string GetQueueDataAsXml()
        {
            System.Text.StringBuilder listenData = new StringBuilder();

            listenData.AppendLine("<data>");
            
            foreach( BuildProgressInformationData bpi in this.Progress)
            {
                listenData.AppendLine(
                    string.Format(System.Globalization.CultureInfo.CurrentCulture,"<Item Time=\"{0}\" Data=\"{1}\" />", bpi.At ?? string.Empty, bpi.Information ?? string.Empty));
            }

            listenData.AppendLine("</data>");

            return listenData.ToString();
        }


    }

    struct BuildProgressInformationData
    {
        private DateTime at;
        private string information;

        public string At
        {
            get { return this.at.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture); }
        }

        public string Information
        {
            get { return this.CleanUpMessageForXMLLogging(this.information); }
        }

        public BuildProgressInformationData(string info)
        {
            this.at = DateTime.Now;
            this.information = info;
        }

        private string CleanUpMessageForXMLLogging(string msg)
        {
            return msg.Replace("\"", string.Empty);
        }
    }


}



