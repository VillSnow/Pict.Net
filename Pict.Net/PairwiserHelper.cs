using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Pict.Net
{
	internal static class PairwiserHelper
	{

		public static IReadOnlyList<KeyValuePair<IModelParameter, IModelValue>[]> Generate(
			IReadOnlyList<IModelParameter> parameters,
			IReadOnlyCollection<ModelValueIntersection> exclusions
		)
		{
			if (ExistsDuplicatedValueReference(parameters.SelectMany(p => p.Values).ToArray()))
			{
				throw new ArgumentException("!!! Detected Duplicated Values !!!");
			}

			var surrogateMgr = new SurrogateManager(parameters);

			var modelText = CreateModelText(parameters, surrogateMgr);
			var constraintText = CreateConstraintText(exclusions, surrogateMgr);
			var pictOutput = RunPict(modelText.Concat(constraintText), "");
			var parsedOutput = ParseOutput(pictOutput);

			return parsedOutput.Select(
				@case => @case.Select(
					pair => new KeyValuePair<IModelParameter, IModelValue>(
						surrogateMgr.GetModelParameter(pair.Key), 
						surrogateMgr.GetModelValue(pair.Value)
					)
				).ToArray()
			).ToArray();
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

		static IEnumerable<string> CreateModelText(IEnumerable<IModelParameter> parameters, SurrogateManager surrogateManager)
		{
			foreach (var parameter in parameters)
			{
				yield return surrogateManager.GetSurrogate(parameter) + ":" + string.Join(",", parameter.Values.Select(surrogateManager.GetSurrogate));
			}
		}

		static IEnumerable<string> CreateConstraintText(IReadOnlyCollection<ModelValueIntersection> exclusions, SurrogateManager surrogateManager)
		{
			foreach (var exclusion in exclusions)
			{
				yield return string.Join(" OR ",
					from value in exclusion.List
					let parameter = surrogateManager.GetParentModelParameter(value)
					let parameterSurrogate = surrogateManager.GetSurrogate(parameter)
					let valueSurrogate = surrogateManager.GetSurrogate(value)
					select $"[{parameterSurrogate}] <> \"{valueSurrogate}\""
				) + ";";
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
	}

	public interface IModelParameter
	{
		string ParameterName { get; }

		Type ValueType { get; }

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
