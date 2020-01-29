using System;
using System.Collections.Generic;
using System.Text;

namespace WebAPITools.EntityMapper
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
	public class FieldMapperAttribute: Attribute
	{
		public string SourceFieldName { get; }
		public bool TrimString { get; }
		public Type EntityType { get; }
		public bool ReadOnly { get; set; }
		public bool IsArray { get; set; }
		public int ArrayMaxRank { get; set; }
		public object DefaultValue { get; set; }


		public FieldMapperAttribute(string sourceFieldName, Type entityType = null, bool trimString = true, bool readOnly = false)
		{
			SourceFieldName = sourceFieldName;
			EntityType = entityType;
			TrimString = trimString;
			ReadOnly = readOnly;
		}
	}
}
