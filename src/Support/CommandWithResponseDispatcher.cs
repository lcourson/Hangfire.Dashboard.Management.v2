using System;
using System.Threading.Tasks;

namespace Hangfire.Dashboard.Management.v2.Support
{
    internal class CommandWithResponseDispatcher : IDashboardDispatcher
    {
        private readonly Func<DashboardContext, bool> _command;

        public CommandWithResponseDispatcher(Func<DashboardContext, bool> command)
        {
            this._command = command;
        }

        public Task Dispatch(DashboardContext context)
        {
            DashboardRequest request = context.Request;
            DashboardResponse response = context.Response;
            if (!"POST".Equals(request.Method, StringComparison.OrdinalIgnoreCase))
            {
                response.StatusCode = 405;
                return (Task)Task.FromResult<bool>(false);
            }

            this._command(context);
            return (Task)Task.FromResult<bool>(true);
        }
    }
}
