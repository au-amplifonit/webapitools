using System;
using System.Collections.Generic;
using System.Text;

namespace WebAPITools.Models
{
	public class OperationResult<T>
	{
		public T Result { get; set; }
		public string Message { get; set; }
	}
}
