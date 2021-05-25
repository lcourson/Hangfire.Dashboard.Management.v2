using System;
using System.Collections.Generic;
using System.Linq;

using Hangfire.Annotations;

namespace Hangfire.Dashboard.Management.v2.Pages
{
	internal class CustomSidebarMenu : RazorPage
	{
		public CustomSidebarMenu([NotNull] IEnumerable<Func<RazorPage, MenuItem>> items)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			Items = items;
		}

		public IEnumerable<Func<RazorPage, MenuItem>> Items { get; }

		public override void Execute()
		{
			if (!Items.Any()) return;

			WriteLiteral($@"
		<ul class=""nav nav-tabs hidden-md hidden-lg"">
");
			foreach (var item in Items)
			{
				var itemValue = item(this);
				WriteLiteral($@"
			<li role=""presentation"" class=""{(itemValue.Active ? "active" : "")}"">
				<a href=""{itemValue.Url}"">{itemValue.Text}
					<span class=""pull-right"">
");
				/*
				foreach (var metric in itemValue.GetAllMetrics())
				{
					Write(Html.InlineMetric(metric));
				}
				*/
				WriteLiteral($@"
					</span>
				</a>
			</li>
");
			}

			WriteLiteral($@"
		</ul>
");

			WriteLiteral($@"
		<ul class=""nav nav-pills nav-stacked visible-md-block visible-lg-block"">
");
			foreach (var item in Items)
			{
				var itemValue = item(this);
				WriteLiteral($@"
			<li role=""presentation"" class=""{(itemValue.Active ? "active" : "")}"">
				<a href=""{itemValue.Url}"">{itemValue.Text}
					<span class=""pull-right"">
");
				/*
				foreach (var metric in itemValue.GetAllMetrics())
				{
					Write(Html.InlineMetric(metric));
				}
				*/
				WriteLiteral($@"
					</span>
				</a>
			</li>
");
			}

			WriteLiteral($@"
		</ul>
");
			//            WriteLiteral($@"
			//        <div id=""stats"" class=""list-group"">
			//");

			//            foreach (var item in Items)
			//            {
			//                var itemValue = item(this);
			//                WriteLiteral($@"
			//            <a href=""{itemValue.Url}"" class=""list-group-item {(itemValue.Active ? "active" : "")}"">
			//                {itemValue.Text}
			//                <span class=""pull-right"">
			//");
			//                /*
			//                foreach (var metric in itemValue.GetAllMetrics())
			//                {
			//                    Write(Html.InlineMetric(metric));
			//                }
			//                */
			//                WriteLiteral($@"
			//                </span>
			//            </a>
			//");
			//            }
			//            WriteLiteral($@"
			//        </div>
			//");
		}
	}
}
