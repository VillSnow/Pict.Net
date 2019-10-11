using System;
using System.Linq;

namespace Pict.Net.Run
{
	class Program
	{
		enum FileSystem
		{
			FAT,
			FAT32,
			NTFS
		}

		static void Main(string[] args)
		{
			var pairwiser = new Pairwiser();
			pairwiser.AddParameter("Type", new[] { "Single", "Span", "Stripe", "Mirror", "RAID-5" });
			pairwiser.AddParameter("Size", new[] { 10, 100, 500, 1000, 5000, 10000, 40000 });
			pairwiser.AddParameter("Format method", new[] { "Quick", "Slow" });
			pairwiser.AddParameter("File system", Enum.GetValues(typeof(FileSystem)).Cast<FileSystem>());
			pairwiser.AddParameter("Cluster size", new[] { 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536 });
			pairwiser.AddParameter("Compression", new[] { true, false });

			var results = pairwiser.GetCases(
				"Type",
				"Size",
				"Format method",
				"File system",
				"Cluster size",
				"Compression"
			);

			foreach (var result in results)
			{
				Console.WriteLine(string.Join("\t", result));
			}


			Console.ReadKey(true);
		}
	}
}
