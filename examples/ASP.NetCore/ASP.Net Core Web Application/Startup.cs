using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Hangfire;
using Hangfire.Dashboard.Management.v2;
using Hangfire.Dashboard.Management.v2.Support;
using Hangfire.MemoryStorage;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ASP.Net_Core_Web_Application
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddHangfire((configuration) => {
				configuration
					.UseMemoryStorage()
					.UseSimpleAssemblyNameTypeSerializer()
					.UseRecommendedSerializerSettings()
					.UseManagementPages(typeof(Startup).Assembly);
			});

			services.AddHangfireServer((options) => {
				var queues = new List<string>();
				queues.Add("default");
				queues.AddRange(JobsHelper.GetAllQueues());

				options.Queues = queues.Distinct().ToArray();
			});

			services.AddRazorPages();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
			}

			app.UseStaticFiles();

			app.UseRouting();


			/* Adding basic CSP Header middleware */
			app.Use(async (context, next) => {
				context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; connect-src 'self' wss://*:*; img-src 'self' data:;");
				await next();
			});

			app.UseEndpoints(endpoints => {
				endpoints.MapRazorPages();
			});

			app.UseHangfireDashboard("/hangfire", new DashboardOptions() {
				DisplayStorageConnectionString = false,
				DashboardTitle = "ASP.Net Core Hangfire Management",
				StatsPollingInterval = 5000
			});
		}
	}
}
