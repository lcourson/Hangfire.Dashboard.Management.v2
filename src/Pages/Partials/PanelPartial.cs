using System;
using System.Collections.Generic;
using System.Text;
using Hangfire.Annotations;
using Hangfire.Dashboard.Management.v2.Metadata;

namespace Hangfire.Dashboard.Management.v2.Pages.Partials
{
	partial class PanelPartial
	{
		public IEnumerable<Func<RazorPage, MenuItem>> Items { get; }
		public readonly string SectionName;
		public readonly List<JobMetadata> Jobs;
		public PanelPartial(string section, List<JobMetadata> jobs)
		{
			if (section == null) throw new ArgumentNullException(nameof(section));
			if (jobs == null) throw new ArgumentNullException(nameof(jobs));
			SectionName = section;
			Jobs = jobs;
		}
	}
}
