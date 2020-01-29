using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebAPITools.Cors
{
	public class CorsHandlingMiddleware
	{
		private readonly RequestDelegate _next;

		public CorsHandlingMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public Task Invoke(HttpContext context)
		{
			return BeginInvoke(context);
		}

		private Task BeginInvoke(HttpContext context)
		{
			context.Response.Headers.Add("Access-Control-Allow-Origin", new[] { (string)context.Request.Headers["Origin"] });
			/*
			if (context.Request.Method == "OPTIONS")
			{
				context.Response.Headers.Add("Access-Control-Allow-Headers", new[] { "Origin, X-Requested-With, Content-Type, Accept" });
				context.Response.Headers.Add("Access-Control-Allow-Methods", new[] { "GET, POST, PUT, DELETE, OPTIONS" });
				context.Response.Headers.Add("Access-Control-Allow-Credentials", new[] { "true" });
				context.Response.StatusCode = StatusCodes.Status204NoContent;
				return context.Response.WriteAsync(string.Empty);
			}
			*/
			return _next.Invoke(context);
		}
	}
	/*	
	public static class OptionsMiddlewareExtensions
	{
		public static IApplicationBuilder UseOptions(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<CorsHandlingMiddleware>();
		}
	}
	*/
}

