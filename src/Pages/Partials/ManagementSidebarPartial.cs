using System;
using System.Collections.Generic;
using Hangfire.Annotations;

namespace Hangfire.Dashboard.Management.v2.Pages.Partials
{
	partial class ManagementSidebarPartial
	{
		public ManagementSidebarPartial([NotNull] IEnumerable<Func<RazorPage, MenuItem>> items)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			Items = items;
		}

		public IEnumerable<Func<RazorPage, MenuItem>> Items { get; }
	}
}
