using System.ComponentModel;
using Hangfire;
using Hangfire.Dashboard.Management.v2.Metadata;
using Hangfire.Dashboard.Management.v2.Support;
using Hangfire.Server;

namespace ASP.Net_Core_Web_Application.HangfireManagement
{
	[ManagementPage(MenuName = "Multiple Classes in One Menu", Title = nameof(Simple2))]
	public class Simple2 : IJob
	{
		[DisplayName("Simple 2 Job 0")]
		public void Job0(PerformContext context, IJobCancellationToken token)
		{
		}
	}

	[ManagementPage(MenuName = "Multiple Classes in One Menu", Title = nameof(Simple3))]
	public class Simple3 : IJob
	{
		[DisplayName("Simple 3 Job 0")]
		public void Job0(PerformContext context, IJobCancellationToken token)
		{
		}
	}
}
