using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pict.Net
{
	class SurrogateManager
	{
		readonly IReadOnlyList<IModelParameter> parameters;

		public SurrogateManager(IReadOnlyList<IModelParameter> parameters)
		{
			this.parameters = parameters.ToArray();
		}

		public string GetSurrogate(IModelValue modelValue)
		{
			var name = parameters.SelectMany(
				(p, i) => p.Values.SelectMany(
					(v, j) => ReferenceEquals(v, modelValue) ? new[] { $"p{i}v{j}" } : new string[0]
				)
			).Single();

			if (modelValue.Negative) return "~" + name;
			else return name;
		}

		public string GetSurrogate(IModelParameter modelParameter)
		{
			var name = parameters.SelectMany(
				(p, i) => ReferenceEquals(p, modelParameter) ? new[] { $"p{i}" } : new string[0]
			).Single();

			return name;
		}

		static readonly Regex ModelValueSurrogateRegex = new Regex(@"~?p(?<i>\d+)v(?<j>\d+)");

		public IModelValue GetModelValue(string surrogate)
		{
			var match = ModelValueSurrogateRegex.Match(surrogate);
			if (!match.Success) throw new FormatException();

			int i = int.Parse(match.Groups["i"].Value);
			int j = int.Parse(match.Groups["j"].Value);

			return parameters[i].Values[j];
		}

		static readonly Regex ModelParameterSurrogateRegex = new Regex(@"~?p(?<i>\d+)");

		public IModelParameter GetModelParameter(string surrogate)
		{
			var match = ModelParameterSurrogateRegex.Match(surrogate);
			if (!match.Success) throw new FormatException();

			int i = int.Parse(match.Groups["i"].Value);

			return parameters[i];
		}

		public IModelParameter GetParentModelParameter(IModelValue modelValue)
		{
			return parameters.Single(p => p.Values.Any(v => ReferenceEquals(modelValue, v)));
		}
	}
}
