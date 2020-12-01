using System;
using System.Globalization;
using System.Reflection;

using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.Remote.Messages;

namespace WellEngineered.CruiseControl.Core
{
	/// <summary>
    /// 	
    /// </summary>
    public class ConsoleRunner
	{
		private readonly ConsoleRunnerArguments args;
		private readonly ICruiseServerFactory serverFactory;
		private ICruiseServer server;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleRunner" /> class.	
        /// </summary>
        /// <param name="args">The args.</param>
        /// <param name="serverFactory">The server factory.</param>
        /// <remarks></remarks>
		public ConsoleRunner(ConsoleRunnerArguments args, ICruiseServerFactory serverFactory)
		{
			this.args = args;
			this.serverFactory = serverFactory;
		}

        /// <summary>
        /// Runs this instance.	
        /// </summary>
        /// <remarks></remarks>
		public void Run()
		{
			Console.WriteLine("CruiseControl.NET Server {0} -- .NET Continuous Integration Server", Assembly.GetExecutingAssembly().GetName().Version);
            // Find out our copyright claim, if any, and display it.
		    AssemblyCopyrightAttribute[] copyrightAttributes = (AssemblyCopyrightAttribute[]) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof (AssemblyCopyrightAttribute), false);
            if (copyrightAttributes.Length > 0)
            {
                Console.WriteLine("{0}  All Rights Reserved.", copyrightAttributes[0].Copyright);
            }
			Console.WriteLine(".NET Runtime Version: {0}{2}\tImage Runtime Version: {1}", Environment.Version, Assembly.GetExecutingAssembly().ImageRuntimeVersion, this.GetRuntime());
			Console.WriteLine("OS Version: {0}\tServer locale: {1}", Environment.OSVersion, CultureInfo.CurrentCulture);
			Console.WriteLine();

            // In DEBUG builds, give the developer a chance to debug our execution
            if (this.args.LaunchDebugger)
                System.Diagnostics.Debugger.Launch();

			if (this.args.ShowHelp)
			{
				//Log.Warning(ConsoleRunnerArguments.Usage);
				return;
			}

            if (!this.args.Logging)
            {
                Log.Warning("Logging has been disabled - no information (except errors) will be written to the log");
                Log.DisableLogging();
            }

            if (this.args.ValidateConfigOnly)
            {
                this.serverFactory.Create(false, this.args.ConfigFile);
                return;
            }
			this.LaunchServer();
		}

        /// <summary>
        /// Stops this instance.	
        /// </summary>
        /// <remarks></remarks>
        public void Stop()
        {
            this.server.Stop();
            this.server.WaitForExit();
        }

		private string GetRuntime()
		{
			if (Type.GetType ("Mono.Runtime") != null)
				return " [Mono]";
			return string.Empty;
		}

		private void LaunchServer()
		{
			using (ConsoleEventHandler handler = new ConsoleEventHandler())
			{
				handler.OnConsoleEvent += new EventHandler(this.HandleControlEvent);

				using (this.server = this.serverFactory.Create(this.args.UseRemoting, this.args.ConfigFile))
				{
					if (this.args.Project == null)
					{
						this.server.Start();
						this.server.WaitForExit();
					}
					else
					{
                        // Force the build
                        this.ValidateResponse(
                            this.server.ForceBuild(
                                new ProjectRequest(SecurityOverride.SessionIdentifier, this.args.Project)));

                        // Tell the server to stop as soon as the build has finished and then wait for it
                        this.ValidateResponse(
                            this.server.Stop(
                                new ProjectRequest(SecurityOverride.SessionIdentifier, this.args.Project)));
						this.server.WaitForExit(
                            new ProjectRequest(SecurityOverride.SessionIdentifier, this.args.Project));
					}
				}
			}
		}

		private void HandleControlEvent(object sender, EventArgs args)
		{
			this.server.Dispose();
		}

        /// <summary>
        /// Validates that the request processed ok.
        /// </summary>
        /// <param name="value">The response to check.</param>
        private void ValidateResponse(Response value)
        {
            if (value.Result == ResponseResult.Failure)
            {
                string message = "Request has failed on the server:" + Environment.NewLine +
                    value.ConcatenateErrors();
                throw new CruiseControlException(message);
            }
        }
    }
}
