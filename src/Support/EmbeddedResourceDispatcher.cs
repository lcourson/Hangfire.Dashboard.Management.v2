using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Hangfire.Dashboard.Management.v2.Support
{
	internal class EmbeddedResourceDispatcher : IDashboardDispatcher
	{
		private readonly Assembly _assembly;
		private readonly string _resourceName;
		private readonly string _contentType;

		public EmbeddedResourceDispatcher(string contentType, Assembly assembly, string resourceName)
		{
			if (contentType == null) throw new ArgumentNullException(nameof(contentType));
			if (assembly == null) throw new ArgumentNullException(nameof(assembly));

			_assembly = assembly;
			_resourceName = resourceName;
			_contentType = contentType;
		}

		public async Task Dispatch(DashboardContext context)
		{
			context.Response.ContentType = _contentType;
			context.Response.SetExpire(DateTimeOffset.Now.AddYears(1));

			await WriteResponse(context.Response).ConfigureAwait(false);
		}

		protected virtual Task WriteResponse(DashboardResponse response)
		{
			return WriteResource(response, _assembly, _resourceName);
		}

		protected async Task WriteResource(DashboardResponse response, Assembly assembly, string resourceName)
		{
			using (var inputStream = assembly.GetManifestResourceStream(resourceName))
			{
				if (inputStream == null)
				{
					throw new ArgumentException($@"Resource with name {resourceName} not found in assembly {assembly}.");
				}

				await inputStream.CopyToAsync(response.Body).ConfigureAwait(false);
			}
		}
	}
}
