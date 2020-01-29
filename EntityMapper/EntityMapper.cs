using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using WebAPITools.Models;
using WebAPITools.Models.Configuration;

namespace WebAPITools.EntityMapper
{
	public static class EntityMapper
	{
		public static AppSettings Settings { get; set; }

		public static T Map<T, E>(DbContext context, object entity, object entityExt = null) where T: ModelBase, new()
		{
			if (entity == null)
				throw new ArgumentNullException("Entity cannot be null");

			T Result = new T();
			if (entityExt != null)
				UpdateModel<T>(Result, entityExt);
			UpdateModel<T>(Result, entity);

			Result.LoadData<E>(context, (E)entity);

			return Result;
		}

		public static void UpdateModel<T>(T model, object entity, bool strictBinding = false) where T : ModelBase
		{
			foreach (PropertyInfo PI in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField))
			{
				IEnumerable<FieldMapperAttribute> attributes = PI.GetCustomAttributes<FieldMapperAttribute>(true);
				FieldMapperAttribute attribute = attributes.FirstOrDefault(E => E.EntityType == entity.GetType() || (E.EntityType != null && entity.GetType().Name == E.EntityType.Name+"Proxy"));
				if (attribute == null && !strictBinding)
					attribute = attributes.FirstOrDefault(E => E.EntityType == null);

				if (attribute != null)
				{
					if (attribute.IsArray)
					{
						List<string> modelValue = new List<string>();
						for (int index = 0; index < attribute.ArrayMaxRank; index++)
						{
							PropertyInfo entityPI = entity.GetType().GetProperty(string.Format("{0}{1}", attribute.SourceFieldName, index+1));
							if (entityPI != null)
							{
								object entityValue = entityPI.GetValue(entity);
								modelValue.Add(entityValue?.ToString());
							}
						}
						PI.SetValue(model, modelValue.ToArray());
					}
					else
					{
						PropertyInfo entityPI = entity.GetType().GetProperty(attribute.SourceFieldName);
						if (entityPI != null)
						{
							object entityValue = entityPI.GetValue(entity);
							object targetValue = entityValue;

							if (PI.PropertyType == typeof(bool) && entityPI.PropertyType == typeof(string))
								targetValue = (string)entityValue == "Y";
							else if (PI.PropertyType == typeof(bool?) && entityPI.PropertyType == typeof(string))
							{
								if (entityValue == null)
									targetValue = null;
								else
									targetValue = (string)entityValue == "Y";
							}
							else if (PI.PropertyType == typeof(bool) && entityPI.PropertyType == typeof(int))
								targetValue = (int)entityValue == 1;
							else if (PI.PropertyType == typeof(bool?) && entityPI.PropertyType == typeof(int?))
							{
								if (entityValue == null)
									targetValue = null;
								targetValue = (int)entityValue == 1;
							}
							else if (PI.PropertyType == typeof(string) && entityPI.PropertyType == typeof(string) && attribute.TrimString)
								targetValue = targetValue != null ? ((string)targetValue).Trim() : null;

							PI.SetValue(model, targetValue);
						}
					}
				}
			}
		}

		public static void UpdateEntity(ModelBase model, object entity, object entityExt)
		{
			if (model == null)
				throw new ArgumentNullException("Model cannot be null");
			if (entity == null)
				throw new ArgumentNullException("Entity cannot be null");

			UpdateEntity(model, entity);
			if (entityExt != null)
				UpdateEntity(model, entityExt, false);
		}


