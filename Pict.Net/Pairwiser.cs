using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Pict.Net
{
	public partial class Pairwiser
	{
		readonly List<IModelParameter> parameters = new List<IModelParameter>();
		readonly List<IReadOnlyCollection<KeyValuePair<IModelParameter, IModelValue>>> exclusions = new List<IReadOnlyCollection<KeyValuePair<IModelParameter, IModelValue>>>();

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
			return PairwiserHelper.Generate(parameters, exclusions).Select(pairs =>
			{
				return parameterNames.Select(name => pairs.Single(pair => pair.Key.ParameterName == name).Value.Value).ToArray();
			}).ToArray();
		}

		Pairwiser AddConstraint(string[] parameterNames, Delegate constraint)
		{
			var ps = parameterNames.Select(name => parameters.Single(p => p.ParameterName == name)).ToArray();
			return AddConstraint(ps, constraint);
		}

		Pairwiser AddConstraint(LambdaExpression constraint)
		{
			var ps = constraint.Parameters.Select(p => FindParameterByPattern(p.Name)).ToArray();
			return AddConstraint(ps, constraint.Compile());
		}

		Pairwiser AddConstraint(IReadOnlyList<IModelParameter> parameters, Delegate constraint)
		{
			var crossed = Cross(parameters);
			var a = crossed.ToArray();
			var b = a.First();
			var excludeValuesSeq = crossed.Where(args => !(bool)constraint.DynamicInvoke(args.Select(x => x.Value).ToArray()));
			exclusions.AddRange(excludeValuesSeq.Select(vs => parameters.Zip(vs, (k, v) => new KeyValuePair<IModelParameter, IModelValue>(k, v)).ToArray()));
			return this;
		}


		IModelParameter FindParameterByPattern(string pattern)
		{
			var regex = new Regex(
				"^" + new string(
					pattern
						.SelectMany(c => c == '_' ? "." : $"{c}")
						.ToArray()
				) + "$"
			);

			var hits = parameters.Where(p => regex.IsMatch(p.ParameterName)).ToArray();
			if (hits.Length > 1)
			{
				throw new InvalidOperationException();
			}
			else if (hits.Length <= 0)
			{
				throw new InvalidOperationException();
			}
			else
			{
				return hits.Single();
			}
		}

		IEnumerable<IModelValue[]> Cross(IReadOnlyList<IModelParameter> ps)
		{
			if (ps.Count == 0) throw new ArgumentException();

			var pivotParameter = ps.First();
			if (ps.Count == 1)
			{
				foreach (var value in pivotParameter.Values)
				{
					yield return new[] { value };
				}
			}
			else
			{
				foreach (var value in pivotParameter.Values)
				{
					foreach (var tail in Cross(ps.Skip(1).ToArray()))
					{
						yield return tail.Prepend(value).ToArray();
					}
				}
			}
		}
	}


}
