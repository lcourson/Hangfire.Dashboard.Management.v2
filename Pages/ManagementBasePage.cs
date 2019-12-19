using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Hangfire.Common;
using Hangfire.Dashboard.Management.v2Unofficial.Metadata;
using Hangfire.Dashboard.Management.v2Unofficial.Support;
using Hangfire.Dashboard.Pages;
using Hangfire.Server;
using Hangfire.States;

using Newtonsoft.Json;

namespace Hangfire.Dashboard.Management.v2Unofficial.Pages
{
	public class ManagementBasePage : RazorPage
	{
		private readonly string pageTitle;
		private readonly string pageHeader;
		private readonly string queue;


		protected internal ManagementBasePage(string pageTitle, string pageHeader, string queue)
		{
			this.pageTitle = pageTitle;
			this.pageHeader = pageHeader;
			this.queue = queue;
		}

		protected virtual void Content()
		{
			var jobs = JobsHelper.Metadata.Where(j => j.Queue.Contains(queue));

			foreach (var jobMetadata in jobs) {
				var route = $"{ManagementPage.UrlRoute}/{queue.Replace(" ", "").ToLower()}/{jobMetadata.DisplayName?.Replace(" ", string.Empty) ?? jobMetadata.MethodName}";
				var id = $"{jobMetadata.DisplayName?.Replace(" ", string.Empty) ?? jobMetadata.MethodName}";

				string inputs = string.Empty;

				foreach (var parameterInfo in jobMetadata.MethodInfo.GetParameters()) {
					if (parameterInfo.ParameterType == typeof(PerformContext) || parameterInfo.ParameterType == typeof(IJobCancellationToken))
						continue;

					DisplayDataAttribute displayInfo = null;
					if (parameterInfo.GetCustomAttributes(true).OfType<DisplayDataAttribute>().Any()) {
						displayInfo = parameterInfo.GetCustomAttribute<DisplayDataAttribute>();
					}

					var myId = $"{id}_{parameterInfo.Name}";
					if (parameterInfo.ParameterType == typeof(string)) {
						inputs += InputTextbox(myId, displayInfo.Css, displayInfo?.LabelText ?? parameterInfo.Name, displayInfo?.PlaceholderText ?? parameterInfo.Name, displayInfo?.DescriptionText, displayInfo.DefaultValue, displayInfo.IsDisabled);
					}
					else if (parameterInfo.ParameterType == typeof(int)) {
						inputs += InputNumberbox(myId, displayInfo.Css, displayInfo?.LabelText ?? parameterInfo.Name, displayInfo?.PlaceholderText ?? parameterInfo.Name, displayInfo?.DescriptionText, displayInfo.DefaultValue, displayInfo.IsDisabled);
					}
					else if (parameterInfo.ParameterType == typeof(Uri)) {
						//inputs += InputNumberbox(myId, parameterInfo?.LabelText ?? parameterInfo.Name, parameterInfo?.PlaceholderText ?? parameterInfo.Name);
						inputs += Input(myId, displayInfo.Css, displayInfo?.LabelText ?? parameterInfo.Name, displayInfo?.PlaceholderText ?? displayInfo?.LabelText ?? parameterInfo.Name, displayInfo?.DescriptionText, "url", displayInfo.DefaultValue, displayInfo.IsDisabled);
					}
					else if (parameterInfo.ParameterType == typeof(DateTime)) {
						inputs += InputDatebox(myId, displayInfo.Css, displayInfo?.LabelText ?? parameterInfo.Name, displayInfo?.PlaceholderText ?? parameterInfo.Name, displayInfo?.DescriptionText, displayInfo.DefaultValue, displayInfo.IsDisabled);
					}
					else if (parameterInfo.ParameterType == typeof(bool)) {
						inputs += "<br/>" + InputCheckbox(myId, displayInfo.Css, displayInfo?.LabelText ?? parameterInfo.Name, displayInfo?.PlaceholderText ?? parameterInfo.Name, displayInfo?.DescriptionText, displayInfo.DefaultValue, displayInfo.IsDisabled);
					}
					else if (parameterInfo.ParameterType.IsEnum) {
						var data = Enum.GetNames(parameterInfo.ParameterType).ToDictionary(f => f, f => f).ToArray();
						inputs += InputDataList(myId, displayInfo.Css, displayInfo?.LabelText ?? parameterInfo.Name, displayInfo?.PlaceholderText ?? displayInfo?.LabelText ?? parameterInfo.Name, data, displayInfo.DefaultValue?.ToString(), displayInfo.IsDisabled);
					}
					else {
						inputs += InputTextbox(myId, displayInfo.Css, displayInfo?.LabelText ?? parameterInfo.Name, displayInfo?.PlaceholderText ?? parameterInfo.Name, displayInfo?.DescriptionText, displayInfo.DefaultValue, displayInfo.IsDisabled);
					}
				}

				var isFirst = jobs.First() == jobMetadata;
				Panel(id, jobMetadata.DisplayName ?? jobMetadata.MethodName + "." + jobMetadata.MethodInfo.Name, jobMetadata.Description, inputs, "", CreateButtons(route, "Enqueue", "enqueueing", id), isFirst);
			}

			WriteLiteral($@"
	<script>
		function LoadJSM() {{
			//var jsmLib = ""{Url.To("/jsm")}"";
			//$.getScript(jsmLib, function() {{ }});
			var link2 = document.createElement('script');
			link2.setAttribute('src', '{Url.To("/jsm")}');
			document.getElementsByTagName('body')[0].appendChild(link2);
		}}

		if (window.attachEvent) {{
			window.attachEvent('onload', LoadJSM);
		}}
		else {{
			if (window.onload) {{
				var curronload = window.onload;
				var newonload = function (evt) {{
					curronload(evt);
					LoadJSM(evt);
				}};
				window.onload = newonload;
			}} else {{
				window.onload = LoadJSM;
			}}
		}}
	</script>
");

			WriteLiteral($@"
	<link rel=""stylesheet"" type=""text/css"" href=""{Url.To("/jsmcss")}"" />
");
		}

		public static void AddCommands(string queue)
		{
			var jobs = JobsHelper.Metadata.Where(j => j.Queue.Contains(queue));

			foreach (var jobMetadata in jobs) {

				var route = $"{ManagementPage.UrlRoute}/{queue.Replace(" ", "").ToLower()}/{jobMetadata.DisplayName?.Replace(" ", string.Empty) ?? jobMetadata.MethodName}";

				DashboardRoutes.Routes.Add(route, new CommandWithResponseDispatcher(context => {
					var par = new List<object>();
					string GetFormVariable(string key)
					{
						return Task.Run(() => context.Request.GetFormValuesAsync(key)).Result.FirstOrDefault();
					}
					var id = GetFormVariable("id");
					var type = GetFormVariable("type");

					foreach (var parameterInfo in jobMetadata.MethodInfo.GetParameters()) {
						if (parameterInfo.ParameterType == typeof(PerformContext) || parameterInfo.ParameterType == typeof(IJobCancellationToken)) {
							par.Add(null);
							continue;
						}

						var variable = $"{id}_{parameterInfo.Name}";
						if (parameterInfo.ParameterType == typeof(DateTime)) {
							variable = $"{variable}_datetimepicker";
						}

						variable = variable.Trim('_');
						var formInput = GetFormVariable(variable);

						object item = null;
						if (parameterInfo.ParameterType == typeof(string)) {
							item = formInput;
						}
						else if (parameterInfo.ParameterType == typeof(int)) {
							if (formInput != null) item = int.Parse(formInput);
						}
						else if (parameterInfo.ParameterType == typeof(DateTime)) {
							item = formInput == null ? DateTime.MinValue : DateTime.Parse(formInput);
						}
						else if (parameterInfo.ParameterType == typeof(bool)) {
							item = formInput == "on";
						}
						else if (!parameterInfo.ParameterType.IsValueType) {
							if (formInput == null || formInput.Length == 0) {
								item = null;
							}
							else {
								item = JsonConvert.DeserializeObject(formInput, parameterInfo.ParameterType);
							}
						}
						else {
							item = formInput;
						}

						par.Add(item);
					}

					var job = new Job(jobMetadata.Type, jobMetadata.MethodInfo, par.ToArray());
					string errorMessage = null;
					var client = new BackgroundJobClient(context.Storage);
					string jobLink = null;
					switch (type) {
						case "CronExpression": {
								var manager = new RecurringJobManager(context.Storage);
								var schedule = GetFormVariable($"{id}_schedule");
								var cron = GetFormVariable($"{id}_sys_cron");

								if (string.IsNullOrWhiteSpace(schedule ?? cron)) {
									errorMessage = "No Cron Expression Defined";
									break;
								}

								manager.AddOrUpdate(jobMetadata.DisplayName ?? jobMetadata.MethodName, job, schedule ?? cron, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), queue.Replace(" ", "").ToLower());
								jobLink = new UrlHelper(context).To("/recurring");
								break;
							}
						case "ScheduleDateTime": {
								var datetime = GetFormVariable($"{id}_sys_datetime");

								if (string.IsNullOrWhiteSpace(datetime)) {
									errorMessage = "No Schedule Defined";
									break;
								}

								if (!DateTime.TryParse(datetime, out DateTime dt)) {
									errorMessage = "Unable to parse Schedule";
									break;
								}

								var jobId = client.Create(job, new ScheduledState(dt.ToLocalTime()));//Queue
								jobLink = new UrlHelper(context).JobDetails(jobId);
								break;
							}
						case "ScheduleTimeSpan": {
								var schedule = GetFormVariable("schedule");
								var timeSpan = GetFormVariable($"{id}_sys_timespan");

								if (string.IsNullOrWhiteSpace(schedule ?? timeSpan)) {
									errorMessage = "No Delay Defined";
									break;
								}

								if (!DateTime.TryParse(schedule ?? timeSpan, out DateTime dt)) {
									errorMessage = "Unable to parse Delay";
									break;
								}

								var jobId = client.Create(job, new ScheduledState(dt));//Queue
								jobLink = new UrlHelper(context).JobDetails(jobId);
								break;
							}
						case "Enqueue":
						default: {
								var jobId = client.Create(job, new EnqueuedState(queue.Replace(" ", "").ToLower()));
								jobLink = new UrlHelper(context).JobDetails(jobId);
								break;
							}
					}

					if (!string.IsNullOrEmpty(jobLink)) {
						context.Response.StatusCode = (int)HttpStatusCode.OK;
						context.Response.WriteAsync(JsonConvert.SerializeObject(new { jobLink }));
						return true;
					}

					context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
					context.Response.WriteAsync(JsonConvert.SerializeObject(new { errorMessage }));
					return false;
				}));
			}
		}

		public override void Execute()
		{
			WriteLiteral("\r\n");
			Layout = new LayoutPage(pageTitle);

			WriteLiteral("<div class=\"row\">\r\n");
			WriteLiteral("<div class=\"col-md-3\">\r\n");

			Write(Html.RenderPartial(new CustomSidebarMenu(ManagementSidebarMenu.Items)));

			WriteLiteral("</div>\r\n");
			WriteLiteral("<div class=\"col-md-9 accordion\">\r\n");
			WriteLiteral("<h1 class=\"page-header\">\r\n");
			Write(pageHeader);
			WriteLiteral("</h1>\r\n");

			Content();

			WriteLiteral("\r\n</div>\r\n");

			CronModal();
			WriteLiteral("\r\n</div>\r\n");
		}

		protected void CronModal()
		{
			WriteLiteral($@"
<div class=""modal fade"" id=""cronModal"" tabindex=""-1"" role=""dialog"" aria-labelledby=""cronModalLabel"">
	<div class=""modal-dialog modal-lg"" role=""document"">
		<div class=""modal-content"">
			<div class=""modal-header"">
				<button type=""button"" class=""close"" data-dismiss=""modal"" aria-label=""Close""><span aria-hidden=""true"">&times;</span></button>
				<h4 class=""modal-title"" id=""cronModalLabel"">Cron Expression Builder</h4>
			</div>
			<div class=""modal-body"">");
			Write(Html.RenderPartial(new CronJobsPage()));
			WriteLiteral($@"
			</div>
			<div class=""modal-footer"">
				<button type=""button"" class=""btn btn-default"" data-dismiss=""modal"">Close</button>
				<button type=""button"" class=""btn btn-primary"" id=""connExpressionOk"">OK</button>
			</div>
		</div>
	</div>
</div>");
		}

		protected void Panel(string id, string heading, string description, string content, string options, string buttons, bool expanded)
		{
			WriteLiteral($@"
<div class=""panel panel-info js-management card"" data-id=""{id}"">
	<div id=""heading_{id}"" class=""panel-heading card-header {(expanded ? "" : "collapsed")}collapsed"" role=""button"" data-toggle=""collapse"" data-parent=""#accordion"" href=""#collapse_{id}"" aria-expanded=""{(expanded ? "true" : "false")}"" aria-controls=""collapse_{id}"">
		<h4 class=""panel-title"">
			{heading}
		</h4>
	</div>
	<div id=""collapse_{id}"" class=""panel-collapse {(expanded ? "collapse in" : "collapse")}"" aria-expanded=""{(expanded ? "true" : "false")}"" aria-labelledby=""heading_{id}"" data-parent=""#jobsAccordion"">
		<div class=""panel-body"">
			<p>{description}</p>
		</div>
		{(!string.IsNullOrEmpty(content) ? $@"
			<div class=""panel-body"">
				<div class=""well"">{content}</div>
				<div id=""{id}_error""></div>
				<div id=""{id}_success""></div>
			</div>
		" : "")}
		{(!string.IsNullOrEmpty(options) ? $@"
			<div class=""panel-body commands-options Enqueue CronExpression"">
				<div class=""well"">{options}</div>
				<div id=""{id}_error""></div>
			</div>
		" : "")}
		<div class=""panel-footer clearfix"">
			{buttons}
			<div class=""pull-right"">
			</div>
		</div>
	</div>
</div>");
		}

		protected string CreateButtons(string url, string text, string loadingText, string id)
		{
			return $@"
<div class=""col-sm-2 pull-right commands-panel Enqueue"">
	<button class=""js-management-input-commands btn btn-sm btn-success"" data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"" input-id=""{id}"" input-type=""Enqueue"">
		<span class=""glyphicon glyphicon-play-circle""></span>
		&nbsp;Queue Execution
	</button>
</div>
<div class=""col-sm-8 pull-right commands-panel ScheduleDateTime"" style=""display:none"">
	<div class=""input-group input-group-sm"">
		<div class='input-group date' id='{id}_datetimepicker'>
			<!--<input type=""text"" class=""form-control date"" placeholder=""Enter a date"" id=""{id}_sys_datetime"">-->
			<input type='text' class=""form-control"" placeholder=""Enter a Date"" id=""{id}_sys_datetime"" />
			<span class=""input-group-addon"">
				<span class=""glyphicon glyphicon-calendar""></span>
			</span>
		</div>
		<div class=""input-group-btn "">
			<button class=""btn btn-default btn-sm btn-primary js-management-input-commands"" type=""button"" input-id=""{id}"" input-type=""ScheduleDateTime""
					data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">
				<span class=""glyphicon glyphicon-calendar""></span>
				&nbsp;Schedule Execution
			</button>
		</div>
	</div>
</div>
<div class=""col-sm-8 pull-right commands-panel ScheduleTimeSpan"" style=""display:none"">
	<div class=""input-group input-group-sm"">
		<input type=""text"" class=""form-control time"" placeholder=""Enter a time 00:00:00"" id=""{id}_sys_timespan"" data-inputmask=""'mask':'99:99:99'"" value=""00:00:00"">
		<div class=""input-group-btn "">
			<button class=""btn btn-default btn-sm btn-info js-management-input-commands"" type=""button"" input-id=""{id}"" input-type=""ScheduleTimeSpan""
					data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">
				<span class=""glyphicon glyphicon-time""></span>
				&nbsp;Delayed Execution
			</button>
			<button type=""button"" class=""btn btn-info btn-sm dropdown-toggle"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""false"">
				<span class=""caret""></span>
			</button>
			<ul class=""dropdown-menu"">
				<li>
					<a href=""#"" class=""js-management-input-commands"" input-id=""{id}"" input-type=""ScheduleTimeSpan"" schedule=""0:0:5""
					   data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">5 seconds</a>
				</li>
				<li>
					<a href=""#"" class=""js-management-input-commands"" input-id=""{id}"" input-type=""ScheduleTimeSpan"" schedule=""0:0:10""
					   data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">10 seconds</a>
				</li>
				<li>
					<a href=""#"" class=""js-management-input-commands"" input-id=""{id}"" input-type=""ScheduleTimeSpan"" schedule=""0:0:15""
					   data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">15 seconds</a>
				</li>
				<li>
					<a href=""#"" class=""js-management-input-commands"" input-id=""{id}"" input-type=""ScheduleTimeSpan"" schedule=""0:0:30""
					   data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">30 seconds</a>
				</li>
				<li>
					<a href=""#"" class=""js-management-input-commands"" input-id=""{id}"" input-type=""ScheduleTimeSpan"" schedule=""0:1:0""
					   data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">60 seconds</a>
				</li>

			</ul>
		</div>
	</div>
</div>
<div class=""col-sm-8 pull-right commands-panel CronExpression"" style=""display:none"">
	<div class=""input-group input-group-sm"">
		<input type=""text"" class=""form-control"" title=""Enter a cron expression"" placeholder=""* * * * *"" id=""{id}_sys_cron"">
		<div class=""input-group-btn "">
			<button type=""button"" class=""btn btn-default js-management-input-CronModal"" input-id=""{id}"">
				<span class=""glyphicon glyphicon-wrench""></span>
			</button>
			<button class=""btn btn-default btn-sm btn-warning js-management-input-commands"" type=""button"" input-id=""{id}"" input-type=""CronExpression""
					data-confirm=""If this job already has a schedule then it will be updated.  Continue?"" data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">
				<span class=""glyphicon glyphicon-repeat""></span>
				&nbsp;Repeated Execution
			</button>
			<button type=""button"" class=""btn btn-warning btn-sm dropdown-toggle"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""false"">
				<span class=""caret""></span>
			</button>
			<ul class=""dropdown-menu"">
				<li>
					<a href=""#"" class=""js-management-input-commands"" input-id=""{id}"" input-type=""CronExpression"" schedule=""{Cron.Minutely()}""
					   data-confirm=""If this job already has a schedule then it will be updated.  Continue?"" data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">Every minute ({Cron.Minutely()})</a>
				</li>
				<li>
					<a href=""#"" class=""js-management-input-commands"" input-id=""{id}"" input-type=""CronExpression"" schedule=""{Cron.Hourly()}""
					   data-confirm=""If this job already has a schedule then it will be updated.  Continue?"" data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">Hourly ({Cron.Hourly()})</a>
				</li>
				<li>
					<a href=""#"" class=""js-management-input-commands"" input-id=""{id}"" input-type=""CronExpression"" schedule=""{Cron.Daily()}""
					   data-confirm=""If this job already has a schedule then it will be updated.  Continue?"" data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">Daily ({Cron.Daily()})</a>
				</li>
				<li>
					<a href=""#"" class=""js-management-input-commands"" input-id=""{id}"" input-type=""CronExpression"" schedule=""{Cron.Weekly()}""
					   data-confirm=""If this job already has a schedule then it will be updated.  Continue?"" data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">Weekly ({Cron.Weekly()})</a>
				</li>
				<li>
					<a href=""#"" class=""js-management-input-commands"" input-id=""{id}"" input-type=""CronExpression"" schedule=""{Cron.Monthly()}""
					   data-confirm=""If this job already has a schedule then it will be updated.  Continue?"" data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">Monthly ({Cron.Monthly()})</a>
				</li>
				<li>
					<a href=""#"" class=""js-management-input-commands"" input-id=""{id}"" input-type=""CronExpression"" schedule=""{Cron.Yearly()}""
					   data-confirm=""If this job already has a schedule then it will be updated.  Continue?"" data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">Annually ({Cron.Yearly()})</a>
				</li>
			</ul>
		</div>
	</div>
</div>
<div class=""col-sm-4"">
	<div class=""dropdown"">
		<button class=""btn dropdown-toggle"" type=""button"" id=""dropdownMenu1"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""true"">
			Task type: <span class=""{id} commandsType"">Immediate</span>
			<span class=""caret""></span>
		</button>
		<ul class=""dropdown-menu"" aria-labelledby=""dropdownMenu1"">
			<li><a href=""javascript:void(0)"" class=""commands-type"" data-commands-type=""Enqueue"" data-id=""{id}"">Immediate</a></li>
			<li><a href=""javascript:void(0)"" class=""commands-type"" data-commands-type=""ScheduleDateTime"" data-id=""{id}"">Scheduled</a></li>
			<li><a href=""javascript:void(0)"" class=""commands-type"" data-commands-type=""ScheduleTimeSpan"" data-id=""{id}"">Delayed</a></li>
			<li><a href=""javascript:void(0)"" class=""commands-type"" data-commands-type=""CronExpression"" data-id=""{id}"">Repeating</a></li>
		</ul>
	</div>
</div>";
		}

		protected string Input(string id, string css, string labelText, string placeholderText, string descriptionText, string inputtype, object defaultValue = null, bool isControlDisabled = false)
		{
			return $@"
<div class=""form-group {css}"">
		<label for=""{id}"" class=""control-label"">{labelText}</label>
		{(inputtype != "textarea" ? $@"
		<input class=""form-control"" type=""{inputtype}"" placeholder=""{placeholderText}"" id=""{id}"" value=""{defaultValue}"" {(isControlDisabled ? "disabled='disabled'" : "")} />" : $@"
		<textarea rows=""10"" class=""form-control"" placeholder=""{placeholderText}"" id=""{id}"" {(isControlDisabled ? "disabled='disabled'" : "")}>{defaultValue}</textarea>")}
		{(!string.IsNullOrWhiteSpace(descriptionText) ? $@"
		<small id=""{id}Help"" class=""form-text text-muted"">{descriptionText}</small>
" : "")}
	</div>";
		}

		protected string InputTextbox(string id, string css, string labelText, string placeholderText, string descriptionText, object defaultValue = null, bool isControlDisabled = false)
		{
			return Input(id, css, labelText, placeholderText, descriptionText, "text", defaultValue, isControlDisabled);
		}
		protected string InputNumberbox(string id, string css, string labelText, string placeholderText, string descriptionText, object defaultValue = null, bool isControlDisabled = false)
		{
			return Input(id, css, labelText, placeholderText, descriptionText, "number", defaultValue, isControlDisabled);
		}

		protected string InputDatebox(string id, string css, string labelText, string placeholderText, string descriptionText, object defaultValue = null, bool isControlDisabled = false)
		{
			return $@"
<div class=""form-group {css}"">
	<label for=""{id}"" class=""control-label"">{labelText}</label>
	<div class='input-group date' id='{id}_datetimepicker'>
		<input type='text' class=""form-control"" placeholder=""{placeholderText}"" value=""{defaultValue}"" {(isControlDisabled ? "disabled='disabled'" : "")} />
		<span class=""input-group-addon"">
			<span class=""glyphicon glyphicon-calendar""></span>
		</span>
	</div>
</div>";
		}

		protected string InputCheckbox(string id, string css, string labelText, string placeholderText, string descriptionText, object defaultValue = null, bool isControlDisabled = false)
		{
			var bDefaultValue = (bool)(defaultValue ?? false);

			return $@"
<div>
	<div class=""form-check {css}"">
		<input class=""form-check-input"" type=""checkbox"" id=""{id}"" {(bDefaultValue ? "checked='checked'" : "")} {(isControlDisabled ? "disabled='disabled'" : "")} />
		<label class=""form-check-label"" for=""{id}"">{labelText}</label>
	</div>
		{(!string.IsNullOrWhiteSpace(descriptionText) ? $@"
		<small id=""{id}Help"" class=""form-text text-muted"">{descriptionText}</small>
" : "")}
</div>";
		}

		protected string InputDataList(string id, string css, string labelText, string placeholderText, KeyValuePair<string, string>[] data, string defaultValue = null, bool isControlDisabled = false)
		{
			var output = $@"
<div class=""form-group {css}"">
		<label for=""{id}"" class=""control-label"">{labelText}</label>
		<select id=""{id}"" class=""form-control"" placeholder=""{placeholderText}"" {(isControlDisabled ? "disabled='disabled'" : "")}>";

			foreach (var item in data) {
				output += $@"
			<option value=""{item.Key}"" {(item.Key == defaultValue ? @"selected=""selected""" : "")}>{item.Value}</option>";
			}

			output += $@"
		</select>
	</div>";

			return output;
		}
	}
}
