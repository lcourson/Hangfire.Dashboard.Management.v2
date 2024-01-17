using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Hangfire.Dashboard.Management.v2.Support
{
	[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public class ClientSideConfigurations
	{
		public DateTimeOptions DateTimeOpts { get; set; } = new DateTimeOptions();

		[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
		public class DateTimeOptions
		{
			/// <summary>
			/// Specific JSON configuration data that is parsed on the client side for the "Task Type: Scheduled" execution
			/// Refer to https://getdatepicker.com/6/options/ for all options available.
			/// </summary>
			public string ScheduleButtonOptions = @"
{
	""display"": {
		""buttons"": {
			""today"": true
		}
	},
	""localization"": {
		""hourCycle"": ""h12"",
		""dayViewHeaderFormat"": {
			""month"": ""long"",
			""year"": ""numeric""
		}
	}
}";

			/// <summary>
			/// Localization to use for the DateTime controls
			/// </summary>
			[JsonConverter(typeof(StringEnumConverter))]
			public DateTimeLocales Locale { get; set; } = DateTimeLocales.@default;
			public enum DateTimeLocales
			{
				@default,
				ar,
				arSA,
				ca,
				cs,
				de,
				es,
				fi,
				fr,
				hr,
				hy,
				it,
				nl,
				pl,
				ro,
				ru,
				sl,
				sr,
				srLatin,
				tr
			}
		}
	}
}
