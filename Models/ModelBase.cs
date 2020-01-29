using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPITools.EntityMapper;

namespace WebAPITools.Models
{
	public class ModelBase
	{
		[JsonProperty(Order = 100)]
		[FieldMapper("ROWGUID", ReadOnly = true)]
		public Guid RowGuid { get; set; }

		public ModelBase()
		{

		}

		public ModelBase(Guid ARowGuid) : this()
		{
			RowGuid = ARowGuid;
		}

		public virtual void LoadData<T>(DbContext context, dynamic entity) 
		{
		}

		public virtual void SaveData<T>(DbContext context, dynamic entity)
		{
		}
	}
}
