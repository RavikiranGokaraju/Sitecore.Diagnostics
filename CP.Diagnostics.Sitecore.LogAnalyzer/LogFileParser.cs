using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CP.Diagnostics.Sitecore.LogAnalyzer
{
	public static class LogFileParser
	{
		public static IEnumerable<LogEntry> ParseFile(StreamReader streamReader, string fileName, LogLevel? logLevel, DateTime fromDate)
		{
			var fileStartDate = DateTimeExtensions.GetDateFromFileName(fileName);
			var lines = GetLines(streamReader);
			for (var i = 0; i < lines.Count; i++)
			{
				var line = lines[i];
				if (string.IsNullOrEmpty(line))
					continue;
				line = line.Trim();
				var logEntry = new LogEntry();
				var spaceInTheSameLine = ParserHelper.GetNextSpaceInTheSameLine(line, 0);
				if (spaceInTheSameLine == -1)
					continue;
				int dateTimeStartIndex;
				var logDateTime = FindLogDateTime(line, fileStartDate, ref spaceInTheSameLine, out dateTimeStartIndex);
				if (!logDateTime.HasValue)
					continue;
				if (logDateTime.Value < fromDate)
					continue;
				logEntry.DateTime = logDateTime.Value;
				logEntry.EventSource = line.Substring(0, ParserHelper.GoToPrevWord(line, dateTimeStartIndex));
				switch (GetLogLevel(line, ref spaceInTheSameLine))
				{
					case "INFO":
						logEntry.LogLevel = LogLevel.Info;
						break;
					case "ERROR":
						logEntry.LogLevel = LogLevel.Error;
						break;
					case "WARN":
						logEntry.LogLevel = LogLevel.Warn;
						break;
					case "DEBUG":
						logEntry.LogLevel = LogLevel.Debug;
						break;
					case "FATAL":
						logEntry.LogLevel = LogLevel.Fatal;
						break;
					case "SYSTEM":
						logEntry.LogLevel = LogLevel.System;
						break;
					default:
						continue;
				}
				if (logLevel.HasValue && logEntry.LogLevel != logLevel.Value)
					continue;
				var text = new StringBuilder();
				text.AppendLine(GetText(line, spaceInTheSameLine));
				while (i + 1 < lines.Count)
				{
					if (!string.IsNullOrEmpty(lines[i + 1]))
					{
						int dtStartIndex;
						var spaceIndex = ParserHelper.GetNextSpaceInTheSameLine(lines[i + 1], 0);
						var dateTime = FindLogDateTime(lines[i + 1], fileStartDate, ref spaceIndex, out dtStartIndex);
						if (!dateTime.HasValue)
						{
							text.AppendLine(GetText(lines[i + 1], spaceIndex));
							i = i + 1;
						}
						else
							break;
					}
					else
						break;
				}
				logEntry.Text = text.ToString();
				logEntry.FileName = fileName;
				yield return logEntry;
			}
		}

		private static List<string> GetLines(StreamReader streamReader)
		{
			var result = new List<string>();
			string line;
			while ((line = streamReader.ReadLine()) != null)
				result.Add(line);

			return result;
		}

		public static DateTime? FindLogDateTime(string line, DateTime startDateTime, ref int index, out int dateTimeStartIndex)
		{
			var index1 = index;
			DateTime? dateTime;
			do
			{
				index1 = ParserHelper.GoToNextWordOnTheSameLine(line, index1);
				dateTimeStartIndex = index1;
				dateTime = DateTimeExtensions.GetDateTime(line, ref index1, startDateTime);
			}
			while (!dateTime.HasValue && (index1 = ParserHelper.GetNextSpaceInTheSameLine(line, index1)) != -1);
			if (dateTime.HasValue)
				index = index1;
			else
				dateTimeStartIndex = -1;
			return dateTime;
		}

		public static string GetLogLevel(string line, ref int index)
		{
			var index1 = line.IndexOf(' ', index);
			string str;
			if (index1 != -1)
			{
				str = line.Substring(index, index1 - index);
				index = ParserHelper.GoToNextWord(line, index1);
			}
			else
			{
				str = line.Substring(index);
				index = line.Length;
			}
			return str;
		}

		public static string GetText(string line, int index)
		{
			return index >= line.Length - 1 ? string.Empty : line.Substring(index);
		}
	}
}
