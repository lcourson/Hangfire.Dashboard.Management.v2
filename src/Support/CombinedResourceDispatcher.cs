using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Hangfire.Annotations;

namespace Hangfire.Dashboard.Management.v2.Support
{
	internal class CombinedResourceDispatcher : EmbeddedResourceDispatcher
	{
		private readonly Assembly _assembly;
		private readonly string _baseNamespace;
		private readonly string[] _resourceNames;

		public CombinedResourceDispatcher(
			[NotNull] string contentType,
			[NotNull] Assembly assembly,
			string baseNamespace,
			params string[] resourceNames) : base(contentType, assembly, null)
		{
			_assembly = assembly;
			_baseNamespace = baseNamespace;
			_resourceNames = resourceNames;
		}

		protected override async Task WriteResponse(DashboardResponse response)
		{
			foreach (var resourceName in _resourceNames)
			{
				var nameBytes = new UTF8Encoding().GetBytes($"\n\r/* {resourceName} */\n\r");
				await response.Body.WriteAsync(nameBytes, 0, nameBytes.Length).ConfigureAwait(false);
				await WriteResource(
					response,
					_assembly,
					$"{_baseNamespace}.{resourceName}").ConfigureAwait(false);
			}
		}
	}
}
