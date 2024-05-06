using System.Diagnostics;

namespace TestTask.ConsoleView
{
	internal class ProcessInfo
	{
		public string Name { get; set; }

		public Stopwatch Timer { get; private set; }

        public int Progress { get; set; }

        public ProcessInfo(string name)
		{
			Name = name;
			Timer = new Stopwatch();
		}

		public void Start()
		{
			Timer.Start();
		}

		public void Stop()
		{
			Timer.Stop();
		}

		public TimeSpan GetElapsedTime()
		{
			return Timer.Elapsed;
		}
	}
}
