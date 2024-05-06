using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Timer = System.Threading.Timer;

namespace TestTask.ConsoleView
{
	internal static class ConsoleViewer
	{
		private static readonly List<ProcessInfo> _processes = new List<ProcessInfo>();
		private static Timer _timer;

		static ConsoleViewer()
		{
			_timer = new Timer(OnProgressTimerElapsed, null, 0, 1000);
		}
		
		public static bool AddProcess(string processName)
		{
			lock(_processes)
			{
				var process = _processes.Find(process => process.Name == processName);

				if(process == null)
				{
					process = new ProcessInfo(processName);
					_processes.Add(process);

					process.Start();

					return true;
				}
			}

			return false;
		}

		public static bool StopProcess(string processName)
		{
			lock (_processes)
			{
				var process = _processes.Find(process => process.Name == processName);

				if (process != null)
				{
					process.Stop();

					return true;
				}
			}

			return false;
		}

		public static bool UpdateProcess(string processName, int progress)
		{
			lock (_processes)
			{
				var process = _processes.Find(process => process.Name == processName);

				if (process != null)
				{
					process.Progress = progress;

					return true;
				}
			}

			return false;
		}

		private static void OnProgressTimerElapsed(object state)
		{
			int consoleWidth = Console.BufferWidth;

			Console.Clear();

            Console.WriteLine("Текущие процессы:");

			lock( _processes )
			{
				foreach(var process in _processes)
				{
					StringBuilder sb = new StringBuilder();
					string processName = "Процесс: " + process.Name + " ";
					string timeStr = string.Format(" {0:f3} сек.", process.Timer.Elapsed.TotalSeconds);

					sb.Append(processName);

					var charsRemains = consoleWidth - processName.Length - timeStr.Length;

					if (process.Timer.IsRunning)
					{
						if (process.Progress != 0 && charsRemains > 0)
						{
							var countOfEqualSign = (int)Math.Floor((charsRemains - 3) / 100.0 * process.Progress);

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
	
		public static void Dispose()
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
					string processName = "Процесс: " + process.Name + " ";
					string timeStr = string.Format(" {0:f3} сек.", process.Timer.Elapsed.TotalSeconds);

					sb.Append(processName);

					var charsRemains = consoleWidth - processName.Length - timeStr.Length;

					if (process.Timer.IsRunning)
					{
						if (charsRemains > 0)
						{
							var message = new string(' ', (charsRemains - 4) / 2) + "прерван";

							sb.Append('|')
							  .Append(message)
							  .Append(' ', (charsRemains - message.Length - 2))
							  .Append('|');
						}

						process.Stop();
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
