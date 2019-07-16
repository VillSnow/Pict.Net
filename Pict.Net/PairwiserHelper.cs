using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Pict.Net
{
	public static class PairwiserHelper
	{
		public static IReadOnlyList<KeyValuePair<object, object>[]> Generate(IEnumerable<KeyValuePair<object, object>> model, int order = 2)
		{
			var parameters = ToParameters(model);
			var parameterDict = parameters.ToDictionary(p => p.Name, p => p.Key);
			var parameterValueDict = parameters
				.SelectMany(p => p.Values)
				.ToDictionary(v => v.Name, v => v.Value);

			var output = RunPict(CreateModelText(parameters), $"/O:{order}");

			return ParseOutput(output).Select(
				line => line.Select(pair => new KeyValuePair<object, object>(parameterDict[pair.Key], parameterValueDict[pair.Value])).ToArray()
			).ToArray();
		}

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

		static IEnumerable<string> CreateModelText(IReadOnlyList<Parameter> parameters)
		{
			foreach (var parameter in parameters)
			{
				yield return parameter.Name + ":" + string.Join(",", parameter.Values.Select(x => x.Name));
			}
		}

		static IEnumerable<string> RunPict(IEnumerable<string> model, string options)
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



		class Parameter
		{
			public object Key { get; set; }
			public string Name { get; set; }

			public IReadOnlyList<ParameterValue> Values { get; set; }
		}

		class ParameterValue
		{
			public object Value { get; set; }
			public string Name { get; set; }
		}


		static IEnumerable<string> StreamToEnumerable(StreamReader strm)
		{
			while (!strm.EndOfStream)
			{
				yield return strm.ReadLine();
			}
		}

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
