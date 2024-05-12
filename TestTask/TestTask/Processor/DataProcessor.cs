using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using TestTask.Entity;
using TestTask.ConsoleView;
using Timer = System.Threading.Timer;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;

namespace TestTask.Processor
{
	internal class DataProcessor
	{
		private string _filePath;
		private ConsoleViewer _consoleViewer;

		private long _totalFileSize = 1;
		private long _processedFileSize = 0;

		private decimal revenue = 0m;
		private object locker = new object();

		private ConcurrentDictionary<string, long> brandCounts = new ConcurrentDictionary<string, long>();
		private ConcurrentDictionary<long, long> categoryCounts = new ConcurrentDictionary<long, long>();
		private ConcurrentDictionary<long, string> categoryCodes = new ConcurrentDictionary<long, string>();
		private ConcurrentDictionary<long, long> productCounts = new ConcurrentDictionary<long, long>();

		public DataProcessor(string filePath, ConsoleViewer consoleViewer)
		{
			_filePath = filePath;
			_consoleViewer = consoleViewer;
		}

		private void ReadData(CancellationToken token)
		{
			var processId = _consoleViewer.AddProcess("Обработка файла");

			if (processId == null)
			{
				return;
			}

			var progressTimer = StartProgressTimer(processId.Value);

			using (var reader = new StreamReader(_filePath))
			{
				var config = new CsvConfiguration(CultureInfo.InvariantCulture)
				{
					BadDataFound = null,
					MissingFieldFound = null,
					HasHeaderRecord = true
				};

				_totalFileSize = new FileInfo(_filePath).Length;

				using (var csv = new CsvReader(reader, config))
				{
					List<EventRecord> records = new List<EventRecord>(500);

					while (csv.Read())
					{
						if (token.IsCancellationRequested)
						{
							progressTimer?.Dispose();
							return;
						}

						try
						{
							records.Add(csv.GetRecord<EventRecord>());
						}
						catch (CsvHelperException)
						{
							continue;
						}
						catch (Exception)
						{
							progressTimer?.Dispose();
							return;
						}

						if (records.Count > 400)
						{
							Task dataProcessing = new Task(state => ProcessData(state), new List<EventRecord>(records));
							dataProcessing.Start();

							_processedFileSize = reader.BaseStream.Position;

							//ProcessDataInOneThread(records);

							records.Clear();
						}
					}
				}
			}

			_consoleViewer.StopProcess(processId.Value);
			progressTimer?.Dispose();
		}

		private void ProcessData(object? state)
		{
			if (state is List<EventRecord> events)
			{
				var localRevenue = 0m;
				var localBrandCounts = new Dictionary<string, long>();
				var localCategoryCounts = new Dictionary<long, long>();
				var localCategoryCodes = new Dictionary<long, string>();
				var localProductCounts = new Dictionary<long, long>();

				foreach (var record in events)
				{
					if (record != null)
					{
						if (record.Brand != "")
						{
							if (localBrandCounts.ContainsKey(record.Brand))
							{
								localBrandCounts[record.Brand]++;
							}
							else
							{
								localBrandCounts.Add(record.Brand, 1);
							}
						}

						if (localCategoryCounts.ContainsKey(record.CategoryId))
						{
							localCategoryCounts[record.CategoryId]++;
						}
						else
						{
							localCategoryCounts.Add(record.CategoryId, 1);
							localCategoryCodes.Add(record.CategoryId, record.CategoryCode ?? "unknown");
						}

						if (localProductCounts.ContainsKey(record.ProductId))
						{
							localProductCounts[record.ProductId]++;
						}
						else
						{
							localProductCounts.Add(record.ProductId, 1);
						}

						if (record.EventType == "purchase")
						{
							localRevenue += record.Price;
						}
					}
				}

				events.Clear();

				foreach (var pair in localBrandCounts)
				{
					brandCounts.AddOrUpdate(pair.Key, pair.Value, (key, value) => value + pair.Value);
				}

				foreach (var pair in localCategoryCounts)
				{
					categoryCounts.AddOrUpdate(pair.Key, pair.Value, (key, value) => value + pair.Value);
				}

				foreach (var pair in localCategoryCodes)
				{
					categoryCodes.GetOrAdd(pair.Key, pair.Value);
				}

				foreach (var pair in localProductCounts)
				{
					productCounts.AddOrUpdate(pair.Key, pair.Value, (key, value) => value + pair.Value);
				}

				lock (locker)
				{
					revenue += localRevenue;
				}
			}
		}

		//void ProcessDataInOneThread(List<EventRecord> records)
		//{
		//	foreach (var record in records)
		//	{
		//		if (record != null)
		//		{
		//			if (record.Brand != "")
		//			{
		//				brandCounts.AddOrUpdate(record.Brand, 1, (key, value) => value + 1);
		//			}

		//			categoryCounts.AddOrUpdate(record.CategoryId, 1, (key, value) => value + 1);
		//			categoryCodes.GetOrAdd(record.CategoryId, record.CategoryCode ?? "unknown");

		//			productCounts.AddOrUpdate(record.ProductId, 1, (key, value) => value + 1);

		//			if (record.EventType == "purchase")
		//			{
		//				revenue += record.Price;
		//			}
		//		}
		//	}
		//}

		public EventStatistics? ProcessFile(CancellationToken token)
		{
			if (_filePath == null || !File.Exists(_filePath))
			{
				return null;
			}

			Task fileReading = new Task(() => ReadData(token), token);
			fileReading.Start();

			fileReading.Wait();

			var mostPopularCategoryId = categoryCounts.OrderByDescending(item => item.Value).FirstOrDefault().Key;

			return new EventStatistics()
			{
				TotalRevenue = revenue,
				MostPopularBrand = brandCounts.OrderByDescending(item => item.Value).FirstOrDefault().Key,
				MostPopularCategoryId = mostPopularCategoryId,
				MostPopularCategoryCode = categoryCodes[mostPopularCategoryId],
				MostPopularProductId = productCounts.OrderByDescending(item => item.Value).FirstOrDefault().Key
			};
		}

		private Timer StartProgressTimer(Guid processId)
		{
			return new Timer(callback: (state) =>
				{
					var progress = (int)Math.Floor((double)_processedFileSize / _totalFileSize * 100);

					_consoleViewer.UpdateProcess(processId, progress);
				},
				null,
				100,
				5000
			);
		}
	}
}
