using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPITools.Models.Configuration
{
	public class AppSettings
	{
		const string DefaultUserName = "Microservices";

		public string CompanyCode { get; set; }
		public string DivisionCode { get; set; }
		public string CountryCode { get; set; } = "AUS";
		public int MaxQueryResult { get; set; } = 100;
		public int CommandTimeout { get; set; } = 180;
		public bool ShowErrorDetails { get; set; } = true;

		public string Username { get; set; } = DefaultUserName;

		public void Configure(IHttpContextAccessor httpContextAccessor)
		{
			if (httpContextAccessor != null && httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
			{
				Username = httpContextAccessor?.HttpContext.User.Identity.Name ?? DefaultUserName;
			}
		}
	}
}
