using System.Collections.Generic;
using System.Reflection;

using Hangfire.Dashboard.Management.v2Unofficial.Pages;
using Hangfire.Dashboard.Management.v2Unofficial.Support;

namespace Hangfire.Dashboard.Management.v2Unofficial
{
	public static class GlobalConfigurationExtension
	{
		public static void UseManagementPages(this IGlobalConfiguration config, Assembly assembly)
		{
			JobsHelper.GetAllJobs(assembly);
			CreateManagement();
		}
		public static void UseManagementPages(this IGlobalConfiguration config, Assembly[] assemblies)
		{
			foreach (var assembly in assemblies) {
				JobsHelper.GetAllJobs(assembly);
			}
			CreateManagement();
		}

		private static void CreateManagement()
		{
			var pageSet = new Dictionary<string, string>();
			foreach (var pageInfo in JobsHelper.Pages) {
				ManagementBasePage.AddCommands(pageInfo.Queue);
				if (!pageSet.ContainsKey(pageInfo.Queue)) {
					pageSet.Add(pageInfo.Queue, pageInfo.Queue);
					ManagementSidebarMenu.Items.Add(p => new MenuItem(pageInfo.Queue, p.Url.To($"{ManagementPage.UrlRoute}/{pageInfo.Queue.Replace(" ", "").ToLower()}")) {
						Active = p.RequestPath.StartsWith($"{ManagementPage.UrlRoute}/{pageInfo.Queue.Replace(" ", "").ToLower()}")
					});
				}


				DashboardRoutes.Routes.AddRazorPage($"{ManagementPage.UrlRoute}/{pageInfo.Queue.Replace(" ", "").ToLower()}", x => new ManagementBasePage(pageInfo.Title, pageInfo.Title, pageInfo.Queue));
			}

			//note: have to use new here as the pages are dispatched and created each time. If we use an instance, the page gets duplicated on each call
			DashboardRoutes.Routes.AddRazorPage(ManagementPage.UrlRoute, x => new ManagementPage());

			// can't use the method of Hangfire.Console as it's usage overrides any similar usage here. Thus
			// we have to add our own endpoint to load it and call it from our code. Actually is a lot less work

			DashboardRoutes.Routes.Add("/jsmcss",
				new CombinedResourceDispatcher(
					"text/css",
					typeof(GlobalConfigurationExtension).GetTypeInfo().Assembly,
					$"{typeof(GlobalConfigurationExtension).Namespace}.Content", new[] { "Libraries.dateTimePicker.bootstrap-datetimepicker.min.css", "Libraries.inputmask.inputmask.min.css" }
					)
				);
			DashboardRoutes.Routes.Add("/jsm",
				new CombinedResourceDispatcher(
					"application/javascript",
					typeof(GlobalConfigurationExtension).GetTypeInfo().Assembly,
					$"{typeof(GlobalConfigurationExtension).Namespace}.Content", new[] { "Libraries.dateTimePicker.bootstrap-datetimepicker.min.js", "Libraries.inputmask.jquery.inputmask.bundle.min.js", "management.js", "cron.js" }
					)
				);

			NavigationMenu.Items.Add(page => new MenuItem(ManagementPage.Title, page.Url.To(ManagementPage.UrlRoute)) {
				Active = page.RequestPath.StartsWith(ManagementPage.UrlRoute)
			});

		}
	}
}