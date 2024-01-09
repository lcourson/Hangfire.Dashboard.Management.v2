using System.Collections.Generic;
using System.Reflection;

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

		internal enum ManagementUrlType
		{
			Css,
			JsInit,
			JsBundle,
			FontAwesomeWebFontWoff2,
			FontAwesomeWebFontTtf
		}

		internal static string GetURL(ManagementUrlType urlType)
		{
			switch (urlType)
			{
				case ManagementUrlType.FontAwesomeWebFontWoff2: return $"{RouteBase}/assets/woff2/fontawesome";
				case ManagementUrlType.FontAwesomeWebFontTtf: return $"{RouteBase}/assets/ttf/fontawesome";
				case ManagementUrlType.Css: return $"{RouteBase}/assets/hdm-css-{FileSuffix()}";
				case ManagementUrlType.JsInit: return $"{RouteBase}/assets/hdm-init-{FileSuffix()}";
				case ManagementUrlType.JsBundle: return $"{RouteBase}/assets/hdm-bundle-{FileSuffix()}";
				default: return "";
			}

		}

		public static void UseManagementPages(this IGlobalConfiguration config, Assembly assembly)
		{
			JobsHelper.GetAllJobs(assembly);
			AddClientResourceRoutes();
			CreateManagement();
		}
		public static void UseManagementPages(this IGlobalConfiguration config, Assembly[] assemblies)
		{
			foreach (var assembly in assemblies)
			{
				JobsHelper.GetAllJobs(assembly);
			}
			AddClientResourceRoutes();
			CreateManagement();
		}

		private static void AddClientResourceRoutes()
		{
			/* Init JS script that will add other JS Script tags on page load.  This is to make sure that jQuery and BootStrap are already loaded in the DOM before our scripts */
			DashboardRoutes.Routes.Add(GetURL(ManagementUrlType.JsInit),
				new CombinedResourceDispatcher(
					"application/javascript",
					typeof(GlobalConfigurationExtension).GetTypeInfo().Assembly,
					$"{typeof(GlobalConfigurationExtension).Namespace}.Content", new[] { "jsm-init.js" }
				)
			);

			DashboardRoutes.Routes.Add(GetURL(ManagementUrlType.FontAwesomeWebFontWoff2),
				new EmbeddedResourceDispatcher(
					"font/woff2",
					typeof(GlobalConfigurationExtension).GetTypeInfo().Assembly,
					$"{typeof(GlobalConfigurationExtension).Namespace}.Content.Libraries.fontawesome.webfonts.fa-solid-900.woff2"
				)
			);

			DashboardRoutes.Routes.Add(GetURL(ManagementUrlType.FontAwesomeWebFontTtf),
				new EmbeddedResourceDispatcher(
					"application/octet-stream",
					typeof(GlobalConfigurationExtension).GetTypeInfo().Assembly,
					$"{typeof(GlobalConfigurationExtension).Namespace}.Content.Libraries.fontawesome.webfonts.fa-solid-900.ttf"
				)
			);

			DashboardRoutes.Routes.Add(GetURL(ManagementUrlType.Css),
				new CombinedResourceDispatcher(
					"text/css",
					typeof(GlobalConfigurationExtension).GetTypeInfo().Assembly,
					$"{typeof(GlobalConfigurationExtension).Namespace}.Content", new[] { "Libraries.fontawesome.css.fontawesome.css", "Libraries.fontawesome.css.customized-solid.css", "Libraries.tempusDominus.tempus-dominus.min.css", "Libraries.inputmask.inputmask.min.css", "management.css" }
				)
			);

			DashboardRoutes.Routes.Add(GetURL(ManagementUrlType.JsBundle),
				new CombinedResourceDispatcher(
					"application/javascript",
					typeof(GlobalConfigurationExtension).GetTypeInfo().Assembly,
					$"{typeof(GlobalConfigurationExtension).Namespace}.Content", new[] { "Libraries.popperJS.popper.min.js", "Libraries.tempusDominus.tempus-dominus.min.js", "Libraries.inputmask.inputmask.min.js", "management.js" }
				)
			);
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

		//private static void CreateManagement2()
		//{
		//	var pageSet = new List<string>();
		//	foreach (var pageInfo in JobsHelper.Pages)
		//	{
		//		ManagementBasePage2.AddCommands(pageInfo.MenuName);
		//		if (!pageSet.Contains(pageInfo.MenuName))
		//		{
		//			pageSet.Add(pageInfo.MenuName);
		//			ManagementSidebarMenu.Items2.Add(p => new MenuItem(pageInfo.MenuName, p.Url.To($"{RouteBase}/{pageInfo.MenuName.ScrubURL()}")) {
		//				Active = p.RequestPath.StartsWith($"{RouteBase}/{pageInfo.MenuName.ScrubURL()}")
		//			});
		//		}

		//		DashboardRoutes.Routes.AddRazorPage($"{RouteBase}/{pageInfo.MenuName.ScrubURL()}", x => new ManagementBasePage2(pageInfo.MenuName));
		//	}

		//	//note: have to use new here as the pages are dispatched and created each time. If we use an instance, the page gets duplicated on each call
		//	DashboardRoutes.Routes.AddRazorPage($"{RouteBase}", x => new ManagementPage2());

		//	NavigationMenu.Items.Add(page => new MenuItem(ManagementBasePage2.Title, page.Url.To($"{RouteBase}2")) {
		//		Active = page.RequestPath == $"{RouteBase}" || page.RequestPath.StartsWith($"{RouteBase}2/")
		//	});

		//}
	}
}
