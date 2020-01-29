using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using WebAPITools.Models;

namespace WebAPITools.Helpers
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class EnumValueAttribute : Attribute
	{
		public string Value { get; }

		public EnumValueAttribute(string value)
		{
			Value = value;
		}
	}

	public static class EnumHelper
	{
		public static string GetDescription<T>(this T e) where T : IConvertible
		{
			if (e is Enum)
			{
				Type type = e.GetType();
				Array values = System.Enum.GetValues(type);

				foreach (int val in values)
				{
					if (val == e.ToInt32(CultureInfo.InvariantCulture))
					{
						var memInfo = type.GetMember(type.GetEnumName(val));

						var descriptionAttribute = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
						if (descriptionAttribute != null)
							return descriptionAttribute.Description;

					}
				}
			}

			return null; // could also return string.Empty
		}


		public static string GetEnumValue<T>(this T e) where T : IConvertible
		{
			if (e is Enum)
			{
				Type type = e.GetType();
				Array values = System.Enum.GetValues(type);

				foreach (int val in values)
				{
					if (val == e.ToInt32(CultureInfo.InvariantCulture))
					{
						var memInfo = type.GetMember(type.GetEnumName(val));

						var attribute = memInfo[0].GetCustomAttributes(typeof(EnumValueAttribute), false).FirstOrDefault() as EnumValueAttribute;
						if (attribute != null)
							return attribute.Value;

					}
				}
			}

			return null; // could also return string.Empty
		}

		public static T GetEnumFromValue<T>(string EnumValue, T defaultValue) where T : Enum
		{
			Type EnumType = typeof(T);
			Array values = Enum.GetValues(EnumType);

			foreach (int val in values)
			{
				var memInfo = EnumType.GetMember(EnumType.GetEnumName(val));

				var attribute = memInfo[0].GetCustomAttributes(typeof(EnumValueAttribute), false).FirstOrDefault() as EnumValueAttribute;
				if (attribute != null && attribute.Value == EnumValue)
					return (T)Enum.ToObject(EnumType, val);
			}
			return defaultValue;
		}


		public static List<SimpleModel> GetEnumItems<T>()
		{
			List<SimpleModel> Result = new List<SimpleModel>();
			Type enumType = typeof(T);
			Array values = System.Enum.GetValues(enumType);
			foreach (int val in values)
			{
				var memInfo = enumType.GetMember(enumType.GetEnumName(val));

				var valueAttr = memInfo[0].GetCustomAttributes(typeof(EnumValueAttribute), false).FirstOrDefault() as EnumValueAttribute;
				var descriptionAttribute = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;

				SimpleModel model = new SimpleModel()
				{
					Value = valueAttr != null ? valueAttr.Value : null,
					Description = descriptionAttribute != null ? descriptionAttribute.Description : null,
				};
				Result.Add(model);
			}

			return Result;
		}
	}
}
