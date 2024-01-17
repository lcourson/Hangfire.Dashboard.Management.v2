using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hangfire.Dashboard.Management.v2.Pages;
using Hangfire.Dashboard.Management.v2.Support;

namespace Hangfire.Dashboard.Management.v2
{
	public static class GlobalConfigurationExtension
	{
		public static string RouteBase = "/management";
		internal static string FileSuffix()
		{
			var version = typeof(GlobalConfigurationExtension).Assembly.GetName().Version;
			return $"{version.Major}_{version.Minor}_{version.Build}";
		}

		internal static string GetAssetBaseURL() => $"{RouteBase}/{FileSuffix()}/assets";

		public static IGlobalConfiguration UseManagementPages(this IGlobalConfiguration config, Assembly assembly, ClientSideConfigurations configOptions = null)
		{
			JobsHelper.GetAllJobs(assembly);
			InitStandard(configOptions);
			return config;
		}
		public static IGlobalConfiguration UseManagementPages(this IGlobalConfiguration config, Assembly[] assemblies, ClientSideConfigurations configOptions = null)
		{
			foreach (var assembly in assemblies)
			{
				JobsHelper.GetAllJobs(assembly);
			}
			InitStandard(configOptions);
			return config;
		}

		private static void InitStandard(ClientSideConfigurations configOptions)
		{
			AddClientResourceRoutes();
			CreateManagement();

			if (configOptions != null)
			{
				JobsHelper.ClientSideConfigurationOptions = configOptions;
			}
		}

		public static Tuple<string, string> GetResourceInfo(string resourceName)
		{
			var route = resourceName.Replace($"Content", "");
			var contentType = "application/x-hdm-unknown";
			if (route.EndsWith("_js")) { contentType = "application/javascript"; }
			else if (route.EndsWith("_css")) { contentType = "text/css"; }
			else if (route.EndsWith("_woff2")) { contentType = "font/woff2"; }
			else if (route.EndsWith("_ttf")) { contentType = "application/octet-stream"; }
			else if (route.EndsWith("_map")) { contentType = "application/json"; }

			return new Tuple<string, string>(route, contentType);

		}
		private static void AddClientResourceRoutes()
		{
			var contentNames = typeof(GlobalConfigurationExtension).GetTypeInfo().Assembly.GetManifestResourceNames();
			foreach (var contentName in contentNames)
			{
				var resourceInfo = GetResourceInfo(contentName);
				DashboardRoutes.Routes.Add(
					$"{GetAssetBaseURL()}{resourceInfo.Item1}",
					new ClientSideResourceDispatcher(
						typeof(GlobalConfigurationExtension).GetTypeInfo().Assembly,
						resourceInfo.Item2
					)
				);
			}
		}

		private static void CreateManagement()
		{
			var pageSet = new List<string>();
			foreach (var pageInfo in JobsHelper.Pages)
			{
				ManagementBasePage.AddCommands(pageInfo.MenuName);
				if (!pageSet.Contains(pageInfo.MenuName))
				{
					pageSet.Add(pageInfo.MenuName);
					ManagementSidebarItemCollection.Items.Add(p => new MenuItem(pageInfo.MenuName, p.Url.To($"{RouteBase}/{pageInfo.MenuName.ScrubURL()}")) {
						Active = p.RequestPath.StartsWith($"{RouteBase}/{pageInfo.MenuName.ScrubURL()}")
					});
				}

				DashboardRoutes.Routes.AddRazorPage($"{RouteBase}/{pageInfo.MenuName.ScrubURL()}", x => new ManagementBasePage(pageInfo.MenuName));
			}

			//note: have to use new here as the pages are dispatched and created each time. If we use an instance, the page gets duplicated on each call
			DashboardRoutes.Routes.AddRazorPage(RouteBase, x => new ManagementPage());


			NavigationMenu.Items.Add(page => new MenuItem(ManagementPage.Title, page.Url.To(RouteBase)) {
				Active = page.RequestPath == RouteBase || page.RequestPath.StartsWith($"{RouteBase}/")
			});

		}
	}
}
