using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Timer = System.Threading.Timer;

namespace TestTask.ConsoleView
{
	internal class ConsoleViewer : IDisposable
	{
		private static readonly Dictionary<Guid, ProcessInfo> _processes = new Dictionary<Guid, ProcessInfo>();
		private static Timer _timer;

		public ConsoleViewer() {}

		public void StartDisplayProcesses()
		{
			_timer = new Timer(OnProgressTimerElapsed, null, 0, 1000);
		}
		
		public Guid? AddProcess(string processName)
		{
			lock(_processes)
			{
				var process = _processes.Where(pair => pair.Value.Name == processName).Select(pair => pair.Value).FirstOrDefault();

				if(process == null)
				{

					var id = Guid.NewGuid();

					process = new ProcessInfo(processName);
					_processes.Add(id, process);

					process.Start();

					return id;
				}
			}

			return null;
		}

		public bool StopProcess(Guid id)
		{
			lock (_processes)
			{
				if (_processes.TryGetValue(id, out var process))
				{
					process.Stop();
					return true;
				}
			}

			return false;
		}

		public bool UpdateProcess(Guid id, int progress)
		{
			lock (_processes)
			{
				if (_processes.TryGetValue(id, out var process))
				{
					process.Progress = progress;
					return true;
				}
			}

			return false;
		}

		private void OnProgressTimerElapsed(object state)
		{
			int consoleWidth = Console.BufferWidth;

			Console.Clear();

            Console.WriteLine("Текущие процессы:");

			lock( _processes )
			{
				foreach(var process in _processes)
				{
					StringBuilder sb = new StringBuilder();
					string processName = "Процесс: " + process.Value.Name + " ";
					string timeStr = string.Format(" {0:f3} сек.", process.Value.Timer.Elapsed.TotalSeconds);

					sb.Append(processName);

					var charsRemains = consoleWidth - processName.Length - timeStr.Length;

					if (process.Value.Timer.IsRunning)
					{
						if (process.Value.Progress != 0 && charsRemains > 0)
						{
							var countOfEqualSign = (int)Math.Floor((charsRemains - 3) / 100.0 * process.Value.Progress);

							sb.Append('|')
							  .Append('=', countOfEqualSign)
							  .Append('>')
							  .Append(' ', charsRemains - countOfEqualSign - 3)
							  .Append('|');
						}
						else
						{
							sb.Append("|=>")
							  .Append(' ', charsRemains - 4)
							  .Append('|');
						}

						Console.ForegroundColor = ConsoleColor.Green;
                    }
					else
					{
						sb.Append('|')
						  .Append('=', charsRemains - 3)
						  .Append(">|");

						Console.ForegroundColor = ConsoleColor.White;
					}

					sb.Append(timeStr);

                    Console.WriteLine(sb.ToString());
                }

				Console.ForegroundColor = ConsoleColor.White;
			}
		}
	
		public void Dispose()
		{
			_timer.Dispose();

			int consoleWidth = Console.BufferWidth;

			Console.Clear();

			Console.WriteLine("Текущие процессы:");

			lock (_processes)
			{
				foreach (var process in _processes)
				{
					StringBuilder sb = new StringBuilder();
					string processName = "Процесс: " + process.Value.Name + " ";
					string timeStr = string.Format(" {0:f3} сек.", process.Value.Timer.Elapsed.TotalSeconds);

					sb.Append(processName);

					var charsRemains = consoleWidth - processName.Length - timeStr.Length;

					if (process.Value.Timer.IsRunning)
					{
						if (charsRemains > 0)
						{
							var message = new string(' ', (charsRemains - 4) / 2) + "прерван";

							sb.Append('|')
							  .Append(message)
							  .Append(' ', (charsRemains - message.Length - 2))
							  .Append('|');
						}

						process.Value.Stop();
						Console.ForegroundColor = ConsoleColor.Red;
					}
					else
					{
						sb.Append('|')
						  .Append('=', charsRemains - 3)
						  .Append(">|");

						Console.ForegroundColor = ConsoleColor.White;
					}

					sb.Append(timeStr);

					Console.WriteLine(sb.ToString());
				}

				Console.ForegroundColor = ConsoleColor.White;
			}
		}
	}
}
