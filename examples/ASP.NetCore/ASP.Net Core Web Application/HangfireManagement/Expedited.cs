using System;
using System.ComponentModel;
using Hangfire;
using Hangfire.Dashboard.Management.v2.Metadata;
using Hangfire.Dashboard.Management.v2.Support;
using Hangfire.Server;
using Newtonsoft.Json;

namespace ASP.Net_Core_Web_Application.HangfireManagement
{
	[ManagementPage(MenuName = "Expedited Jobs", Title = nameof(Expedited))]
	/*              A                            B                        */
	public class Expedited : IJob
	{
		private const string dateTimeOptions = @"
{
	""display"": {
		""buttons"": {
			""clear"": true,
			""close"": false,
			""today"": true
		}
	},
	""localization"": {
		""format"": ""L"",
		""clear"": ""This button clears the current value"",
		""hourCycle"": ""h12"",
		""dayViewHeaderFormat"": {
			""month"": ""long"",
			""year"": ""numeric""
		}
	},
	""restrictions"": {
		""minDate"": ""01/01/2020 0:00""
	}
}";

		[DisplayName("Job Number 1")] //C
		[Description("This is the description for Job Number 1")] //D
		[Queue("expedited")]
		[AllowMultiple]
		[ShowMetaData(true)]
		[AutomaticRetry]
		public void Job1(PerformContext context, IJobCancellationToken token,
			[DisplayData(
				Label = "String Input 1",
				Description = "This is the description text for the string input with a default value and the control is disabled",
				DefaultValue = "This is the Default Value",
				IsDisabled = true
			)] string strInput1,

			[DisplayData(
				Placeholder = "This is the placeholder text",
				Description = "This is the description text for the string input without a default value and the control is enabled",
				IsRequired = true
			)] string strInput2,

			[DisplayData(
				Label = "Multiline Input",
				IsMultiLine = true,
				Placeholder = "This is the multiline\nplaceholder text",
				Description = "This is the description text for the multiline input without a default value where the control is enabled and not required"
			)]
			string strInput3,

			[DisplayData(
				Label = "DateTime Input",
				Placeholder = "What is the date and time?",
				DefaultValue = "01/20/2020 1:02 AM",
				Description = "This is a date time input control"//,
				//ControlConfiguration = dateTimeOptions
			)] DateTime dtInput,

			[DisplayData(
				Label = "Boolean Input",
				DefaultValue = true,
				Description = "This is a boolean input"
			)] bool blInput,

			[DisplayData(
				Label = "Select Input",
				DefaultValue = TestEnum.Test5,
				Description = "Based on an enum object"
			)] TestEnum enumTest
		)
		{
			//Do awesome things here
		}

		public enum TestEnum
		{
			Test1,
			Test2,
			Test3,
			Test4 = 44,
			Test5
		}
	}
}
