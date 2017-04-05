using Rix.AzureSearch;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace Rix.CLI
{
	class Program
	{
		static void Main(string[] args)
		{
			var indexerHelper = new IndexerHelper();

			var task1 = Task.Run(async () => await indexerHelper.EnsureIndexerCreatedAsync());
			task1.Wait();

			Console.WriteLine("Indexer ready!");

			var task4 = Task.Run(async () => await indexerHelper.RunIndexerAsync());
			Console.WriteLine("Indexer updated!");

			//var task2 = Task.Run(async () => await indexerHelper.GetDocumentsCountAsync());
			//task2.Wait();

			//Console.WriteLine("Documents retreaved!");

			//var i = 0;
			//foreach (var val in task2.Result)
			//{
			//	Console.WriteLine("{0}. {1}", i++, val);

			//	var task3 = Task.Run(async () => { await indexerHelper.DeleteDocumentByIdAsync(val.ToString()); });
			//}

			//Console.WriteLine("Documents deleted!");

			//var task5 = Task.Run(async () => await indexerHelper.GetDocumentsCountAsync());
			//task5.Wait();

			//i = 0;
			//foreach (var val in task5.Result)
			//{
			//	Console.WriteLine("{0}. {1}", i++, val);
			//}

			//Console.WriteLine("Press 'enter' when new files are uploaded");
			//Console.ReadLine();

			//var task6 = Task.Run(async () => await indexerHelper.RunIndexerAsync());
			//task6.Wait();

			//Console.WriteLine("Indexer updated!");

			var task7 = Task.Run(async () => await indexerHelper.GetDocumentsAsync());
			task7.Wait();

			Console.WriteLine("Documents retreaved!");

			foreach (var val in task7.Result)
			{
				Console.WriteLine(val.Id);
				Console.WriteLine(val.Content);
				Console.WriteLine();
			}

			var task8 = Task.Run(async() => { await indexerHelper.DeleteDocumentsByIdsAsync(task7.Result.Select(x => x.Id).ToList()); });
			task8.Wait();

			Console.WriteLine("Documents deleted!");

			Console.WriteLine("Press 'enter' to quit");
			Console.ReadLine();
		}
	}
}
