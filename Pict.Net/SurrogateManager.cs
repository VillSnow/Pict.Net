using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pict.Net
{
	class SurrogateManager
	{
		readonly IReadOnlyCollection< KeyValuePair<IModelValue, string>> list;

		public SurrogateManager(IReadOnlyCollection<IModelParameter> parameters)
		{
			var objectValues = parameters.SelectMany(p => p.Values);
			list = objectValues.Select((value, i) => new KeyValuePair<IModelValue, string>(value, $"{(value.Negative ? "~" : "")}v{i}")).ToArray();
		}

		public string Getsurrogate(IModelValue modelValue)
		{
			return list.Where(pair => ReferenceEquals(pair.Key, modelValue)).Select(pair => pair.Value).Single();
		}
		
		public IModelValue GetModelValue(string surrogate)
		{
			return list.Where(pair => pair.Value == surrogate).Select(pair => pair.k).Single();
		}

	}
}
