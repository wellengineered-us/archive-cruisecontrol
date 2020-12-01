

using System;

using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core.Triggers
{
	/// <summary>
    /// <para>
    /// The Interval Trigger is used to specify that an integration should be run periodically, after a certain amount of time. By default, an integration
    /// will only be triggered if modifications have been detected since the last integration. The trigger can also be configured to force a build even if
    /// no changes have occurred to source control. The items to watch for modifications are specified with <link>Source Control Blocks</link>.
    /// </para>
    /// <para type="info">
    /// Like all triggers, the intervalTrigger must be enclosed within a triggers element in the appropriate <link>Project Configuration Block</link>.
    /// </para>
    /// </summary>
    /// <title>Interval Trigger</title>
    /// <version>1.0</version>
    /// <remarks>
    /// <para type="warning">
    /// This trigger replaces the <b>PollingIntervalTrigger</b> and the <b>ForceBuildIntervalTrigger</b>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code title="Minimalist example">
    /// &lt;intervalTrigger /&gt;
    /// </code>
    /// <code title="Full example">
    /// &lt;intervalTrigger name="continuous" seconds="30" buildCondition="ForceBuild" initialSeconds="30" /&gt;
    /// </code>
    /// </example>
    [ReflectorType("intervalTrigger")]
	public class IntervalTrigger : ITrigger
	{
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const double DefaultIntervalSeconds = 60;
		private readonly DateTimeProvider dateTimeProvider;
		private string name;
		private double intervalSeconds = DefaultIntervalSeconds;
        private double initialIntervalSeconds = -1;         // -1 indicates unset
	    private bool isInitialInterval = true;

        private DateTime nextBuildTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntervalTrigger"/> class.
        /// </summary>
		public IntervalTrigger() : this(new DateTimeProvider()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntervalTrigger"/> class.
        /// </summary>
        /// <param name="dtProvider">The dt provider.</param>
		public IntervalTrigger(DateTimeProvider dtProvider)
		{
			this.dateTimeProvider = dtProvider;
            this.IncrementNextBuildTime();
            this.BuildCondition = BuildCondition.IfModificationExists;
		}

        /// <summary>
        /// The name of the trigger. This name is passed to external tools as a means to identify the trigger that requested the build.
        /// </summary>
        /// <version>1.1</version>
        /// <default>IntervalTrigger</default>
		[ReflectorProperty("name", Required=false)]
		public virtual string Name
		{
			get
			{
				if (this.name == null) this.name = this.GetType().Name;
				return this.name;
			}
			set { this.name = value; }
		}

        /// <summary>
        /// The number of seconds after an integration cycle completes before triggering the next integration cycle.
        /// </summary>
        /// <version>1.0</version>
        /// <default>60</default>
        [ReflectorProperty("seconds", Required=false)]
        public double IntervalSeconds
        {
            get { return this.intervalSeconds; }
            set
            {
                this.intervalSeconds = value;
                this.IncrementNextBuildTime();
            }
        }

        /// <summary>
        /// The delay (in seconds) from CCNet startup to the first check for modifications.
        /// </summary>
        /// <version>1.4</version>
        /// <default>Defaults to the IntervalSettings value.</default>
		[ReflectorProperty("initialSeconds", Required = false)]
		public double InitialIntervalSeconds
		{
			get
			{
                if (this.initialIntervalSeconds == -1) 
                    return this.IntervalSeconds;     // If no setting for this, use IntervalSeconds instead.
                else
                    return this.initialIntervalSeconds;
			}
			set
			{
				this.initialIntervalSeconds = value;
				this.IncrementNextBuildTime();
			}
		}                    
		
        /// <summary>
        /// The condition that should be used to launch the integration. By default, this value is <b>IfModificationExists</b>, meaning that an integration will
        /// only be triggered if modifications have been detected. Set this attribute to <b>ForceBuild</b> in order to ensure that a build should be launched 
        /// regardless of whether new modifications are detected. 
        /// </summary>
        /// <version>1.0</version>
        /// <default>IfModificationExists</default>
        [ReflectorProperty("buildCondition", Required = false)]
        public BuildCondition BuildCondition { get; set; }

        /// <summary>
        /// Integrations the completed.	
        /// </summary>
        /// <remarks></remarks>
		public virtual void IntegrationCompleted()
		{
            this.isInitialInterval = false;

			this.IncrementNextBuildTime();
		}

        /// <summary>
        /// Increments the next build time.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		protected DateTime IncrementNextBuildTime()
		{
		    double delaySeconds;
            if (this.isInitialInterval)
				delaySeconds = this.InitialIntervalSeconds;
            else
                delaySeconds = this.IntervalSeconds;

            return this.nextBuildTime = this.dateTimeProvider.Now.AddSeconds(delaySeconds);
		}

        /// <summary>
        /// Gets the next build.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public DateTime NextBuild
		{
			get {  return this.nextBuildTime;}
		}

        /// <summary>
        /// Fires this instance.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public virtual IntegrationRequest Fire()
		{
			BuildCondition buildCondition = this.ShouldRunIntegration();
			if (buildCondition == BuildCondition.NoBuild) return null;
			return new IntegrationRequest(buildCondition, this.Name, null);
		}

		private BuildCondition ShouldRunIntegration()
		{
			if (this.dateTimeProvider.Now < this.nextBuildTime)
				return BuildCondition.NoBuild;

			return this.BuildCondition;
		}
	}
}