using System;
using System.Collections.Generic;

namespace Hangfire.Dashboard.Management.v2.Support
{
	public static class ManagementSidebarItemCollection
	{
		public static List<Func<RazorPage, MenuItem>> Items = new List<Func<RazorPage, MenuItem>>();
	}
}
