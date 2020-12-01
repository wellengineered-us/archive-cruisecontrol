using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;

using WellEngineered.CruiseControl.PrivateBuild.Log4Net.Config;
using WellEngineered.CruiseControl.PrivateBuild.Log4Net.Extensions;
using WellEngineered.CruiseControl.Remote;

//using ThoughtWorks.CruiseControl.Core.Util.Log4NetTrace;

// This attribute tells log4net to use the settings in the app.config file for configuration
[assembly: XmlConfigurator()]

namespace WellEngineered.CruiseControl.Core.Util
{
    // TODO: Replace using this class with the ILogger interface and the IoC container.
    /// <summary>
    /// 	
    /// </summary>
	public static class Log
	{
		private static ITraceLog logger = TraceLogManager.GetLogger("CruiseControl.NET");
        
        private static bool loggingEnabled = true;

		static Log()
		{

			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("The trace level is currently set to debug.  "
				+ "This will cause CCNet to log at the most verbose level, which is useful for setting up or debugging the server.  "
				+ "Once your server is running smoothly, we recommend changing this setting in {0} to a lower level.",
					default(string));
			}
            
            if (logger.IsTraceEnabled)
            {
                logger.WarnFormat("! ! Tracing is enabled ! !"
                    + "It allows you to sent the developpers of CCNet very detailed information of the program flow. "
                    + "This setting should only be enabled if you want to report a bug with the extra information. "
                    + "When bug reporting is done, it is advised to set the trace setting off. "
                    + "Adjust the setting in {0}", default(string));
            }
        
        }

        /// <summary>
        /// Disables logging
        /// </summary>
        public static void DisableLogging()
        {
            loggingEnabled = false;
        }

        /// <summary>
        /// Enables logging
        /// </summary>
        public static void EnableLogging()
        {
            loggingEnabled = true;
        }

        /// <summary>
        /// Logs at information level
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Info(string message, params object[] args)
		{
			if (loggingEnabled) logger.Info(string.Format(CultureInfo.CurrentCulture, message,args));
		}

