using System;

namespace CP.Diagnostics.Sitecore.LogAnalyzer
{
	public class LogEntry
	{
		public string FileName { get; set; }
		public LogLevel LogLevel { get; set; }
		public DateTime DateTime { get; set; }
		public string Text { get; set; }
		public string EventSource { get; set; }
	}
}
