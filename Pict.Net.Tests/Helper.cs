using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pict.Net.Tests
{
	public class Helper
	{
		public static IReadOnlyList<string[]> ReadCases(string path)
		{
			IEnumerable<string[]> inner()
			{
				foreach (var line in File.ReadLines(path).Skip(1))
				{
					yield return line.Split('\t');
				}
			}
			return inner().ToArray();
		}

		public static void AssertAreEquals(IReadOnlyList<string[]> expected, IReadOnlyList<object[]> actual)
		{
			Assert.AreEqual(expected.Count, actual.Count);
			foreach (var (expected1, actual1) in expected.Zip(actual, (a, b) => (a, b)))
			{
				Assert.AreEqual(expected1.Length, actual1.Length);
				foreach (var (expected2, actual2) in expected1.Zip(actual1, (a, b) => (a, b)))
				{
					Assert.AreEqual(expected2, actual2.ToString());
				}
			}

		}

	}




}
