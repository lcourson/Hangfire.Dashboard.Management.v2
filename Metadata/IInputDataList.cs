using System.Collections.Generic;

namespace Hangfire.Dashboard.Management.v2Unofficial.Metadata
{
    public interface IInputDataList
    {
        Dictionary<string, string> GetData();
        string GetDefaultValue();
    }
}
