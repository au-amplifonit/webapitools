using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebAPITools;
using WebAPITools.Cors;
using WebAPITools.ErrorHandling;
using WebAPITools.Models.Configuration;

namespace WebAPITools
{
	public class BaseStartup
	{
		protected readonly ILogger<BaseStartup> _logger;
		public BaseStartup(IConfiguration configuration, ILogger<BaseStartup> logger, ILoggerFactory loggerFactory)
		{
			Configuration = configuration;
			_logger = logger;
			loggerFactory.AddFile(configuration.GetSection("Logging"));
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public virtual void ConfigureServices(IServiceCollection services)
		{
			services.AddCors(options =>
			{
				options.AddDefaultPolicy(
						builder =>
						{

							builder.AllowAnyOrigin()
										.AllowAnyHeader()
										.AllowAnyMethod()
										.AllowCredentials();
						});
			});

			services.Configure<IISOptions>(options =>
			{
				options.AutomaticAuthentication = true;
			});
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddAuthentication(IISDefaults.AuthenticationScheme);

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			// Add functionality to inject IOptions<T>
			services.AddOptions();

			services.AddMvc()
				 .AddJsonOptions(
						 options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
				 );

			// Add our Config object so it can be injected
			services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
			services.Configure<SwaggerSettings>(Configuration.GetSection("SwaggerSettings"));
			string ConnectionString = Configuration.GetConnectionString("ConnectionString");
			DbConnectionStringBuilder ConnBuilder = new DbConnectionStringBuilder();
			ConnBuilder.ConnectionString = ConnectionString;
			_logger.LogInformation("Using database: {0}\\{1}", ConnBuilder["Data Source"], ConnBuilder["Initial Catalog"]);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			try
			{
				_logger.LogInformation("Using environment: {0}", env.EnvironmentName);
				var settings = Configuration.GetSection(nameof(SwaggerSettings)).Get<SwaggerSettings>();
				//app.UseOptions();

				// Enable middleware to serve generated Swagger as a JSON endpoint.
				app.UseSwagger();

				if (env.IsDevelopment())
				{
					app.UseDeveloperExceptionPage();
					// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
					// specifying the Swagger JSON endpoint.
					app.UseSwaggerUI(c =>
					{
						c.SwaggerEndpoint("/swagger/v1/swagger.json", settings.Title);
					});
				}
				else
				{
					app.UseHsts();
					// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
					// specifying the Swagger JSON endpoint.
					app.UseSwaggerUI(c =>
					{
						c.SwaggerEndpoint("v1/swagger.json", settings.Title);
					});
				}

				app.UseAuthentication();
				app.UseHttpsRedirection();
				app.UseMiddleware(typeof(ErrorHandlingMiddleware));
				app.UseMiddleware(typeof(CorsHandlingMiddleware));
				app.UseMvc();
			}
			catch (Exception E)
			{
				_logger.LogError(E, E.Message);
			}
		}
	}
}
