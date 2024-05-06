using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using TestTask.Entity;
using System.Timers;
using TestTask.ConsoleView;
using Timer = System.Threading.Timer;

namespace TestTask.Processor
{
	internal class DataProcessor
	{
		private string _filePath;
		private int _totalLines = 1;
		private int _processedLines = 0;
		private bool keepRunning = true;
		//private List<Exception> exceptions = new List<Exception>();
		private Timer _progressTimer;
		
		public DataProcessor(string filePath)
		{
			_filePath = filePath;
		}

		private IEnumerable<EventRecord> ReadData()
		{
			if(keepRunning)
			{
				using (var reader = new StreamReader(_filePath))
				{
					var config = new CsvConfiguration(CultureInfo.InvariantCulture)
					{
						BadDataFound = null,
						MissingFieldFound = null,
						HasHeaderRecord = true
					};

					//todo: добавить поиск по id и чему-то подобному(ссылка)
					ConsoleViewer.AddProcess("Подсчет количества строк в файле");

					_totalLines = File.ReadLines(_filePath).Count();

					ConsoleViewer.StopProcess("Подсчет количества строк в файле");

					using (var csv = new CsvReader(reader, config))
					{
						while (csv.Read() && keepRunning)
						{
							EventRecord record;

							try
							{
								record = csv.GetRecord<EventRecord>();
							}
							catch (CsvHelperException)
							{
								//exceptions.Add(ex);
								continue;
							}
							catch (FileNotFoundException)
							{
								break;
							}
							catch (Exception)
							{
								break;
							}

							_processedLines++;

							yield return record;
						}
					}
				}
			}

			yield break;
		}

		public EventStatistics? ProcessData()
		{
			IEnumerable<EventRecord> eventRecords;

			eventRecords = ReadData();

			var revenue = 0m;
			var locker = new object();

			var brandCounts = new Dictionary<string, long>();
			var categoryCounts = new Dictionary<long, long>();
			var categoryCodes = new Dictionary<long, string>();
			var productCounts = new Dictionary<long, long>();

			ConsoleViewer.AddProcess("Обработка данных файла");
			_progressTimer = new Timer(callback: (state) =>
				{
					var progress = (int)Math.Floor((double)_processedLines / _totalLines * 100);

					ConsoleViewer.UpdateProcess("Обработка данных файла", progress);
				},
				null,
				100,
				5000
			);

			new Thread(() =>
			{
				while (keepRunning)
				{
					if (Console.KeyAvailable)
					{
						var key = Console.ReadKey(intercept: true);
						if (key.Key == ConsoleKey.C)
						{
                            keepRunning = false;
                        }
					}

					Thread.Sleep(100);
				}
			}).Start();

			try
			{
				Parallel.ForEach(eventRecords, record =>
				{
					if (record == null)
					{
						return;
					}

					if (record.Brand != null)
					{
						lock (brandCounts)
						{
							if (brandCounts.ContainsKey(record.Brand))
							{
								brandCounts[record.Brand]++;
							}
							else
							{
								brandCounts.Add(record.Brand, 1);
							}
						}
					}

					lock (categoryCounts)
					{
						if (categoryCounts.ContainsKey(record.CategoryId))
						{
							categoryCounts[record.CategoryId]++;
						}
						else
						{
							categoryCounts.Add(record.CategoryId, 1);

							lock (categoryCodes)
							{
								categoryCodes.Add(record.CategoryId, record.CategoryCode ?? "indefined");
							}
						}
					}

					lock (productCounts)
					{
						if (productCounts.ContainsKey(record.ProductId))
						{
							productCounts[record.ProductId]++;
						}
						else
						{
							productCounts.Add(record.ProductId, 1);
						}
					}

					if (record.EventType == "purchase")
					{
						lock (locker)
						{
							revenue += record.Price;
						}
					}
				});
			}
			catch (Exception ex)
			{
				return null;
			}

			ConsoleViewer.StopProcess("Обработка данных файла");
			_progressTimer?.Dispose();

			var mostPopularCategoryId = categoryCounts.OrderBy(item => item.Value).FirstOrDefault().Key;

			return new EventStatistics()
			{
				TotalRevenue = revenue,
				MostPopularBrand = brandCounts.OrderBy(item => item.Value).FirstOrDefault().Key,
				MostPopularCategoryId = mostPopularCategoryId,
				MostPopularCategoryCode = categoryCodes[mostPopularCategoryId],
				MostPopularProductId = productCounts.OrderBy(item => item.Value).FirstOrDefault().Key
			};
		}
	}
}
