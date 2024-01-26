using System.Linq;

namespace Hangfire.Dashboard.Management.v2.Support
{
	public static class ExtensionMethods
	{
		public static string ScrubURL(this string seed) => System.Web.HttpUtility.HtmlEncode(seed);
	}
}
