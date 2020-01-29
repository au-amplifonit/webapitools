using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPITools.Models
{
	public class QueryResult<T>
	{
		public int RecordCount { get; set; }
		public int PageCount { get; set; }
		public int PageRequested { get; set; }

		public IEnumerable<T> PageItems { get; set; }

		public QueryResult(int pageSize, int recordCount, int pageNumber, IEnumerable<T> pageItems)
		{
			RecordCount = recordCount;
			PageRequested = pageNumber;
			PageCount = recordCount / pageSize + (recordCount % pageSize == 0 ? 0 : 1);
			PageItems = pageItems;
		}
	}
}
