using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Globalization;
using TestTask.ConsoleView;
using TestTask.Entity;
using TestTask.Processor;

namespace TestTask
{
	internal class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1 || args.Length > 1)
			{
				Console.WriteLine("Не корректно указан аргмент. Параметры: <Путь к файлу>");
				return;
			}

			ThreadPool.SetMaxThreads(16, 2);

			CancellationTokenSource cancellTokenSource = new CancellationTokenSource();
			CancellationToken token = cancellTokenSource.Token;

			var cancellationThread = StartCancellationThread(cancellTokenSource);

			var consoleViewer = new ConsoleViewer();
			consoleViewer.StartDisplayProcesses();

			var processor = new DataProcessor(args[0], consoleViewer);
			var result = processor.ProcessFile(token);

			consoleViewer?.Dispose();
            Console.WriteLine();

            if (cancellationThread.ThreadState == ThreadState.Running)
			{
				cancellTokenSource.Cancel();
			}

			if (result != null)
			{
				Console.WriteLine($"Выручка: {result.TotalRevenue}");
				Console.WriteLine($"Наиболее популярный бренд: {result.MostPopularBrand}");
				Console.WriteLine($"Самая популярная категория: {result.MostPopularCategoryId}. Код категории: {result.MostPopularCategoryCode}");
				Console.WriteLine($"Самый популярный товар: {result.MostPopularProductId}");
			}
			else
			{
				Console.WriteLine("Во время обработки файла произошла ошибка");
			}
		}

		private static Thread StartCancellationThread(CancellationTokenSource cancellTokenSource)
		{
			Thread cansellationThread = new Thread(() =>
			{
				while (!cancellTokenSource.Token.IsCancellationRequested)
				{
					if (Console.KeyAvailable)
					{
						var key = Console.ReadKey(intercept: true);

						if (key.Key == ConsoleKey.C)
						{
							cancellTokenSource.Cancel();
                        }
					}

					Thread.Sleep(100);
				}
			});

			cansellationThread.Start();

			return cansellationThread;
		}
	}
}