using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Pict.Net.Tests
{
	[TestClass]
	public class ConstraintTests
	{
		enum FileSystem
		{
			FAT,
			FAT32,
			NTFS
		}

		public TestContext TestContext { get; set; }

		[TestMethod]
		public void TestMethod1()
		{
			var pairwiser = new Pairwiser();
			pairwiser.AddParameter("Type", new[] { "Single", "Span", "Stripe", "Mirror", "RAID-5" });
			pairwiser.AddParameter("Size", new[] { 10, 100, 500, 1000, 5000, 10000, 40000 });
			pairwiser.AddParameter("Format method", new[] { "Quick", "Slow" });
			pairwiser.AddParameter("File system", Enum.GetValues(typeof(FileSystem)).Cast<FileSystem>());
			pairwiser.AddParameter("Cluster size", new[] { 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536 });
			pairwiser.AddParameter("Compression", new[] { true, false });
			pairwiser.AddConstraint("File system", "Size", (FileSystem fileSystem, int size) => fileSystem == FileSystem.FAT ? size < 4096 : true);
			pairwiser.AddConstraint("File system", "Size", (FileSystem fileSystem, int size) => fileSystem == FileSystem.FAT32 ? size < 32000 : true);

			var actual = pairwiser.GetCases("Type", "Size", "Format method", "File system", "Cluster size", "Compression");
			var expected = Helper.ReadCases("cases2.txt");

			foreach (var result in actual)
			{
				TestContext.WriteLine(string.Join("\t", result));
			}

			Helper.AssertAreEquals(expected, actual);


		}
	}
}
