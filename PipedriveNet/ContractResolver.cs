using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PipedriveNet
{
	internal class ContractResolver : DefaultContractResolver
	{
	    private readonly Dictionary<MemberInfo, string> _names = new Dictionary<MemberInfo, string>();
		public ContractResolver() : base()
		{

		}

		protected override string ResolvePropertyName(string propertyName)
		{
			return GetSnakeCase(propertyName);
		}

		protected override string ResolveDictionaryKey(string dictionaryKey)
		{
			// Don't modify dictionary keys - use them as-is from the JSON
			// This prevents issues with custom_fields keys being transformed
			return dictionaryKey;
		}

		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			var lst = new List<JsonProperty>();
			foreach (var member in GetSerializableMembers(type))
			{
				var prop = CreateProperty(member, memberSerialization);
				string customName;
				if (_names.TryGetValue(member, out customName))
				{
					prop.PropertyName = customName;
				}

				// Special handling for custom_fields property
				if (prop.PropertyName == "custom_fields" && prop.PropertyType == typeof(Dictionary<string, object>))
				{
					var originalProvider = prop.ValueProvider;
					prop.ValueProvider = new CustomFieldsValueProvider(originalProvider, this);
				}

				lst.Add(prop);
			}

			return lst;
		}

		//https://gist.github.com/crallen/9238178
		private string GetSnakeCase(string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			var buffer = "";

			for (var i = 0; i < input.Length; i++)
			{
				var isLast = (i == input.Length - 1);
				var isSecondFromLast = (i == input.Length - 2);

				var curr = input[i];
				var next = !isLast ? input[i + 1] : '\0';
				var afterNext = !isSecondFromLast && !isLast ? input[i + 2] : '\0';

				buffer += char.ToLower(curr);

				if (!char.IsDigit(curr) && char.IsUpper(next))
				{
					if (char.IsUpper(curr))
					{
						if (!isLast && !isSecondFromLast && !char.IsUpper(afterNext))
							buffer += "_";
					}
					else
						buffer += "_";
				}

				if (!char.IsDigit(curr) && char.IsDigit(next))
					buffer += "_";
				if (char.IsDigit(curr) && !char.IsDigit(next) && !isLast)
					buffer += "_";
			}

			return buffer;
		}

		public void Register(PropertyInfo property, string key)
		{
			_names[property] = key;
		}

		public string ResolveCustomName(PropertyInfo property)
		{
			return _names[property];
		}

		public HashSet<string> GetMappedCustomFieldKeys()
		{
			// Return all the custom field hash keys that are mapped to properties
			return new HashSet<string>(_names.Values);
		}
	}

	/// <summary>
	/// Value provider that filters custom_fields dictionary to exclude keys already mapped to properties
	/// </summary>
	internal class CustomFieldsValueProvider : IValueProvider
	{
		private readonly IValueProvider _innerProvider;
		private readonly ContractResolver _resolver;

		public CustomFieldsValueProvider(IValueProvider innerProvider, ContractResolver resolver)
		{
			_innerProvider = innerProvider;
			_resolver = resolver;
		}

		public object GetValue(object target)
		{
			var value = _innerProvider.GetValue(target);
			return value;
		}

		public void SetValue(object target, object value)
		{
			// Filter the dictionary value to remove keys that are already mapped to properties
			if (value is Dictionary<string, object> dict)
			{
				var mappedKeys = _resolver.GetMappedCustomFieldKeys();
				var filteredDict = new Dictionary<string, object>();

				foreach (var kvp in dict)
				{
					// Only include keys that are NOT mapped to properties
					if (!mappedKeys.Contains(kvp.Key))
					{
						filteredDict[kvp.Key] = kvp.Value;
					}
				}

				_innerProvider.SetValue(target, filteredDict);
			}
			else
			{
				_innerProvider.SetValue(target, value);
			}
		}
	}
}