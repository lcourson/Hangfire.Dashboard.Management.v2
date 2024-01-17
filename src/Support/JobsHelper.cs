using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Hangfire.Dashboard.Management.v2.Metadata;

namespace Hangfire.Dashboard.Management.v2.Support
{
	public static class JobsHelper
	{
		public static List<JobMetadata> Metadata { get; private set; } = new List<JobMetadata>();
		internal static List<ManagementPageAttribute> Pages { get; set; } = new List<ManagementPageAttribute>();

		internal static ClientSideConfigurations ClientSideConfigurationOptions { get; set; } = new ClientSideConfigurations();

		internal static void GetAllJobs(Assembly assembly)
		{
			foreach (Type ti in assembly.GetTypes().Where(x => typeof(IJob).IsAssignableFrom(x) && x.Name != (typeof(IJob).Name)))
			{
				var q = "default";
				var title = "Default";
				var menuName = "Default";

				if (ti.GetCustomAttributes(true).OfType<ManagementPageAttribute>().Any())
				{
					var mgmtPageAttr = ti.GetCustomAttribute<ManagementPageAttribute>();
					title = mgmtPageAttr.Title;
					menuName = mgmtPageAttr.MenuName;
					if (!Pages.Any(x => x.MenuName == menuName)) Pages.Add(mgmtPageAttr);
				}

				foreach (var methodInfo in ti.GetMethods().Where(m => m.DeclaringType == ti))
				{
					var meta = new JobMetadata { Type = ti, Queue = q, SectionTitle = title, MenuName = menuName };

					meta.MethodInfo = methodInfo;

					if (methodInfo.GetCustomAttributes(true).OfType<QueueAttribute>().Any())
					{
						meta.Queue = methodInfo.GetCustomAttribute<QueueAttribute>().Queue;
					}

					if (methodInfo.GetCustomAttributes(true).OfType<DescriptionAttribute>().Any())
					{
						meta.Description = methodInfo.GetCustomAttribute<DescriptionAttribute>().Description;
					}

					if (methodInfo.GetCustomAttributes(true).OfType<DisplayNameAttribute>().Any())
					{
						meta.DisplayName = methodInfo.GetCustomAttribute<DisplayNameAttribute>().DisplayName;
					}

					if (methodInfo.GetCustomAttributes(true).OfType<AllowMultipleAttribute>().Any())
					{
						meta.AllowMultiple = methodInfo.GetCustomAttribute<AllowMultipleAttribute>().AllowMultiple;
					}

					Metadata.Add(meta);
				}
			}
		}
		public static List<string> GetAllQueues()
		{
			var queues = Metadata.Select(m => m.Queue).Distinct().ToList();
			Regex rx = new Regex("[^a-z0-9_-]+");
			if (queues.Any(q => rx.Match(q).Success))
			{
				throw new Exception("The queue name must consist of lowercase letters, digits, underscore, and dash characters only.");
			}
			return queues;
		}
	}
}
