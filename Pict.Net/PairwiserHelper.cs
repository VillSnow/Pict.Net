using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Pict.Net
{
	public static class PairwiserHelper
	{
		[Obsolete]
		public static IReadOnlyList<KeyValuePair<object, object>[]> Generate(IEnumerable<KeyValuePair<object, object>> model, int order = 2)
		{
			var parameters = ToParameters(model);
			var parameterDict = parameters.ToDictionary(p => p.Name, p => p.Key);
			var parameterValueDict = parameters
				.SelectMany(p => p.Values)
				.ToDictionary(v => v.Name, v => v.Value);

			var text = CreateModelText(parameters);
			var output = RunPict(text, $"/O:{order}");
			var parsedOutput = ParseOutput(output);

			return ParseOutput(output).Select(
				line => line.Select(pair => new KeyValuePair<object, object>(parameterDict[pair.Key], parameterValueDict[pair.Value])).ToArray()
			).ToArray();
		}

		public static IReadOnlyList<KeyValuePair<IModelParameter, IModelValue>[]> Generate(IReadOnlyList<IModelParameter> parameters)
		{
			if (ExistsDuplicatedValueReference(parameters.SelectMany(p => p.Values).ToArray()))
			{
				throw new ArgumentException("!!! Detected Duplicated Values !!!");
			}

			var surrogateMgr = new SurrogateManager(parameters);

			var modelText = CreateModelText(parameters, surrogateMgr);
			var pictOutput = RunPict(modelText, "");
			var parsedOutput = ParseOutput(pictOutput);

			IModelParameter parameterSolver(string name)
			{
				return parameters.Single(p => p.ParameterName == name);
			}

			return parsedOutput.Select(
				@case => @case.Select(
					pair => RestoreCase(pair.Key, pair.Value, parameterSolver, surrogateMgr)
				).ToArray()
			).ToArray();
		}

		[Obsolete]
		static IReadOnlyList<Parameter> ToParameters(IEnumerable<KeyValuePair<object, object>> model)
		{
			var groups = model.GroupBy(pair => pair.Key, pair => pair.Value);

			return groups.Select(
				(g, i) => new Parameter
				{
					Key = g.Key,
					Name = $"P{i}",
					Values = g.Select(
						(v, j) => new ParameterValue
						{
							Value = v,
							Name = $"P{i}V{j}"
						}
					).ToList()
				}
			).ToList();

		}

		static bool ExistsDuplicatedValueReference(IReadOnlyCollection<object> collection)
		{
			var join =
				from x in collection
				select Enumerable.Count(
					from y in collection
					where ReferenceEquals(x, y)
					select 1
				);
			return join.Any(n => n > 1);
		}

		static IReadOnlyCollection<KeyValuePair<IModelValue, string>> CreateSurrogateNames(IReadOnlyCollection<IModelParameter> parameters)
		{

			var objectValues = parameters.SelectMany(p => p.Values);

			var result = objectValues.Select((value, i) => new KeyValuePair<IModelValue, string>(value, $"{(value.Negative ? "~" : "")}v{i}")).ToArray();

			return result;
		}

		[Obsolete]
		static IEnumerable<string> CreateModelText(IReadOnlyList<Parameter> parameters)
		{
			foreach (var parameter in parameters)
			{
				yield return parameter.Name + ":" + string.Join(",", parameter.Values.Select(x => x.Name));
			}
		}

		static IEnumerable<string> CreateModelText(IEnumerable<IModelParameter> parameters, SurrogateManager surrogateManager)
		{
			foreach (var parameter in parameters)
			{
				if (parameter.IsNumber)
				{
					yield return parameter.ParameterName + ":" + string.Join(",", parameter.Values.Select(x => x.ToValueString()));
				}
				else
				{
					yield return parameter.ParameterName + ":" + string.Join(",", parameter.Values.Select(surrogateManager.Getsurrogate));
				}
			}
		}

		static IReadOnlyList<string> RunPict(IEnumerable<string> model, string options)
		{
			var stdout = new List<string>();
			var stderr = new List<string>();
			using (var modelPath = new TempFile())
			{
				File.WriteAllLines(modelPath.Path, model);

				var psi = new ProcessStartInfo
				{
					WindowStyle = ProcessWindowStyle.Hidden,
					FileName = "pict.exe",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					Arguments = modelPath.Path + " " + options
				};
				var process = Process.Start(psi);

				process.ErrorDataReceived += (sender, e) => { if (e.Data is string) { stderr.Add(e.Data); } };
				process.BeginErrorReadLine();

				process.OutputDataReceived += (sender, e) => { if (e.Data is string) { stdout.Add(e.Data); } };
				process.BeginOutputReadLine();

				process.WaitForExit();
				if (process.ExitCode != 0)
				{
					throw new Exception($"ExitCode={process.ExitCode}\r\n{string.Join(Environment.NewLine, stderr)}");
				}

				process.CancelOutputRead();
				process.CancelErrorRead();
			}

			return stdout;
		}

		static IEnumerable<IReadOnlyList<KeyValuePair<string, string>>> ParseOutput(IEnumerable<string> output)
		{
			IReadOnlyList<string> header = null;

			foreach (var line in output)
			{
				if (header is null)
				{
					header = line.Split('\t');
				}
				else
				{
					var values = line.Split('\t');
					yield return header.Zip(values, (h, v) => new KeyValuePair<string, string>(h, v)).ToList();
				}
			}
		}

		static KeyValuePair<IModelParameter, IModelValue> RestoreCase(string header, string value, Func<string, IModelParameter> parameterSolver, SurrogateManager surrogateManager)
		{
			var parameter = parameterSolver(header);

			if (parameter.IsNumber)
			{
				bool negative = value[0] == '~';
				string body = negative ? value.Substring(1) : value;

				object valueObject = Convert.ChangeType(body, parameter.ValueType);
				var ctor = typeof(ModelValue<>).MakeGenericType(parameter.ValueType).GetConstructor(new[] { parameter.ValueType, typeof(bool) });
				var modelValue = (IModelValue)ctor.Invoke(new object[] { valueObject, negative });
				return new KeyValuePair<IModelParameter, IModelValue>(parameter, modelValue);
			}
			else
			{
				var modelValue = surrogateManager.GetModelValue(value);
				return new KeyValuePair<IModelParameter, IModelValue>(parameter, modelValue);
			}
		}

		[Obsolete]
		class Parameter
		{
			public object Key { get; set; }
			public string Name { get; set; }

			public IReadOnlyList<ParameterValue> Values { get; set; }
		}

		[Obsolete]
		class ParameterValue
		{
			public object Value { get; set; }
			public string Name { get; set; }
		}

		[Obsolete]
		static IEnumerable<string> StreamToEnumerable(StreamReader strm)
		{
			while (!strm.EndOfStream)
			{
				yield return strm.ReadLine();
			}
		}

	}

	public interface IModelParameter
	{
		string ParameterName { get; }

		Type ValueType { get; }

		bool IsString { get; }

		bool IsNumber { get; }

		IReadOnlyList<IModelValue> Values { get; }
	}

	public class ModelParameter<T> : IModelParameter
	{
		public string ParameterName { get; }

		public Type ValueType => typeof(T);

		public IReadOnlyList<ModelValue<T>> Values { get; }
		IReadOnlyList<IModelValue> IModelParameter.Values => Values;

		public bool IsString => Type.GetTypeCode(typeof(T)) == TypeCode.String;

		public bool IsNumber => new bool[] {
			typeof(T) == typeof(int)
		}.Any(x => x);

		public ModelParameter(string name, IEnumerable<ModelValue<T>> values)
		{
			ParameterName = name;
			Values = values.ToArray();
		}
	}

	public interface IModelValue : ICloneable
	{
		object Value { get; }
		bool Negative { get; }
		string ToValueString();
		new IModelValue Clone();
	}

	public class ModelValue
	{
		public static ModelValue<T> Create<T>(T value, bool negative = false)
		{
			return new ModelValue<T>(value, negative);
		}

		public static ModelValue<T> Create<T>(T value) => new ModelValue<T>(value, false);
	}

	public class ModelValue<T> : IModelValue, ICloneable
	{
		public T Value { get; }
		public bool Negative { get; }
		object IModelValue.Value => Value;

		public string ToValueString()
		{
			string result;
			if (Value is float f32) { result = f32.ToString("R"); }
			else if (Value is double f64) { result = f64.ToString("R"); }
			else { result = Value.ToString(); }

			if (Negative) result = "~" + result;

			return result;
		}

		public ModelValue(T value, bool negative = false)
		{
			Value = value;
			Negative = negative;
		}

		public static implicit operator ModelValue<T>(T value)
		{
			return new ModelValue<T>(value);
		}

		public ModelValue<T> Create(T value) => new ModelValue<T>(value);

		public ModelValue<T> Clone() => (ModelValue<T>)MemberwiseClone();
		IModelValue IModelValue.Clone() => Clone();
		object ICloneable.Clone() => Clone();
	}


	public class TempFile : IDisposable
	{
		public string Path { get; private set; }
		public TempFile()
		{
			Path = System.IO.Path.GetTempFileName();
		}
		public void Dispose()
		{
			if (Path is string) { File.Delete(Path); }
			Path = null;
		}
	}

}
