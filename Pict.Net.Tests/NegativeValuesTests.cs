using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Pict.Net.Tests
{
	[TestClass]
	public class NegativeValuesTests
	{
		public TestContext TestContext { get; set; }

		[TestMethod]
		public void Case01()
		{
			var pairwiser = new Pairwiser();
			pairwiser.AddParameter("A", new[] { ModelValue.Create(-1, true), 0, 1, 2 });
			pairwiser.AddParameter("B", new[] { ModelValue.Create(-1, true), 0, 1, 2 });

			var actual = pairwiser.GetCases("A", "B");
			var expected = Helper.ReadCases("negative_values_cases.txt");

			TestContext.WriteLine("expected:");
			foreach (var result in expected)
			{
				TestContext.WriteLine(string.Join("\t", result));
			}

			TestContext.WriteLine("actual:");
			foreach (var result in actual)
			{
				TestContext.WriteLine(string.Join("\t", result));
			}

			Helper.AssertAreEquals(expected, actual);
		}
	}
}
