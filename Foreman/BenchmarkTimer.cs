using System;
using System.Collections.Generic;

namespace Foreman
{
	//http://procbits.com/2010/08/25/benchmarking-c-apps-algorithms
	public static class BenchmarkTimer
	{
		private static Stack<BenchmarkData> _startStack = new Stack<BenchmarkData>();

		public static void Start()
		{
			_startStack.Push(new BenchmarkData());
		}

		public static void Start(string label)
		{
			var bd = new BenchmarkData() { Label = label };
			_startStack.Push(bd);
		}

		public static TimeSpan Stop()
		{
			var stop = DateTime.Now;
			var startBD = _startStack.Pop();
			return stop - startBD.DateTime;
		}

		public static void StopAndOutput()
		{
			var stop = DateTime.Now;
			var startBD = _startStack.Pop();

			var delta = stop - startBD.DateTime;

			var lbl = "{0}: {1} ms";
			Console.WriteLine(String.Format(lbl, startBD.Label, delta.TotalMilliseconds));
		}

		private class BenchmarkData
		{
			public DateTime DateTime { get; set; }
			public string Label { get; set; }

			public BenchmarkData()
			{
				this.DateTime = DateTime.Now;
				this.Label = "";
			}
		}
	}
}
