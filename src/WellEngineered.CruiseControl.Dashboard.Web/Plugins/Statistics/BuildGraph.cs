using System;
using System.Collections.Generic;

using WellEngineered.CruiseControl.Core;
using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.WebDashboard.Dashboard;
using WellEngineered.CruiseControl.WebDashboard.Plugins.BuildReport;
using WellEngineered.CruiseControl.WebDashboard.Resources;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.Statistics
{
    /// <summary>
	/// Provides functions for making a graph of the specified builds.
    /// These are HTML tables, so should not be browser specific.
	/// </summary>
    public class BuildGraph
    {
        private IBuildSpecifier[] mybuildSpecifiers;
        private ILinkFactory mylinkFactory;
        private Int32 myHighestAmountPerDay;

        private Int32 myOKBuildAmount;
        private Int32 myFailedBuildAmount;
        private Translations translations;
            
        public BuildGraph(IBuildSpecifier[] buildSpecifiers, ILinkFactory linkFactory, Translations translations)
        {
            this.mybuildSpecifiers = buildSpecifiers;
            this.mylinkFactory = linkFactory;
            this.translations = translations;
        }


        /// <summary>
        /// highest amount of builds found in 1 day for the entire graph
        /// used for calculating the height of the graph
        /// </summary>
        public Int32 HighestAmountPerDay
        {
            get
            {
                return this.myHighestAmountPerDay;
            }
        }


        /// <summary>
        /// Total amount of OK builds
        /// </summary>
        public Int32 AmountOfOKBuilds
        {
            get
            {
                return this.myOKBuildAmount;
            }
        }

        /// <summary>
        /// Total amount of failed builds
        /// </summary>
        public Int32 AmountOfFailedBuilds
        {
            get
            {
                return this.myFailedBuildAmount;
            }
        }

        public override bool Equals(object obj)
        {            
            if (obj.GetType() != this.GetType() )
                return false;

            BuildGraph Comparable = obj as BuildGraph;

            if (this.mybuildSpecifiers.Length != Comparable.mybuildSpecifiers.Length)
                {return false; }
        

            for (int i=0; i < this.mybuildSpecifiers.Length ; i++)
            {
                if (! this.mybuildSpecifiers[i].Equals(Comparable.mybuildSpecifiers[i]) )
                {return false; }
            }

            return true;
        }

		public override int GetHashCode()
		{
			int hashCode = 0;
			unchecked {
				if (this.mybuildSpecifiers != null) hashCode += 1000000007 * this.mybuildSpecifiers.GetHashCode(); 
				if (this.mylinkFactory != null) hashCode += 1000000009 * this.mylinkFactory.GetHashCode(); 
				hashCode += 1000000021 * this.myHighestAmountPerDay.GetHashCode();
				hashCode += 1000000033 * this.myOKBuildAmount.GetHashCode();
				hashCode += 1000000087 * this.myFailedBuildAmount.GetHashCode();
			}
			return hashCode;
		}
        
        ///<summary>
        /// Returns a sorted list containing build information per buildday
        ///</summary>
        public List<GraphBuildDayInfo> GetBuildHistory(Int32 maxAmountOfDays)
        {
            GraphBuildInfo currentBuildInfo;
            GraphBuildDayInfo currentBuildDayInfo;

            // adding the builds to a list per day
            var foundDates = new Dictionary<DateTime, GraphBuildDayInfo>();
            var dateSorter = new List<DateTime>();

            foreach (IBuildSpecifier buildSpecifier in this.mybuildSpecifiers)
            {           
                currentBuildInfo = new GraphBuildInfo(buildSpecifier, this.mylinkFactory);

                if (!foundDates.ContainsKey(currentBuildInfo.BuildDate()))
                {
                    foundDates.Add(currentBuildInfo.BuildDate(), new GraphBuildDayInfo(currentBuildInfo, this.translations) );
                    dateSorter.Add(currentBuildInfo.BuildDate());
                }
                else
                {
                    currentBuildDayInfo = foundDates[currentBuildInfo.BuildDate()];
                    currentBuildDayInfo.AddBuild(currentBuildInfo);

                    foundDates[currentBuildInfo.BuildDate()] = currentBuildDayInfo;
                }                            
            }
 
            //making a sorted list of the dates where we have builds of
            //and limit to the amount specified in maxAmountOfDays
            dateSorter.Sort();
            while (dateSorter.Count > maxAmountOfDays)
            {
                dateSorter.RemoveAt(0);
            }

            //making final sorted arraylist
            var result = new List<GraphBuildDayInfo>();
            this.myHighestAmountPerDay = 1;

            foreach (DateTime BuildDate in dateSorter)
            {
                currentBuildDayInfo = foundDates[BuildDate];
                result.Add(currentBuildDayInfo);            

                if (currentBuildDayInfo.AmountOfBuilds > this.myHighestAmountPerDay) 
                {
                    this.myHighestAmountPerDay = currentBuildDayInfo.AmountOfBuilds; 
                }

                this.myOKBuildAmount += currentBuildDayInfo.AmountOfOKBuilds;
                this.myFailedBuildAmount += currentBuildDayInfo.AmountOfFailedBuilds;
            }

            return result;
        }



        /// <summary>
        /// Information about a certain build 
        /// Wrapper around existing functions for ease of use in template
        ///</summary>
        public class GraphBuildInfo
        {
            private IBuildSpecifier mybuildSpecifier;
            private ILinkFactory mylinkFactory;

            public GraphBuildInfo(IBuildSpecifier buildSpecifier,  ILinkFactory linkFactory)
            {
                this.mybuildSpecifier = buildSpecifier;
                this.mylinkFactory = linkFactory;
            }
        
            //returns the day of the build (no time specification)            
            public DateTime BuildDate()
            {
                return new LogFile(this.mybuildSpecifier.BuildName).Date.Date;
            }

            public bool IsSuccesFull()
            {
                return new LogFile(this.mybuildSpecifier.BuildName).Succeeded;
            }

            public string LinkTobuild()
            {
                return this.mylinkFactory.CreateBuildLink( 
                    this.mybuildSpecifier,BuildReportBuildPlugin.ACTION_NAME).Url;                
            }

            public string Description()
            {               
                DefaultBuildNameFormatter BuildNameFormatter;
                BuildNameFormatter = new DefaultBuildNameFormatter();
                return BuildNameFormatter.GetPrettyBuildName(this.mybuildSpecifier);
            }

        }

        /// <summary>
        /// structure containing all the builds on 1 day (YYYY-MM-DD)
        /// </summary>
        public class GraphBuildDayInfo
        {
            private DateTime myBuildDate; 
            private List<GraphBuildInfo> myBuilds;

            private Int32 myOKBuildAmount;
            private Int32 myFailedBuildAmount;
            private Translations translations;

            public GraphBuildDayInfo(GraphBuildInfo buildInfo, Translations translations)
            {
                this.translations = translations;
                this.myBuildDate = buildInfo.BuildDate();
                this.myBuilds = new List<GraphBuildInfo>();
                //myBuilds.Add(buildInfo);
                this.AddBuild(buildInfo);
            }

            
            //returns the day of the builds contained
            public DateTime BuildDate
            {
                get 
                { 
                    return this.myBuildDate; 
                }
            }

            public string BuildDateFormatted
            {
                get 
                {
                    return this.myBuildDate.Date.ToString("ddd", this.translations.UICulture)
                           + "<BR>" 
                           + this.myBuildDate.Year.ToString("0000") 
                           + "<BR>" 
                           + this.myBuildDate.Month.ToString("00")
                           + "<BR>"
                           + this.myBuildDate.Day.ToString("00"); 
                }
            }


            // the amount of builds in this day
            public Int32 AmountOfBuilds
            {
                get
                {
                    return this.myBuilds.Count;
                }
            }


            // the amount of ok builds in this day
            public Int32 AmountOfOKBuilds
            {
                get
                {
                    return this.myOKBuildAmount;
                }
            }

            // the amount of failed builds in this day
            public Int32 AmountOfFailedBuilds
            {
                get
                {
                    return this.myFailedBuildAmount;
                }
            }

            //retrieves a specific build in this day
            public GraphBuildInfo Build(Int32 index)
            {
                return this.myBuilds[index];
            }

            // adds a build to this day
            public void AddBuild(GraphBuildInfo buildInfo)
            {
                this.myBuilds.Insert(0, buildInfo);
                if (buildInfo.IsSuccesFull())
                {
                    this.myOKBuildAmount++;
                }
                else
                {
                    this.myFailedBuildAmount++;
                }
            }
        }
	}
}
