using System.Collections.Generic;
using System.Linq;

namespace Pict.Net
{
	public class Pairwiser
	{
		readonly List<IModelParameter> parameters = new List<IModelParameter>();


		public Pairwiser AddParameter<T>(string parameterName, IEnumerable<T> values)
		{
			parameters.Add(new ModelParameter<T>(parameterName, values.Select(ModelValue.Create)));
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
