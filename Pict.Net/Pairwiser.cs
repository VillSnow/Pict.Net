using System.Collections.Generic;
using System.Linq;

namespace Pict.Net
{
	public class Pairwiser
	{
		readonly List<IModelParameter> parameters = new List<IModelParameter>();


		public Pairwiser AddParameter<T>(string parameterName, IEnumerable<T> values)
		{
			return AddParameter(parameterName, values.Select(ModelValue.Create));
		}

		public Pairwiser AddParameter<T>(string parameterName, IEnumerable<ModelValue<T>> values)
		{
			parameters.Add(new ModelParameter<T>(parameterName, values));
			return this;
		}

		public IReadOnlyList<object[]> GetCases(params string[] parameterNames)
		{
			return PairwiserHelper.Generate(parameters).Select(pairs => {
				return parameterNames.Select(name => pairs.Single(pair => pair.Key.ParameterName == name).Value.Value).ToArray();
			}).ToArray();
		}
		
	}


}
