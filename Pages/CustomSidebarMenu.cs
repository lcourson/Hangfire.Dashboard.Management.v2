using System;
using System.Collections.Generic;
using System.Linq;

using Hangfire.Annotations;

namespace Hangfire.Dashboard.Management.v2Unofficial.Pages
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
            WriteLiteral("\r\n");

            if (!Items.Any()) return;

            WriteLiteral("<div id=\"stats\" class=\"list-group\">\r\n");

            foreach (var item in Items)
            {
                var itemValue = item(this);
                WriteLiteral("<a href=\"");
                Write(itemValue.Url);
                WriteLiteral("\" class=\"list-group-item ");
                Write(itemValue.Active ? "active" : null);
                WriteLiteral("\">\r\n");
                Write(itemValue.Text);
                WriteLiteral("\r\n<span class=\"pull-right\">\r\n");
                /*
                foreach (var metric in itemValue.GetAllMetrics())
                {
                    Write(Html.InlineMetric(metric));
                }
                */
                WriteLiteral("</span>\r\n</a>\r\n");
            }
            
            WriteLiteral("</div>\r\n");
        }
    }
}