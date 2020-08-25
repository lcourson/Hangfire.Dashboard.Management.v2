using System.Collections.Generic;

namespace Hangfire.Dashboard.Management.v2.Metadata
{
    public interface IInputDataList
    {
        Dictionary<string, string> GetData();
        string GetDefaultValue();
    }
}
