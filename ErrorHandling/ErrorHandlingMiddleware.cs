using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebAPITools.Models.Configuration;

namespace WebAPITools.ErrorHandling
{

	public class ErrorHandlingMiddleware
	{
		private readonly RequestDelegate next;

		public ErrorHandlingMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public async Task Invoke(HttpContext context, IHostingEnvironment env, ILogger<ErrorHandlingMiddleware> logger, IOptions<AppSettings> ASettings /* other dependencies */)
		{
			try
			{
				await next(context);
			}
			catch (Exception ex)
			{
				await HandleExceptionAsync(context, env, logger, ex, ASettings);
			}
		}

		private static Task HandleExceptionAsync(HttpContext context, IHostingEnvironment env, ILogger<ErrorHandlingMiddleware> logger, Exception exception, IOptions<AppSettings> ASettings)
		{
			logger.LogError(exception, exception.Message);

			HttpStatusCode code = HttpStatusCode.InternalServerError; // 500 if unexpected
			//string Message = env.IsDevelopment() ? exception.Message : "Internal server error";
			string Message = "Internal server error";
			if (env.IsDevelopment() || ASettings.Value.ShowErrorDetails)
			{
				Message = exception.Message;
				Exception innerException = exception.InnerException;
				while (innerException != null)
				{
					Message += "\nInner Exception: " + innerException.Message;
					innerException = innerException.InnerException;
				}
			}

			if (exception is NotFoundException)
				code = HttpStatusCode.NotFound;
			else if (exception is ArgumentException || exception is ArgumentNullException || exception is ArgumentOutOfRangeException || exception is InvalidOperationException)
				code = HttpStatusCode.BadRequest;
			/*
			else if (exception is MyUnauthorizedException) code = HttpStatusCode.Unauthorized;
			else if (exception is MyException) code = HttpStatusCode.BadRequest;
			*/
			var result = JsonConvert.SerializeObject(new { error = Message });
			context.Response.ContentType = "application/json";
			context.Response.StatusCode = (int)code;
			return context.Response.WriteAsync(result);
		}
	}
}
