using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Hangfire.Common;
using Hangfire.Dashboard.Management.v2.Metadata;
using Hangfire.Dashboard.Management.v2.Support;
using Hangfire.Dashboard.Pages;
using Hangfire.Server;
using Hangfire.States;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hangfire.Dashboard.Management.v2.Pages
{
	public class ManagementBasePage : RazorPage
	{
		private readonly string menuName;


		protected internal ManagementBasePage(string menuName)
		{
			this.menuName = menuName;
		}

		public static void AddCommands(string menuName)
		{
			var jobs = JobsHelper.Metadata.Where(j => j.MenuName.Contains(menuName));

			foreach (var jobMetadata in jobs)
			{

				var route = $"{ManagementPage.UrlRoute}/{jobMetadata.JobId.ScrubURL()}";

				DashboardRoutes.Routes.Add(route, new CommandWithResponseDispatcher(context => {
					string errorMessage = null;
					string jobLink = null;
					var par = new List<object>();
					string GetFormVariable(string key)
					{
						return Task.Run(() => context.Request.GetFormValuesAsync(key)).Result.FirstOrDefault();
					}
					var id = GetFormVariable("id");
					var type = GetFormVariable("type");

					foreach (var parameterInfo in jobMetadata.MethodInfo.GetParameters())
					{
						if (parameterInfo.ParameterType == typeof(PerformContext) || parameterInfo.ParameterType == typeof(IJobCancellationToken))
						{
							par.Add(null);
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

						var variable = $"{id}_{parameterInfo.Name}";
						if (parameterInfo.ParameterType == typeof(DateTime))
						{
							variable = $"{variable}_datetimepicker";
						}

						variable = variable.Trim('_');
						var formInput = GetFormVariable(variable);

						object item = null;
						if (parameterInfo.ParameterType == typeof(string))
						{
							item = formInput;
							if (displayInfo.IsRequired && string.IsNullOrWhiteSpace((string)item))
							{
								errorMessage = $"{parameterInfo.Name} is required.";
								break;
							}
						}
						else if (parameterInfo.ParameterType == typeof(int))
						{
							int intNumber;
							if (int.TryParse(formInput, out intNumber) == false)
							{
								errorMessage = $"{parameterInfo.Name} was not in a correct format.";
								break;
							}
							item = intNumber;
						}
						else if (parameterInfo.ParameterType == typeof(DateTime))
						{
							item = formInput == null ? DateTime.MinValue : DateTime.Parse(formInput);
							if (displayInfo.IsRequired && item.Equals(DateTime.MinValue))
							{
								errorMessage = $"{parameterInfo.Name} is required.";
								break;
							}
						}
						else if (parameterInfo.ParameterType == typeof(bool))
						{
							item = formInput == "on";
						}
						else if (!parameterInfo.ParameterType.IsValueType)
						{
							if (formInput == null || formInput.Length == 0)
							{
								item = null;
								if (displayInfo.IsRequired)
								{
									errorMessage = $"{parameterInfo.Name} is required.";
									break;
								}
							}
							else
							{
								item = JsonConvert.DeserializeObject(formInput, parameterInfo.ParameterType);
							}
						}
						else
						{
							item = formInput;
						}

						par.Add(item);
					}

					if (errorMessage == null)
					{
						var job = new Job(jobMetadata.Type, jobMetadata.MethodInfo, par.ToArray());
						var client = new BackgroundJobClient(context.Storage);
						switch (type)
						{
							case "CronExpression":
								{
									var manager = new RecurringJobManager(context.Storage);
									var schedule = GetFormVariable($"{id}_schedule");
									var cron = GetFormVariable($"{id}_sys_cron");
									var name = GetFormVariable($"{id}_sys_name");

									if (string.IsNullOrWhiteSpace(schedule ?? cron))
									{
										errorMessage = "No Cron Expression Defined";
										break;
									}
									if (jobMetadata.AllowMultiple && string.IsNullOrWhiteSpace(name))
									{
										errorMessage = "No Job Name Defined";
										break;
									}

									try
									{
										var jobId = jobMetadata.AllowMultiple ? name : jobMetadata.JobId;
										manager.AddOrUpdate(jobId, job, schedule ?? cron, TimeZoneInfo.Local, jobMetadata.Queue);
										jobLink = new UrlHelper(context).To("/recurring");
									}
									catch (Exception e)
									{
										errorMessage = e.Message;
									}
									break;
								}
							case "ScheduleDateTime":
								{
									var datetime = GetFormVariable($"{id}_sys_datetime");

									if (string.IsNullOrWhiteSpace(datetime))
									{
										errorMessage = "No Schedule Defined";
										break;
									}

									if (!DateTime.TryParse(datetime, out DateTime dt))
									{
										errorMessage = "Unable to parse Schedule";
										break;
									}
									try
									{
										var jobId = client.Create(job, new ScheduledState(dt.ToLocalTime()));//Queue
										jobLink = new UrlHelper(context).JobDetails(jobId);
									}
									catch (Exception e)
									{
										errorMessage = e.Message;
									}
									break;
								}
							case "ScheduleTimeSpan":
								{
									var schedule = GetFormVariable("schedule");
									var timeSpan = GetFormVariable($"{id}_sys_timespan");

									if (string.IsNullOrWhiteSpace(schedule ?? timeSpan))
									{
										errorMessage = "No Delay Defined";
										break;
									}

									if (!TimeSpan.TryParse(schedule ?? timeSpan, out TimeSpan dt))
									{
										errorMessage = "Unable to parse Delay";
										break;
									}

									try
									{
										var jobId = client.Create(job, new ScheduledState(dt));//Queue
										jobLink = new UrlHelper(context).JobDetails(jobId);
									}
									catch (Exception e)
									{
										errorMessage = e.Message;
									}
									break;
								}
							case "Enqueue":
							default:
								{
									try
									{
										var jobId = client.Create(job, new EnqueuedState(jobMetadata.Queue));
										jobLink = new UrlHelper(context).JobDetails(jobId);
									}
									catch (Exception e)
									{
										errorMessage = e.Message;
									}
									break;
								}
						}
					}

					context.Response.ContentType = "application/json";

					if (!string.IsNullOrEmpty(jobLink))
					{
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
			Layout = new LayoutPage(menuName);
			WriteLiteral($@"
<div class=""row"">
	<div class=""col-md-3"">
");
			Write(Html.RenderPartial(new CustomSidebarMenu(ManagementSidebarMenu.Items)));
			WriteLiteral($@"
	</div>
	<div class=""col-md-9 accordion job-panels"">
");

			var jobs = JobsHelper.Metadata.Where(j => j.MenuName.Contains(menuName)).OrderBy(x => x.SectionTitle).ThenBy(x => x.Name);
			var taskSections = jobs.Select(j => j.SectionTitle).Distinct().ToDictionary(k => k, v => string.Empty);

			foreach (var section in taskSections.Keys)
			{
				var scrubbedSection = section.ScrubURL();
				var expanded = taskSections.Keys.First() == section;

				if (taskSections.Count > 1)
				{
					WriteLiteral($@"
		<div class=""panel panel-info card wrapper-panel"" data-id=""section_{scrubbedSection}"">
			<div id=""section_heading_{scrubbedSection}"" class=""panel-heading card-header {(expanded ? "" : "collapsed")}collapsed"" role=""button"" data-toggle=""collapse"" data-parent=""#accordion"" href=""#section_collapse_{scrubbedSection}"" aria-expanded=""{(expanded ? "true" : "false")}"" aria-controls=""section_collapse_{scrubbedSection}"">
				<h4 class=""panel-title"">
					{section}
				</h4>
			</div>
");
				}
				else
				{
					WriteLiteral($@"
			<h1 class=""page-header single-section"">{section}</h1>
");
				}
				WriteLiteral($@"
			<div id=""section_collapse_{scrubbedSection}"" class=""panel-collapse {(expanded ? "collapse in" : "collapse")}"" aria-expanded=""{(expanded ? "true" : "false")}"" aria-labelledby=""section_heading_{scrubbedSection}"" data-parent=""#jobsAccordion"">
");
				PanelWriter(scrubbedSection, jobs.Where(j => j.SectionTitle == section).ToList());
				WriteLiteral($@"
			</div>
		</div>
");
			}

			if (taskSections.Count > 1)
			{
				WriteLiteral($@"
	</div>
");
			}

			CronModal();
			WriteLiteral($@"
</div>
<script>
	function LoadJSM() {{
		var link2 = document.createElement('script');
		link2.setAttribute('src', '{Url.To($"{ManagementPage.UrlRoute}/jsm")}');
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
<link rel=""stylesheet"" type=""text/css"" href=""{Url.To($"{ManagementPage.UrlRoute}/jsmcss")}"" />
");
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

		protected void PanelWriter(string section, List<JobMetadata> jobs)
		{
			foreach (var job in jobs)
			{
				var id = $"{section}_{job.Name.ScrubURL()}";
				var expanded = jobs.First() == job;

				var options = new JObject();
				var qAttr = job.MethodInfo.GetCustomAttributes(true).OfType<QueueAttribute>().FirstOrDefault();
				options.Add("Queue", (qAttr == default ? "default" : qAttr.Queue).ToUpper());

				var showMDAttr = job.MethodInfo.GetCustomAttributes(true).OfType<ShowMetaDataAttribute>().FirstOrDefault();
				var showMeta = showMDAttr != default && showMDAttr.ShowOnUI;
				if (showMeta)
				{
					var retryAttr = job.MethodInfo.GetCustomAttributes(true).OfType<AutomaticRetryAttribute>().FirstOrDefault();
					if (retryAttr != default)
					{
						var ar = new JObject
						{
							{ "Attempts", retryAttr.Attempts },
							{ "AllowMultiple", retryAttr.AllowMultiple },
							{ "DelaysInSeconds", (retryAttr.DelaysInSeconds != null ? JsonConvert.SerializeObject(retryAttr.DelaysInSeconds) : null) },
							{ "LogEvents", retryAttr.LogEvents },
							{ "OnAttemptsExceeded", (retryAttr.OnAttemptsExceeded == AttemptsExceededAction.Delete ? "Delete" : "Fail") }
						};
						options.Add("AutomaticRetryAttribute", ar);
					}
				}

				WriteLiteral($@"
	<div class=""panel panel-info js-management card"" data-id=""{id}"" style=""{(expanded ? "margin-top:20px" : "")}"">
		<div id=""heading_{id}"" class=""panel-heading card-header {(expanded ? "" : "collapsed")}collapsed"" role=""button"" data-toggle=""collapse"" data-parent=""#accordion"" href=""#collapse_{id}"" aria-expanded=""{(expanded ? "true" : "false")}"" aria-controls=""collapse_{id}"">
			<h4 class=""panel-title"">
				{job.Name}
			</h4>
		</div>
		<div id=""collapse_{id}"" class=""panel-collapse {(expanded ? "collapse in" : "collapse")}"" aria-expanded=""{(expanded ? "true" : "false")}"" aria-labelledby=""heading_{id}"" data-parent=""#jobsAccordion"">
			<div class=""panel-body"" style=""padding-bottom: 0px;"">
				<p>{job.Description}</p>
");
				if (showMeta)
				{
					WriteLiteral($@"
				<div class=""well"" style=""display: flex; padding: 3px; margin-bottom: 0px;"">
					<div class=""col-xs-1"" role=""button"" data-toggle=""collapse"" href=""#options_collapse_{id}"" aria-expanded=""false"" aria-controls=""options_collapse_{id}"">
						<span class=""glyphicon glyphicon-info-sign""></span>
					</div>
					<pre style=""margin-bottom: 0px; border: transparent;"" class=""col-xs-11 collapse"" aria-expanded=""false"" id=""options_collapse_{id}"">{JsonConvert.SerializeObject(options, Formatting.Indented)}</pre>
				</div>
");
				}
				WriteLiteral($@"
			</div>
			<div class=""panel-body"" style=""padding-bottom: 0px;"">
");
				JobWriter(id, job);
				WriteLiteral($@"
			</div>
			<div class=""panel-footer"">
");
				ButtonWriter(id, job);
				WriteLiteral($@"
			</div>
		</div>
	</div>
");
			}
		}

		protected void JobWriter(string id, JobMetadata job)
		{
			string inputs = string.Empty;

			foreach (var parameterInfo in job.MethodInfo.GetParameters())
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
				var myId = $"{id}_{parameterInfo.Name}";

				if (parameterInfo.ParameterType == typeof(string))
				{
					inputs += InputTextbox(myId, displayInfo.CssClasses, labelText, placeholderText, displayInfo.Description, displayInfo.DefaultValue, displayInfo.IsDisabled, displayInfo.IsRequired);
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
					inputs += InputDatebox(myId, displayInfo.CssClasses, labelText, placeholderText, displayInfo.Description, displayInfo.DefaultValue, displayInfo.IsDisabled, displayInfo.IsRequired);
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
				<div id=""{id}_error""></div>
				<div id=""{id}_success""></div>
");
		}

		protected void ButtonWriter(string id, JobMetadata job)
		{
			var url = $"{ManagementPage.UrlRoute}/{job.JobId.ScrubURL()}";
			var loadingText = "Queuing";

			WriteLiteral($@"
				<div class=""btn-group col-xs-12 col-sm-3"">
					<button class=""btn btn-default dropdown-toggle"" type=""button"" id=""dropdownMenu1"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""false"">
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
				<div class=""commands-panel col-xs-12 Enqueue col-sm-9"">
					<button class=""js-management-input-commands btn btn-sm btn-success"" data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"" input-id=""{id}"" input-type=""Enqueue"">
						<span class=""glyphicon glyphicon-play-circle""></span>
						&nbsp;Queue Execution
					</button>
				</div>
				<div class=""commands-options ScheduleDateTime col-xs-12 col-sm-6"" style=""display:none;"">
					<div class='input-group date' id='{id}_datetimepicker'>
						<input type='text' class=""form-control"" placeholder=""Enter a Date"" id=""{id}_sys_datetime"" />
						<span class=""input-group-addon"">
							<span class=""glyphicon glyphicon-calendar""></span>
						</span>
					</div>
				</div>
				<div class=""commands-panel ScheduleDateTime col-xs-12 col-sm-3"" style=""display:none;"">
					<button class=""btn btn-default btn-sm btn-primary js-management-input-commands"" type=""button"" input-id=""{id}"" input-type=""ScheduleDateTime"" data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">
						<span class=""glyphicon glyphicon-calendar""></span>
						&nbsp;Schedule Execution
					</button>
				</div>
				<div class=""commands-options ScheduleTimeSpan col-xs-12 col-sm-6"" style=""display:none;"">
					<input type=""text"" class=""form-control time"" placeholder=""Enter a time 00:00:00"" id=""{id}_sys_timespan"" data-inputmask=""'mask':'99:99:99'"" value=""00:00:00"">
				</div>
				<div class=""commands-panel ScheduleTimeSpan col-xs-12 col-sm-3"" style=""display:none;"">
					<div class=""btn-group"">
						<button class=""btn btn-default btn-sm btn-info js-management-input-commands"" type=""button"" input-id=""{id}"" input-type=""ScheduleTimeSpan""
								data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">
							<span class=""glyphicon glyphicon-time""></span>
							&nbsp;Delayed Execution
						</button>
						<button type=""button"" class=""btn btn-info btn-sm dropdown-toggle"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""false"">
							<span class=""caret""></span>
						</button>
						<ul class=""dropdown-menu dropdown-menu-right"">
");
			var timeSpanItems = new Dictionary<string, string>() {
				{ "5 seconds", "0:0:5" },
				{ "10 seconds", "0:0:10" },
				{ "15 seconds", "0:0:15" },
				{ "30 seconds", "0:0:30" },
				{ "60 seconds", "0:1:0" }
			};
			foreach (var o in timeSpanItems)
			{
				WriteLiteral($@"
							<li>
								<a href=""#"" class=""js-management-input-commands text-center"" input-id=""{id}"" input-type=""ScheduleTimeSpan"" schedule=""{o.Value}""
									data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">{o.Key}</a>
							</li>
");
			}

			WriteLiteral($@"
						</ul>
					</div>
				</div>
				<div class=""commands-options CronExpression col-xs-12 col-sm-5"" style=""display:none;"">
					<div class='input-group' id='{id}_cronbuilder'>
						<input type=""text"" class=""form-control"" title=""Enter a cron expression or use the builder by clicking on the wrench"" placeholder=""* * * * *"" id=""{id}_sys_cron"">
						<span class=""input-group-addon btn btn-default js-management-input-CronModal"" title=""Cron Expression Builder"" input-id=""{id}"">
							<span class=""glyphicon glyphicon-wrench""></span>
						</span>
					</div>
				</div>");
			if (job.AllowMultiple)
			{
				WriteLiteral($@"
				<div class=""commands-options CronExpression col-xs-12 col-sm-4"" style=""display:none;"">
							   <div class=""input-group"" id=""{id}_Name"">
						<input type=""text"" class=""form-control"" title="""" placeholder=""Job Name"" id=""{id}_sys_name"" data-original-title=""Give a unique name to your job"" spellcheck=""false"" data-ms-editor=""true"">
					</div>
				</div>
");
			}
			WriteLiteral($@"
				<div class=""commands-panel CronExpression col-xs-12 col-sm-4"" style=""display:none;"">
					<div class=""btn-group"">
						<button class=""btn btn-default btn-sm btn-warning js-management-input-commands"" type=""button"" input-id=""{id}"" input-type=""CronExpression""
								data-confirm=""If this job already has a schedule then it will be updated.  Continue?"" data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">
							<span class=""glyphicon glyphicon-repeat""></span>
							&nbsp;Repeated Execution
						</button>
						<button type=""button"" class=""btn btn-warning btn-sm dropdown-toggle"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""false"">
							<span class=""caret""></span>
						</button>
						<ul class=""dropdown-menu dropdown-menu-right"">
");
			var cronItems = new Dictionary<string, string>() {
							{ "Every Minute", Cron.Minutely() },
							{ "Hourly", Cron.Hourly() },
							{ "Daily", Cron.Daily() },
							{ "Weekly", Cron.Weekly() },
							{ "Monthly", Cron.Monthly() },
							{ "Annually", Cron.Yearly() }
						};
			foreach (var o in cronItems)
			{
				WriteLiteral($@"
								<li>
									<a href=""#"" class=""js-management-input-commands text-right"" input-id=""{id}"" input-type=""CronExpression"" schedule=""{o.Value}""
										data-confirm=""If this job already has a schedule then it will be updated.  Continue?"" data-url=""{Url.To(url)}"" data-loading-text=""{loadingText}"">
										{o.Key}: <span>({o.Value})</span>
									</a>
								</li>
			");
			}
			WriteLiteral($@"
						</ul>
					</div>
				</div>
");
		}

		protected string Input(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, string inputtype, object defaultValue = null, bool isDisabled = false, bool isRequired = false)
		{
			return $@"
<div class=""form-group {cssClasses} {(isRequired ? "required" : "")}"">
		<label for=""{id}"" class=""control-label"">{labelText}</label>
		{(inputtype != "textarea" ? $@"
		<input class=""form-control"" type=""{inputtype}"" placeholder=""{placeholderText}"" id=""{id}"" value=""{defaultValue}"" {(isDisabled ? "disabled='disabled'" : "")} {(isRequired ? "required='required'" : "")} />" : $@"
		<textarea rows=""10"" class=""form-control"" placeholder=""{placeholderText}"" id=""{id}"" {(isDisabled ? "disabled='disabled'" : "")} {(isRequired ? "required='required'" : "")}>{defaultValue}</textarea>")}
		{(!string.IsNullOrWhiteSpace(descriptionText) ? $@"
		<small id=""{id}Help"" class=""form-text text-muted"">{descriptionText}</small>
" : "")}
	</div>";
		}

		protected string InputTextbox(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, object defaultValue = null, bool isDisabled = false, bool isRequired = false)
		{
			return Input(id, cssClasses, labelText, placeholderText, descriptionText, "text", defaultValue, isDisabled, isRequired);
		}

		protected string InputNumberbox(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, object defaultValue = null, bool isDisabled = false, bool isRequired = false)
		{
			return Input(id, cssClasses, labelText, placeholderText, descriptionText, "number", defaultValue, isDisabled, isRequired);
		}

		protected string InputDatebox(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, object defaultValue = null, bool isDisabled = false, bool isRequired = false)
		{
			return $@"
<div class=""form-group {cssClasses} {(isRequired ? "required" : "")}"">
	<label for=""{id}"" class=""control-label"">{labelText}</label>
	<div class='input-group date' id='{id}_datetimepicker'>
		<input type='text' class=""form-control"" placeholder=""{placeholderText}"" value=""{defaultValue}"" {(isDisabled ? "disabled='disabled'" : "")} {(isRequired ? "required='required'" : "")} />
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
		<input class=""form-check-input"" type=""checkbox"" id=""{id}"" {(bDefaultValue ? "checked='checked'" : "")} {(isDisabled ? "disabled='disabled'" : "")} />
		<label class=""form-check-label"" for=""{id}"">{labelText}</label>
	</div>
		{(!string.IsNullOrWhiteSpace(descriptionText) ? $@"
		<small id=""{id}Help"" class=""form-text text-muted"">{descriptionText}</small>
" : "")}
</div>";
		}

		protected string InputDataList(string id, string cssClasses, string labelText, string placeholderText, string descriptionText, Dictionary<string, string> data, string defaultValue = null, bool isDisabled = false)
		{
			var initText = (defaultValue != null ? defaultValue : (!string.IsNullOrWhiteSpace(placeholderText) ? placeholderText : "Select a value"));
			var initValue = (defaultValue != null && data.ContainsKey(defaultValue)) ? data[defaultValue].ToString() : "";
			var output = $@"
<div class=""{cssClasses}"">
	<label class=""control-label"">{labelText}</label>
	<div class=""dropdown"">
		<button id=""{id}"" class=""btn btn-default dropdown-toggle input-control-data-list"" type=""button"" data-selectedvalue=""{initValue}"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""false"" {(isDisabled ? "disabled='disabled'" : "")}>
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
