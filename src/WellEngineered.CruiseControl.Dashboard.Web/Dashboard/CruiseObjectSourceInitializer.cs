using System;
using System.Collections.Specialized;

using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;
using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.Objection;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.WebDashboard.Configuration;
using WellEngineered.CruiseControl.WebDashboard.Dashboard.ActionDecorators;
using WellEngineered.CruiseControl.WebDashboard.IO;
using WellEngineered.CruiseControl.WebDashboard.MVC;
using WellEngineered.CruiseControl.WebDashboard.MVC.Cruise;
using WellEngineered.CruiseControl.WebDashboard.Plugins.BuildReport;
using WellEngineered.CruiseControl.WebDashboard.Plugins.FarmReport;
using WellEngineered.CruiseControl.WebDashboard.Plugins.ForceBuild;
using WellEngineered.CruiseControl.WebDashboard.Plugins.ProjectReport;
using WellEngineered.CruiseControl.WebDashboard.Plugins.Security;
using WellEngineered.CruiseControl.WebDashboard.Plugins.ServerReport;

using Microsoft.AspNetCore.Http;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public class CruiseObjectSourceInitializer
	{
		private readonly ObjectionManager _objectionManager;

		public CruiseObjectSourceInitializer(ObjectionManager objectionManager)
		{
			this._objectionManager = objectionManager;
		}

		public void SetupObjectSourceForFirstRequest(HttpContext context)
		{
			this._objectionManager.AddInstanceForType(typeof(ObjectSource), this._objectionManager);

			ObjectSource objectSource = this.UpdateObjectSourceForRequest(context);

			DefaultUrlBuilder urlBuilder = new DefaultUrlBuilder();
			this._objectionManager.AddInstanceForType(typeof(IUrlBuilder),
												new AbsolutePathUrlBuilderDecorator(
													urlBuilder,
													null /*context.Request.ApplicationPath*/));

			this._objectionManager.SetImplementationType(typeof(ICruiseRequest), typeof(RequestWrappingCruiseRequest));

			this._objectionManager.SetImplementationType(typeof(IMultiTransformer), typeof(PathMappingMultiTransformer));

			this._objectionManager.SetDependencyImplementationForType(typeof(PathMappingMultiTransformer), typeof(IMultiTransformer), typeof(HtmlAwareMultiTransformer));

			IDashboardConfiguration config = GetDashboardConfiguration(objectSource, context);
			this._objectionManager.AddInstanceForType(typeof(IDashboardConfiguration), config);

			IRemoteServicesConfiguration remoteServicesConfig = config.RemoteServices;
			this._objectionManager.AddInstanceForType(typeof(IRemoteServicesConfiguration), remoteServicesConfig);

			IPluginConfiguration pluginConfig = config.PluginConfiguration;
			this._objectionManager.AddInstanceForType(typeof(IPluginConfiguration), pluginConfig);

			ISessionRetriever sessionRetriever = pluginConfig.SessionStore.RetrieveRetriever();
			this._objectionManager.AddInstanceForType(typeof(ISessionRetriever), sessionRetriever);

			ISessionStorer sessionStorer = pluginConfig.SessionStore.RetrieveStorer();
			this._objectionManager.AddInstanceForType(typeof(ISessionStorer), sessionStorer);

			this.LoadFarmPlugins(pluginConfig);
			this.LoadServerPlugins(pluginConfig);
			this.LoadProjectPlugins(pluginConfig);

			// Even if the user hasn't specified to use this plugin, we still need it registered since there are links to it elsewhere
			try
			{
				objectSource.GetByName(LatestBuildReportProjectPlugin.ACTION_NAME.ToLowerInvariant());
			}
			catch (ApplicationException)
			{
				IPlugin latestBuildPlugin = (IPlugin)objectSource.GetByType(typeof(LatestBuildReportProjectPlugin));
				this.AddActionInstance(latestBuildPlugin.NamedActions[0])
					.Decorate(typeof(ServerCheckingProxyAction))
					.Decorate(typeof(ProjectCheckingProxyAction))
					.Decorate(typeof(CruiseActionProxyAction))
					.Decorate(typeof(ExceptionCatchingActionProxy))
					.Decorate(typeof(SiteTemplateActionDecorator));
			}

			this.LoadBuildPlugins(pluginConfig);
			this.LoadSecurityPlugins(pluginConfig, sessionStorer);

			this.AddRequiredSecurityAction(LogoutSecurityAction.ActionName.ToLowerInvariant(), typeof(LogoutSecurityAction));
			this.AddRequiredSecurityAction(ChangePasswordSecurityAction.ActionName.ToLowerInvariant(), typeof(ChangePasswordSecurityAction));

			// ToDo - make this kind of thing specifiable by Plugins (note that this action is not wrapped with a SiteTemplateActionDecorator
			// See BuildLogBuildPlugin for linked todo
			this._objectionManager.AddTypeForName(XmlBuildLogAction.ACTION_NAME.ToLowerInvariant(), typeof(XmlBuildLogAction))
				.Decorate(typeof(ServerCheckingProxyAction))
				.Decorate(typeof(BuildCheckingProxyAction))
				.Decorate(typeof(ProjectCheckingProxyAction))
				.Decorate(typeof(CruiseActionProxyAction));

			// TODO - Xml Exceptions?
			this._objectionManager.AddTypeForName(ForceBuildXmlAction.ACTION_NAME.ToLowerInvariant(), typeof(ForceBuildXmlAction))
				.Decorate(typeof(ServerCheckingProxyAction))
				.Decorate(typeof(ProjectCheckingProxyAction))
				.Decorate(typeof(CruiseActionProxyAction));

			// Supporting xml project status queries from CCTray or clients earlier than version 1.3
			// Also still used by the web dashboard for displaying farm/server reports
			this._objectionManager.AddTypeForName(XmlReportAction.ACTION_NAME.ToLowerInvariant(), typeof(XmlReportAction));
			this._objectionManager.AddTypeForName(ProjectXmlReport.ActionName.ToLowerInvariant(), typeof(ProjectXmlReport))
				.Decorate(typeof(CruiseActionProxyAction));

			// Supporting cruise server project and queue status queries from CCTray or clients 1.3 or later
			this._objectionManager.AddTypeForName(XmlServerReportAction.ACTION_NAME.ToLowerInvariant(), typeof(XmlServerReportAction));

			// Security handler for CCTray or client 1.5 or later
			this._objectionManager.AddTypeForName(XmlServerSecurityAction.ACTION_NAME.ToLowerInvariant(), typeof(XmlServerSecurityAction));

			// RSS publisher
			this._objectionManager.AddTypeForName(Plugins.RSS.RSSFeed.ACTION_NAME.ToLowerInvariant(), typeof(Plugins.RSS.RSSFeed))
				.Decorate(typeof(CruiseActionProxyAction));

			// Status data
			this._objectionManager.AddTypeForName(ProjectStatusAction.ActionName.ToLowerInvariant(), typeof(ProjectStatusAction))
				.Decorate(typeof(ServerCheckingProxyAction))
				.Decorate(typeof(ProjectCheckingProxyAction))
				.Decorate(typeof(CruiseActionProxyAction));

			// File downloads
			this._objectionManager.AddTypeForName(ProjectFileDownload.ActionName.ToLowerInvariant(), typeof(ProjectFileDownload))
				.Decorate(typeof(CruiseActionProxyAction));
			this._objectionManager.AddTypeForName(BuildFileDownload.ActionName.ToLowerInvariant(), typeof(BuildFileDownload))
				.Decorate(typeof(CruiseActionProxyAction));

			// Parameters handler for CCTray or client 1.5 or later
			this._objectionManager.AddInstanceForName(XmlProjectParametersReportAction.ACTION_NAME.ToLowerInvariant(),
				objectSource.GetByType(typeof(XmlProjectParametersReportAction)));

			// Raw XML request handler
			this._objectionManager.AddTypeForName(MessageHandlerPlugin.ActionName.ToLowerInvariant(), typeof(MessageHandlerPlugin))
				.Decorate(typeof(CruiseActionProxyAction));
		}

		public ObjectSource UpdateObjectSourceForRequest(HttpContext context)
		{
			this._objectionManager.AddInstanceForType(typeof(HttpContext), context);
			HttpRequest request = context.Request;
			this._objectionManager.AddInstanceForType(typeof(HttpRequest), request);

			NameValueCollection parametersCollection = new NameValueCollection();
			//parametersCollection.Add(request.QueryString);
			//parametersCollection.Add(request.Form);
			this._objectionManager.AddInstanceForType(typeof(IRequest),
				new NameValueCollectionRequest(
					parametersCollection, null /*request.Headers*/, request.Path,
					null /*request.RawUrl*/, null /*request.ApplicationPath*/));
			return (ObjectSource)this._objectionManager; // Yuch - put this in Object Wizard somewhere
		}

		private void LoadFarmPlugins(IPluginConfiguration pluginConfig)
		{
			var LoadedPlugins = new System.Collections.Generic.List<string>();
			bool UnknownPluginDetected = false;

			foreach (IPlugin plugin in pluginConfig.FarmPlugins)
			{
				if (plugin == null)
				{
					UnknownPluginDetected = true;
				}
				else
				{
					LoadedPlugins.Add(plugin.LinkDescription);

					foreach (INamedAction action in plugin.NamedActions)
					{
						if ((action as INoSiteTemplateAction) == null)
						{
							this.AddActionInstance(action)
								.Decorate(typeof(CruiseActionProxyAction))
								.Decorate(typeof(ExceptionCatchingActionProxy))
								.Decorate(typeof(SiteTemplateActionDecorator))
								.Decorate(typeof(NoCacheabilityActionProxy));
						}
						else
						{
							this.AddActionInstance(action)
								.Decorate(typeof(CruiseActionProxyAction))
								.Decorate(typeof(ExceptionCatchingActionProxy))
								.Decorate(typeof(NoCacheabilityActionProxy));
						}
					}
				}
			}

			if (UnknownPluginDetected) this.ThrowExceptionShowingLoadedPlugins(LoadedPlugins, "FarmPlugins");
		}

		private void LoadServerPlugins(IPluginConfiguration pluginConfig)
		{
			var LoadedPlugins = new System.Collections.Generic.List<string>();
			bool UnknownPluginDetected = false;

			foreach (IPlugin plugin in pluginConfig.ServerPlugins)
			{
				if (plugin == null)
				{
					UnknownPluginDetected = true;
				}
				else
				{
					LoadedPlugins.Add(plugin.LinkDescription);
					foreach (INamedAction action in plugin.NamedActions)
					{
						if ((action as INoSiteTemplateAction) == null)
						{
							this.AddActionInstance(action)
								.Decorate(typeof(ServerCheckingProxyAction))
								.Decorate(typeof(CruiseActionProxyAction))
								.Decorate(typeof(ExceptionCatchingActionProxy))
								.Decorate(typeof(SiteTemplateActionDecorator))
								.Decorate(typeof(NoCacheabilityActionProxy));
						}
						else
						{
							this.AddActionInstance(action)
								.Decorate(typeof(ServerCheckingProxyAction))
								.Decorate(typeof(CruiseActionProxyAction))
								.Decorate(typeof(ExceptionCatchingActionProxy))
								.Decorate(typeof(NoCacheabilityActionProxy));
						}
					}
				}
			}

			if (UnknownPluginDetected) this.ThrowExceptionShowingLoadedPlugins(LoadedPlugins, "ServerPlugins");
		}

		private void LoadProjectPlugins(IPluginConfiguration pluginConfig)
		{
			var LoadedPlugins = new System.Collections.Generic.List<string>();
			bool UnknownPluginDetected = false;

			foreach (IPlugin plugin in pluginConfig.ProjectPlugins)
			{
				if (plugin == null)
				{
					UnknownPluginDetected = true;
				}
				else
				{
					LoadedPlugins.Add(plugin.LinkDescription);
					foreach (INamedAction action in plugin.NamedActions)
					{
						if ((action as INoSiteTemplateAction) == null)
						{
							this.AddActionInstance(action)
								.Decorate(typeof(ServerCheckingProxyAction))
								.Decorate(typeof(ProjectCheckingProxyAction))
								.Decorate(typeof(CruiseActionProxyAction))
								.Decorate(typeof(ExceptionCatchingActionProxy))
								.Decorate(typeof(SiteTemplateActionDecorator));
						}
						else
						{
							this.AddActionInstance(action)
								.Decorate(typeof(ServerCheckingProxyAction))
								.Decorate(typeof(ProjectCheckingProxyAction))
								.Decorate(typeof(CruiseActionProxyAction))
								.Decorate(typeof(ExceptionCatchingActionProxy));
						}
					}
				}
			}

			if (UnknownPluginDetected) this.ThrowExceptionShowingLoadedPlugins(LoadedPlugins, "ProjectPlugins");
		}

		private void LoadBuildPlugins(IPluginConfiguration pluginConfig)
		{
			var LoadedPlugins = new System.Collections.Generic.List<string>();
			bool UnknownPluginDetected = false;

			foreach (IBuildPlugin plugin in pluginConfig.BuildPlugins)
			{
				if (plugin == null)
				{
					UnknownPluginDetected = true;
				}
				else
				{
					LoadedPlugins.Add(plugin.LinkDescription);
					foreach (INamedAction action in plugin.NamedActions)
					{
						if ((action as INoSiteTemplateAction) == null)
						{
							this._objectionManager.AddInstanceForName(action.ActionName.ToLowerInvariant() + "_CONDITIONAL_GET_FINGERPRINT_CHAIN", action.Action)
								.Decorate(typeof(CruiseActionProxyAction))
								.Decorate(typeof(SiteTemplateActionDecorator));
							this.AddActionInstance(action)
								.Decorate(typeof(ServerCheckingProxyAction))
								.Decorate(typeof(BuildCheckingProxyAction))
								.Decorate(typeof(ProjectCheckingProxyAction))
								.Decorate(typeof(CruiseActionProxyAction))
								.Decorate(typeof(CachingActionProxy))
								.Decorate(typeof(ExceptionCatchingActionProxy))
								.Decorate(typeof(SiteTemplateActionDecorator));
						}
						else
						{
							this._objectionManager.AddInstanceForName(action.ActionName.ToLowerInvariant() + "_CONDITIONAL_GET_FINGERPRINT_CHAIN", action.Action)
								.Decorate(typeof(CruiseActionProxyAction));
							this.AddActionInstance(action)
								.Decorate(typeof(ServerCheckingProxyAction))
								.Decorate(typeof(BuildCheckingProxyAction))
								.Decorate(typeof(ProjectCheckingProxyAction))
								.Decorate(typeof(CruiseActionProxyAction))
								.Decorate(typeof(CachingActionProxy))
								.Decorate(typeof(ExceptionCatchingActionProxy));
						}
					}
				}
			}

			if (UnknownPluginDetected) this.ThrowExceptionShowingLoadedPlugins(LoadedPlugins, "BuildPlugins");
		}

		private void LoadSecurityPlugins(IPluginConfiguration pluginConfig, ISessionStorer sessionStorer)
		{
			var LoadedPlugins = new System.Collections.Generic.List<string>();
			bool UnknownPluginDetected = false;

			// Security plugins (for handling authentication)
			foreach (ISecurityPlugin plugin in pluginConfig.SecurityPlugins)
			{
				plugin.SessionStorer = sessionStorer;
				foreach (INamedAction action in plugin.NamedActions)
				{
					if ((action as INoSiteTemplateAction) == null)
					{
						this.AddActionInstance(action)
							.Decorate(typeof(ServerCheckingProxyAction))
							.Decorate(typeof(CruiseActionProxyAction))
							.Decorate(typeof(ExceptionCatchingActionProxy))
							.Decorate(typeof(SiteTemplateActionDecorator))
							.Decorate(typeof(NoCacheabilityActionProxy));
					}
					else
					{
						this.AddActionInstance(action)
							.Decorate(typeof(ServerCheckingProxyAction))
							.Decorate(typeof(CruiseActionProxyAction))
							.Decorate(typeof(ExceptionCatchingActionProxy))
							.Decorate(typeof(NoCacheabilityActionProxy));
					}
				}
			}

			if (UnknownPluginDetected) this.ThrowExceptionShowingLoadedPlugins(LoadedPlugins, "SecurityPlugins");
		}

		private void AddRequiredSecurityAction(string actionName, Type actionType)
		{
			this._objectionManager.AddTypeForName(actionName, actionType)
				.Decorate(typeof(ServerCheckingProxyAction))
				.Decorate(typeof(CruiseActionProxyAction))
				.Decorate(typeof(ExceptionCatchingActionProxy))
				.Decorate(typeof(SiteTemplateActionDecorator))
				.Decorate(typeof(NoCacheabilityActionProxy));
		}

		private static IDashboardConfiguration GetDashboardConfiguration(ObjectSource objectSource, HttpContext context)
		{
			return new CachingDashboardConfigurationLoader(objectSource, context);
			//			return (IDashboardConfiguration) objectSource.GetByType(typeof(IDashboardConfiguration));
		}

		private void ThrowExceptionShowingLoadedPlugins(System.Collections.Generic.List<string> loadedPlugins, string pluginTypeName)
		{
			System.Text.StringBuilder ErrorDescription = new System.Text.StringBuilder();

			ErrorDescription.AppendLine(string.Format(System.Globalization.CultureInfo.CurrentCulture, "Error loading {0} ", pluginTypeName));
			ErrorDescription.AppendLine("Unknown pluginnames detected");
			ErrorDescription.AppendLine("Check your config");
			ErrorDescription.AppendLine("The following plugins were loaded successfully : ");

			foreach (string item in loadedPlugins)
			{
				ErrorDescription.AppendLine(string.Format(System.Globalization.CultureInfo.CurrentCulture, " * {0}", item));
			}
			throw new CruiseControlException(ErrorDescription.ToString());
		}

		private DecoratableByType AddActionInstance(INamedAction action)
		{
			return this._objectionManager.AddInstanceForName(action.ActionName.ToLowerInvariant(), action.Action);
		}

	}
}