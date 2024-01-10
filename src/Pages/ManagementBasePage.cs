using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.Dashboard;
using Hangfire.Dashboard.Management.v2.Metadata;
using Hangfire.Dashboard.Management.v2.Support;
using Hangfire.Dashboard.Pages;
using Hangfire.Server;
using Hangfire.States;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hangfire.Dashboard.Management.v2.Pages
{
	partial class ManagementBasePage
	{
		public readonly string menuName;

		public readonly IEnumerable<JobMetadata> jobs;
		public readonly Dictionary<string, string> jobSections;


		protected internal ManagementBasePage(string menuName) : base()
		{

			//this.UrlHelper = new UrlHelper(this.Context);
			this.menuName = menuName;

			jobs = JobsHelper.Metadata.Where(j => j.MenuName.Contains(menuName)).OrderBy(x => x.SectionTitle).ThenBy(x => x.Name);
			jobSections = jobs.Select(j => j.SectionTitle).Distinct().ToDictionary(k => k, v => string.Empty);
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
							item = formInput == null ? DateTime.MinValue : DateTime.Parse(formInput, null, DateTimeStyles.RoundtripKind);
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
									var cron = GetFormVariable($"{id}_sys_cron");
									var name = GetFormVariable($"{id}_sys_name");

									if (string.IsNullOrWhiteSpace(cron))
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
										manager.AddOrUpdate(jobId, job, cron, TimeZoneInfo.Local, jobMetadata.Queue);
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

									if (!DateTime.TryParse(datetime, null, DateTimeStyles.RoundtripKind, out DateTime dt))
									{
										errorMessage = "Unable to parse Schedule";
										break;
									}
									try
									{
										var jobId = client.Create(job, new ScheduledState(dt.ToUniversalTime()));//Queue
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
									var timeSpan = GetFormVariable($"{id}_sys_timespan");

									if (string.IsNullOrWhiteSpace(timeSpan))
									{
										errorMessage = $"No Delay Defined '{id}'";
										break;
									}

									if (!TimeSpan.TryParse(timeSpan, out TimeSpan dt))
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
	}
}
