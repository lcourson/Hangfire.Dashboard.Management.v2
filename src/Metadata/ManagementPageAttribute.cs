using System;

namespace Hangfire.Dashboard.Management.v2.Metadata
{
	public class ManagementPageAttribute : Attribute
	{
		/// <summary>
		/// Title to display as header for Jobs
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Name of the Menu that contains the jobs
		/// </summary>
		public string MenuName { get; set; }

		public ManagementPageAttribute(string menuName, string title)
		{
			MenuName = menuName;
			Title = title;
		}
		public ManagementPageAttribute()
		{
		}
	}
}