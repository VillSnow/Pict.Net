using System.Collections.Generic;
using System.Linq;

namespace Pict.Net
{
	public class Pairwiser
	{
		readonly List<KeyValuePair<object, object>> values = new List<KeyValuePair<object, object>>();


		public Pairwiser AddParameter<T>(string parameterName, IEnumerable<T> values)
		{
			this.values.AddRange(values.Select(v => new KeyValuePair<object, object>(parameterName, v)));
			return this;
		}

		public IReadOnlyList<object[]> GenerateValues(int order, IReadOnlyList<object> parameters)
		{
			var targetValues = values.Where(pair => parameters.Contains(pair.Key));
			return PairwiserHelper.Generate(targetValues, order).Select(
				pairs => parameters.Select(p => pairs.Single(pair => p.Equals(pair.Key)).Value).ToArray()
			).ToArray();
		}

		public IReadOnlyList<object[]> GenerateValues(int order, params object[] parameters)
			=> GenerateValues(order, (IReadOnlyList<object>)parameters);

		public IReadOnlyList<object[]> GenerateValues(IReadOnlyList<object> parameters)
			=> GenerateValues(2, parameters);

		public IReadOnlyList<object[]> GenerateValues(params object[] parameters)
			=> GenerateValues(2, (IReadOnlyList<object>)parameters);
	}



}
