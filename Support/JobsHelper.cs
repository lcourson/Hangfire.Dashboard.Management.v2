using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using Hangfire.Dashboard.Management.v2Unofficial.Metadata;

namespace Hangfire.Dashboard.Management.v2Unofficial.Support
{
	public static class JobsHelper
	{
		public static List<JobMetadata> Metadata { get; private set; }
		internal static List<ManagementPageAttribute> Pages { get; set; }

		internal static void GetAllJobs(Assembly assembly)
		{
			if (Metadata == null)
				Metadata = new List<JobMetadata>();
			if (Pages == null)
				Pages = new List<ManagementPageAttribute>();

			foreach (Type ti in assembly.GetTypes().Where(x => !x.IsInterface && typeof(IJob).IsAssignableFrom(x) && x.Name != (typeof(IJob).Name))) {
				var q = "default";
				var title = "Default";
				var menuName = "Default";
				if (ti.GetCustomAttributes(true).OfType<ManagementPageAttribute>().Any()) {
					var attr = ti.GetCustomAttribute<ManagementPageAttribute>();
					q = attr.Queue;
					title = attr.Title;
					menuName = attr.MenuName;
					if (!Pages.Any(x => x.Queue == q)) Pages.Add(attr);
				}


				foreach (var methodInfo in ti.GetMethods().Where(m => m.DeclaringType == ti)) {
					var meta = new JobMetadata { Type = ti, Queue = q, PageTitle = title };

					meta.MethodInfo = methodInfo;

					if (methodInfo.GetCustomAttributes(true).OfType<DescriptionAttribute>().Any()) {
						meta.Description = methodInfo.GetCustomAttribute<DescriptionAttribute>().Description;
					}

					if (methodInfo.GetCustomAttributes(true).OfType<DisplayNameAttribute>().Any()) {
						meta.DisplayName = methodInfo.GetCustomAttribute<DisplayNameAttribute>().DisplayName;
					}

					Metadata.Add(meta);
				}
			}
		}
	}
}
