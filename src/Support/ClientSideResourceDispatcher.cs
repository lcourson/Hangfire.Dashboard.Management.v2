using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hangfire.Dashboard.Management.v2;

namespace Hangfire.Dashboard.Management.v2.Support
{
	internal class ClientSideResourceDispatcher : IDashboardDispatcher
	{
		private readonly Assembly _assembly;
		private readonly string _contentType;
		private readonly string _basePath;

		public ClientSideResourceDispatcher(Assembly assembly, string contentType)
		{
			_assembly = assembly;
			_contentType = contentType;
			_basePath = GlobalConfigurationExtension.GetAssetBaseURL();
		}

		public async Task Dispatch(DashboardContext context)
		{
			var path = context.Request.Path;
			if (path.StartsWith(_basePath))
			{
				var resourceName = path.Replace(_basePath, "");
				var resource = _assembly.GetManifestResourceNames().ToList().Where(r => r.Replace($"Content", "") == resourceName).FirstOrDefault();

				// For testing sync JS file loading
				//if (path.ToLower().Contains("popper")) { System.Threading.Thread.Sleep(5000); }

				if (resource != default)
				{
					using (var inputStream = _assembly.GetManifestResourceStream(resource))
					{
						if (inputStream == null)
						{
							throw new ArgumentException($@"Resource with name {resourceName} not found in assembly {_assembly}.");
						}

						context.Response.ContentType = _contentType;
						context.Response.SetExpire(DateTimeOffset.Now.AddYears(1));
						await inputStream.CopyToAsync(context.Response.Body).ConfigureAwait(false);
					}
				}
			}
			else
			{
				throw new ArgumentException($@"Resource with name {path} not found in assembly {_assembly}.");
			}
		}
	}
}
