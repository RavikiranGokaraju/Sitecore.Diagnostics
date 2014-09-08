using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CP.Diagnostics.Sitecore.LogAnalyzer.TaskHelpers;

namespace CP.Diagnostics.Sitecore.LogAnalyzer
{
	public class LogFilesAnalyzer
	{
		public string LogDirectory { get; set; }
		public string LogFileRegex { get; set; }
		public int LookBackHours { get; set; }
		public LogLevel LogLevel { get; set; }

		public LogFilesAnalyzer()
		{
			LogFileRegex = "*log.*.txt";
			LookBackHours = 1;
			LogLevel = LogLevel.Error;
		}

		public DateTime FromDate
		{
			get { return DateTime.Now.AddHours(LookBackHours * -1); }
		}

		public IEnumerable<LogEntry> Analyze()
		{
			var result = new List<LogEntry>();
			var dirInfo = new DirectoryInfo(LogDirectory);
			var files = GetFiles(dirInfo).ToList();

			// This can be replaced with Parallel.ForaEach for .net 4.5
			ForEachExtensions.ForEach(files, fileInfo => result.AddRange(LogFileParser.ParseFile(GetStream(fileInfo), fileInfo.Name, LogLevel, FromDate)));

			return result.OrderByDescending(l => l.DateTime);
		}

		private IEnumerable<FileInfo> GetFiles(DirectoryInfo dirInfo)
		{
			var files = dirInfo.GetFiles(LogFileRegex);
			var validFiles = files.Where(f => DateTimeExtensions.GetDateFromFileName(f.Name).Date >= FromDate.Date).ToList();
			return validFiles.Any() ? validFiles : files.OrderByDescending(f => f.LastWriteTimeUtc).Take(5);
		}

		private StreamReader GetStream(FileInfo fileInfo)
		{
			var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			return new StreamReader(fileStream);
		}
	}
}
