using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.Dashboard.Management.v2.Metadata;
using Hangfire.Server;
using Hangfire.Storage.Monitoring;
using Newtonsoft.Json;

namespace Hangfire.Dashboard.Management.v2.Pages.Partials
{
	internal class JobPartial : RazorPage
	{
		public IEnumerable<Func<RazorPage, MenuItem>> Items { get; }
		public readonly string JobId;
		public readonly JobMetadata Job;
		public JobPartial(string id, JobMetadata job)
		{
			if (id == null) throw new ArgumentNullException(nameof(id));
			if (job == null) throw new ArgumentNullException(nameof(job));
			JobId = id;
			Job = job;
		}

		public override void Execute()
		{
			var inputs = string.Empty;

			foreach (var parameterInfo in Job.MethodInfo.GetParameters())
			{
				if (parameterInfo.ParameterType == typeof(PerformContext) || parameterInfo.ParameterType == typeof(IJobCancellationToken))
				{
					continue;
				}

				DisplayDataAttribute displayInfo = null;
				if (parameterInfo.GetCustomAttributes(true).OfType<DisplayDataAttribute>().Any())
				{
					displayInfo = parameterInfo.GetCustomAttribute<DisplayDataAttribute>();
				}
				else
				{
					displayInfo = new DisplayDataAttribute();
				}

				var labelText = displayInfo?.Label ?? parameterInfo.Name;
				var placeholderText = displayInfo?.Placeholder ?? parameterInfo.Name;
				var myId = $"{JobId}_{parameterInfo.Name}";

				if (parameterInfo.ParameterType == typeof(string))
				{
					inputs += InputTextbox(myId, displayInfo.CssClasses, labelText, placeholderText, displayInfo.Description, displayInfo.DefaultValue, displayInfo.IsDisabled, displayInfo.IsRequired, displayInfo.IsMultiLine);
				}
				else if (parameterInfo.ParameterType == typeof(int))
				{
					inputs += InputNumberbox(myId, displayInfo.CssClasses, labelText, placeholderText, displayInfo.Description, displayInfo.DefaultValue, displayInfo.IsDisabled, displayInfo.IsRequired);
				}
				else if (parameterInfo.ParameterType == typeof(Uri))
				{
					inputs += Input(myId, displayInfo.CssClasses, labelText, placeholderText, displayInfo.Description, "url", displayInfo.DefaultValue, displayInfo.IsDisabled, displayInfo.IsRequired);
				}
				else if (parameterInfo.ParameterType == typeof(DateTime))
				{
					inputs += InputDatebox(myId, displayInfo.CssClasses, labelText, placeholderText, displayInfo.Description, displayInfo.DefaultValue, displayInfo.IsDisabled, displayInfo.IsRequired, displayInfo.ControlConfiguration);
				}
				else if (parameterInfo.ParameterType == typeof(bool))
				{
					inputs += "<br/>" + InputCheckbox(myId, displayInfo.CssClasses, labelText, placeholderText, displayInfo.Description, displayInfo.DefaultValue, displayInfo.IsDisabled);
				}
				else if (parameterInfo.ParameterType.IsEnum)
				{
					var data = new Dictionary<string, string>();
					foreach (int v in Enum.GetValues(parameterInfo.ParameterType))
					{
						data.Add(Enum.GetName(parameterInfo.ParameterType, v), v.ToString());
					}
					inputs += InputDataList(myId, displayInfo.CssClasses, labelText, placeholderText, displayInfo.Description, data, displayInfo.DefaultValue?.ToString(), displayInfo.IsDisabled);
				}
				else
				{
					inputs += InputTextbox(myId, displayInfo.CssClasses, labelText, placeholderText, displayInfo.Description, displayInfo.DefaultValue, displayInfo.IsDisabled, displayInfo.IsRequired);
				}
			}

			if (string.IsNullOrWhiteSpace(inputs))
			{
				inputs = "<span>This job does not require inputs</span>";
			}

			WriteLiteral($@"
				<div class=""well"">
					{inputs}
				</div>
				<div id=""{JobId}_error""></div>
				<div id=""{JobId}_success""></div>
");
		}

		protected string Input(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, string inputtype, object defaultValue = null, bool isDisabled = false, bool isRequired = false)
		{
			var control = $@"
<div class=""form-group {cssClasses} {(isRequired ? "required" : "")}"">
	<label for=""{id}"" class=""control-label"">{labelText}</label>
";

			if (inputtype == "textarea")
			{
				control += $@"
	<textarea rows=""10"" class=""hdm-job-input hdm-input-textarea form-control"" placeholder=""{placeholderText}"" id=""{id}"" {(isDisabled ? "disabled='disabled'" : "")} {(isRequired ? "required='required'" : "")}>{defaultValue}</textarea>
";
			}
			else
			{
				control += $@"
	<input class=""hdm-job-input hdm-input-{inputtype} form-control"" type=""{inputtype}"" placeholder=""{placeholderText}"" id=""{id}"" value=""{defaultValue}"" {(isDisabled ? "disabled='disabled'" : "")} {(isRequired ? "required='required'" : "")} />
";
			}

			if (!string.IsNullOrWhiteSpace(descriptionText))
			{
				control += $@"
	<small id=""{id}Help"" class=""form-text text-muted"">{descriptionText}</small>
";
			}
			control += $@"
</div>";
			return control;
		}

		protected string InputTextbox(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, object defaultValue = null, bool isDisabled = false, bool isRequired = false, bool isMultiline = false)
		{
			return Input(id, cssClasses, labelText, placeholderText, descriptionText, isMultiline ? "textarea" : "text", defaultValue, isDisabled, isRequired);
		}

		protected string InputNumberbox(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, object defaultValue = null, bool isDisabled = false, bool isRequired = false)
		{
			return Input(id, cssClasses, labelText, placeholderText, descriptionText, "number", defaultValue, isDisabled, isRequired);
		}

		protected string InputDatebox(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, object defaultValue = null, bool isDisabled = false, bool isRequired = false, string controlConfig = "")
		{
			if (!string.IsNullOrWhiteSpace(controlConfig))
			{
				controlConfig = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(controlConfig), Formatting.None);
			}
			return $@"
<div class=""form-group {cssClasses} {(isRequired ? "required" : "")}"">
	<label for=""{id}"" class=""control-label"">{labelText}</label>
	<div class='hdm-job-input-container hdm-input-date-container input-group date' id='{id}_datetimepicker' data-td_options='{controlConfig}' data-td_value='{defaultValue}'>
		<input type='text' class=""hdm-job-input hdm-input-date form-control"" placeholder=""{placeholderText}"" {(isDisabled ? "disabled='disabled'" : "")} {(isRequired ? "required='required'" : "")} />
		<span class=""input-group-addon"">
			<span class=""glyphicon glyphicon-calendar""></span>
		</span>
	</div>
		{(!string.IsNullOrWhiteSpace(descriptionText) ? $@"
		<small id=""{id}Help"" class=""form-text text-muted"">{descriptionText}</small>
" : "")}
</div>";
		}

		protected string InputCheckbox(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, object defaultValue = null, bool isDisabled = false)
		{
			var bDefaultValue = (bool)(defaultValue ?? false);

			return $@"
<div class=""form-group {cssClasses}"">
	<div class=""form-check"">
		<input class=""hdm-job-input hdm-input-checkbox form-check-input"" type=""checkbox"" id=""{id}"" {(bDefaultValue ? "checked='checked'" : "")} {(isDisabled ? "disabled='disabled'" : "")} />
		<label class=""form-check-label"" for=""{id}"">{labelText}</label>
	</div>
		{(!string.IsNullOrWhiteSpace(descriptionText) ? $@"
		<small id=""{id}Help"" class=""form-text text-muted"">{descriptionText}</small>
" : "")}
</div>";
		}

		protected string InputDataList(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, Dictionary<string, string> data, string defaultValue = null, bool isDisabled = false)
		{
			var initText = defaultValue != null ? defaultValue : !string.IsNullOrWhiteSpace(placeholderText) ? placeholderText : "Select a value";
			var initValue = defaultValue != null && data.ContainsKey(defaultValue) ? data[defaultValue].ToString() : "";
			var output = $@"
<div class=""{cssClasses}"">
	<label class=""control-label"">{labelText}</label>
	<div class=""dropdown"">
		<button id=""{id}"" class=""hdm-job-input hdm-input-datalist btn btn-default dropdown-toggle input-control-data-list"" type=""button"" data-selectedvalue=""{initValue}"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""false"" {(isDisabled ? "disabled='disabled'" : "")}>
			<span class=""{id} input-data-list-text pull-left"">{initText}</span>
			<span class=""caret""></span>
		</button>
		<ul class=""dropdown-menu data-list-options"" data-optionsid=""{id}"" aria-labelledby=""{id}"">";
			foreach (var item in data)
			{
				output += $@"
			<li><a href=""javascript:void(0)"" class=""option"" data-optiontext=""{item.Key}"" data-optionvalue=""{item.Value}"">{item.Key}</a></li>
";
			}

			output += $@"
		</ul>
	</div>
	{(!string.IsNullOrWhiteSpace(descriptionText) ? $@"
		<small id=""{id}Help"" class=""form-text text-muted"">{descriptionText}</small>
" : "")}
</div>";

			return output;
		}
	}
}
