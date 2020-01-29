using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebAPITools.Helpers
{
	public class QueryHelper
	{
		public static IQueryable<T> GetPageItems<T>(IQueryable<T> itemQuery, int pageSize, int pageNumber)
		{
			return itemQuery.Skip(pageSize * pageNumber).Take(pageSize);
		}

		public static void AddWhereCondition(StringBuilder whereConditionBuilder, string whereCondition)
		{
			if (whereConditionBuilder.Length == 0)
				whereConditionBuilder.Append(" WHERE ");
			else
				whereConditionBuilder.Append(" AND ");
			whereConditionBuilder.Append($" {whereCondition}");
		}
	}
}