		public static void UpdateEntity(ModelBase model, object entity, bool strictBinding = false)
		{
			foreach (PropertyInfo PI in model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				IEnumerable<FieldMapperAttribute> attributes = PI.GetCustomAttributes<FieldMapperAttribute>(true);
				FieldMapperAttribute attribute = attributes.FirstOrDefault(E => E.EntityType == entity.GetType() || (E.EntityType != null && entity.GetType().Name == E.EntityType.Name + "Proxy"));
				if (attribute == null && !strictBinding)
					attribute = attributes.FirstOrDefault(E => E.EntityType == null);

				if (attribute != null && !attribute.ReadOnly)
				{
					if (attribute.IsArray)
					{
						object[] modelValue = (object[])PI.GetValue(model);
						if (modelValue != null)
						{
							int MaxLen = Math.Min(attribute.ArrayMaxRank, modelValue.Length);
							for (int index = 0; index < MaxLen; index++)
								UpdateEntityArrayField(entity, attribute.SourceFieldName, index + 1, modelValue[index] ?? attribute.DefaultValue);
						}
						else
							for (int index = 0; index < attribute.ArrayMaxRank; index++)
								UpdateEntityArrayField(entity, attribute.SourceFieldName, index + 1, attribute.DefaultValue);
					}
					else
					{
						PropertyInfo entityPI = entity.GetType().GetProperty(attribute.SourceFieldName);

						if (entityPI != null)
						{
							object modelValue = PI.GetValue(model);
							object entityValue = modelValue ?? attribute.DefaultValue;

							if (PI.PropertyType == typeof(bool) && entityPI.PropertyType == typeof(string))
							{
								entityValue = (bool)modelValue ? "Y" : "N";
							}
							else if (PI.PropertyType == typeof(bool?) && entityPI.PropertyType == typeof(string))
							{
								if (PI.PropertyType == typeof(bool?) && modelValue != null)
									entityValue = (bool)modelValue ? "Y" : "N";
							}
							/*
							else if (PI.PropertyType == typeof(string) && entityPI.PropertyType == typeof(string))
								entityValue = CheckStringLength(entity, entityPI, (string)entityValue);
							*/
							entityPI.SetValue(entity, entityValue);
						}
					}
				}
			}
			UpdateEntityStandardFields(entity);
		}

		public static T CreateEntity<T>() where T: new()
		{
			T Result = new T();
			InitializeEntityStandardFields(Result);
			return Result;
		}

		public static void InitializeEntityStandardFields(dynamic entity)
		{
			if (Settings == null)
				throw new Exception("Settings cannot be null");

			entity.COMPANY_CODE = Settings.CompanyCode;
			entity.DIVISION_CODE = Settings.DivisionCode;
			entity.DT_INSERT = entity.DT_UPDATE = DateTime.UtcNow;
			entity.USERINSERT = entity.USERUPDATE = Settings.Username;
			entity.ROWGUID = Guid.NewGuid();
		}

		public static void UpdateEntityStandardFields(dynamic entity)
		{
			entity.DT_UPDATE = DateTime.UtcNow;
			entity.USERUPDATE = Settings.Username;
		}

		public static void CheckEntityRowId(dynamic entity, dynamic entityExt, Guid rowId)
		{
			if (entity.ROWGUID == Guid.Empty)
				entity.ROWGUID = rowId;
			if (entityExt != null && entity.ROWGUID == Guid.Empty)
				entityExt.ROWGUID = rowId;
		}

		public static void UpdateEntityArrayField(object entity, string fieldName, int fieldIndex, object value)
		{
			PropertyInfo PI = entity.GetType().GetProperty(string.Format("{0}{1}", fieldName, fieldIndex), BindingFlags.Public | BindingFlags.Instance);
			if (PI != null)
			{
				/*
					if (value != null && value.GetType() == typeof(string) && PI.PropertyType == typeof(string))
						value = CheckStringLength(entity, PI, (string)value);
				*/
				PI.SetValue(entity, value);
			}
		}

		public static void UpdateModelArrayField<T>(object model, string fieldName, int fieldIndex, object value) where T : ModelBase
		{
			PropertyInfo PI = model.GetType().GetProperty(string.Format("{0}{1}", fieldName, fieldIndex), BindingFlags.Public | BindingFlags.Instance);
			if (PI != null)
				PI.SetValue(model, value);
		}

		private static string CheckStringLength(object entity, PropertyInfo propInfo, string value)
		{
			ColumnAttribute attribute = propInfo.GetCustomAttribute<ColumnAttribute>(true);
			if (attribute != null)
			{
				string dbType = attribute.TypeName.ToLower();
				if (dbType.Contains("varchar"))
				{
					int numberStartIndex = dbType.IndexOf("varchar(") + 8;
					int numberEndIndex = dbType.IndexOf(")", numberStartIndex);
					string lengthString = dbType.Substring(numberStartIndex, (numberEndIndex - numberStartIndex));
					int maxLength = 0;
					int.TryParse(lengthString, out maxLength);

					if (!string.IsNullOrEmpty(value) && value.Length > maxLength && lengthString != "max")
						return value.Substring(0, maxLength);

					return value;
				}
			}
			throw new ArgumentException($"Unable to calculate length for field: {propInfo.Name}");
		}

	}
}
