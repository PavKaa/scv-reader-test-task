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

			var processor = new DataProcessor(args[0]);

			var result = processor.ProcessData();

			Thread.Sleep(1000);
			ConsoleViewer.Dispose();
            Console.WriteLine();

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

			return;
		}
	}
}