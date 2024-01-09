using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.Dashboard.Management.v2.Metadata;
using Hangfire.Server;

namespace Hangfire.Dashboard.Management.v2.Pages
{
	partial class ButtonPartial
	{
		public IEnumerable<Func<RazorPage, MenuItem>> Items { get; }
		public readonly string JobId;
		public readonly JobMetadata Job;
		public ButtonPartial(string id, JobMetadata job)
		{
			if (id == null) throw new ArgumentNullException(nameof(id));
			if (job == null) throw new ArgumentNullException(nameof(job));
			JobId = id;
			Job = job;
		}
	}
}
