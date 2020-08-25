using Hangfire.Dashboard.Management.v2.Support;

using System;
using System.Reflection;

namespace Hangfire.Dashboard.Management.v2.Metadata
{
    public class JobMetadata
    {
        public string SectionTitle { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string MenuName { get; set; }

        public string Queue { get; set; }
        public Type Type { get; set; }
        public MethodInfo MethodInfo { get; set; }

        public string MethodName => Type.Name + "_" + MethodInfo.Name;
        public string JobId => $"{MenuName}/{Name.ScrubURL()}";
        public string Name => $"{DisplayName ?? MethodName}";
    }
}