        /// <summary>
        /// Logs at information level
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            if (loggingEnabled) logger.Info(message);
        }

        /// <summary>
        /// Logs at debug level
        /// </summary>
        /// <param name="message"></param>
		public static void Debug(string message)
		{
            if (loggingEnabled) logger.Debug(message);
		}

        /// <summary>
        /// Logs at debug level
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Debug(string message, params object[] args)
        {
            if (loggingEnabled) logger.Debug(string.Format(CultureInfo.CurrentCulture, message,args));
        }

        /// <summary>
        /// Logs at warning level
        /// </summary>
        /// <param name="message"></param>
		public static void Warning(string message)
		{
            if (loggingEnabled) logger.Warn(message);
		}

        /// <summary>
        /// Logs at warning level
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Warning(string message, params object[] args)
        {
            if (loggingEnabled) logger.Warn(string.Format(CultureInfo.CurrentCulture, message,args));
        }

        /// <summary>
        /// Logs at warning level
        /// </summary>
        /// <param name="ex"></param>
		public static void Warning(Exception ex)
		{
            if (loggingEnabled) logger.Warn(CreateExceptionMessage(ex));
		}

        /// <summary>
        /// Logs at Error level
        /// </summary>
        /// <param name="message"></param>
		public static void Error(string message)
		{
            logger.Error(message);
		}

        /// <summary>
        /// Logs at error level
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Error(string message, params object[] args)
        {
            logger.Error(string.Format(CultureInfo.CurrentCulture, message,args));
        }

        /// <summary>
        /// Logs at errorlevel
        /// </summary>
        /// <param name="ex"></param>
		public static void Error(Exception ex)
		{
            logger.Error(CreateExceptionMessage(ex));
		}

        /// <summary>
        /// Logs at trace level
        /// </summary>
        public static void Trace()
        {
            if (loggingEnabled && logger.IsTraceEnabled) logger.TraceFormat(string.Concat(GetCallingClassName(), "Entering"));
        }

        /// <summary>
        /// Logs at trace level
        /// </summary>
        /// <param name="message"></param>
        public static void Trace(string message)
        {
            if (loggingEnabled && logger.IsTraceEnabled)  logger.TraceFormat(string.Concat(GetCallingClassName() ,message));
        }

        /// <summary>
        /// Logs at trace level
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Trace(string message, params object[] args)
        {
            if (loggingEnabled && logger.IsTraceEnabled) logger.TraceFormat(string.Concat(GetCallingClassName(), message), args);
        }

        /// <summary>
        /// Starts the trace.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static TraceBlock StartTrace()
        {
            return new TraceBlock(logger, GetCallingClassName());
        }

        /// <summary>
        /// Starts the trace.	
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static TraceBlock StartTrace(string message, params object[] args)
        {
            return new TraceBlock(logger, GetCallingClassName(), string.Format(CultureInfo.CurrentCulture, message, args));
        }

        private static string GetCallingClassName()
        {
            StackTrace stack = default(StackTrace);
            StackFrame currentFrame = default(StackFrame);
            string myAssemblyName = null;
            string myClassName = null;
            string myMethodName = null;

            try
            {
                stack = new StackTrace();
                currentFrame = stack.GetFrame(2);
                myAssemblyName = currentFrame.GetMethod().ReflectedType.Assembly.FullName.Split(',')[0];
                myClassName = currentFrame.GetMethod().ReflectedType.ToString();
                myMethodName = currentFrame.GetMethod().Name;
            }
            catch
            {
                myClassName = "";
                myMethodName = "";
            }

            return string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0} - {1}.{2} : ", myAssemblyName, myClassName, myMethodName);
        }

		private static string CreateExceptionMessage(Exception ex)
		{
			if (ex is ThreadAbortException)
			{
				return "Thread aborted for Project: " + Thread.CurrentThread.Name;
			}

			StringBuilder buffer = new StringBuilder();
			buffer.Append(GetExceptionAlertMessage(ex));
			buffer.Append(ex.Message).Append(Environment.NewLine);
			buffer.Append("----------").Append(Environment.NewLine);
			buffer.Append(ex.ToString()).Append(Environment.NewLine);
			buffer.Append("----------").Append(Environment.NewLine);
			return buffer.ToString();
		}
		
		private static string GetExceptionAlertMessage(Exception ex)
		{
			return (ex is CruiseControlException) ? "Exception: " : "INTERNAL ERROR: ";
		}

        /// <summary>
        /// A class for putting a trace call in a block.
        /// </summary>
        public class TraceBlock
            : IDisposable
        {
            private string methodName;
            private ITraceLog logger;

            /// <summary>
            /// Initializes a new instance of the <see cref="TraceBlock"/> class.
            /// </summary>
            /// <param name="logger">The underlying logger to use.</param>
            /// <param name="methodName">The name of the method that is being traced;</param>
            public TraceBlock(ITraceLog logger, string methodName)
            {
                this.logger = logger;
                this.methodName = methodName;
                if (this.logger.IsTraceEnabled)
                {
                    this.logger.TraceFormat("Entering {0}", methodName);
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TraceBlock"/> class.
            /// </summary>
            /// <param name="logger">The underlying logger to use.</param>
            /// <param name="methodName">The name of the method that is being traced;</param>
            /// <param name="message"></param>
            public TraceBlock(ITraceLog logger, string methodName, string message)
            {
                this.logger = logger;
                this.methodName = methodName;
                if (this.logger.IsTraceEnabled)
                {
                    this.logger.TraceFormat("Entering {0}: {1}", methodName, message);
                }
            }

            /// <summary>
            /// Disposes of the trace block.
            /// </summary>
            public void Dispose()
            {
                if (this.logger.IsTraceEnabled)
                {
                    this.logger.TraceFormat("Exiting {0}", this.methodName);
                }
            }
        }
	}
}