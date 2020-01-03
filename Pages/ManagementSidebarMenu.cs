using System;
using System.Collections.Generic;

namespace Hangfire.Dashboard.Management.v2.Pages
{
    public static class ManagementSidebarMenu
    {
        public static List<Func<RazorPage, MenuItem>> Items = new List<Func<RazorPage, MenuItem>>();
    }
}