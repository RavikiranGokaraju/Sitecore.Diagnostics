using System;

namespace CP.Diagnostics.Sitecore.LogAnalyzer
{
	public static class DateTimeExtensions
	{
		public static bool IsTimeValid(int hour, int minute, int second)
		{
			if (IsBetween(hour, DateTime.MinValue.Hour, DateTime.MaxValue.Hour) && IsBetween(minute, DateTime.MinValue.Minute, DateTime.MaxValue.Minute))
				return IsBetween(second, DateTime.MinValue.Second, DateTime.MaxValue.Second);
			return false;
		}

		public static bool IsDateTimeValid(int year, int month, int day, int hour, int minute, int second)
		{
			if (IsBetween(year, DateTime.MinValue.Year, DateTime.MaxValue.Year) && IsBetween(month, DateTime.MinValue.Month, DateTime.MaxValue.Month) && IsBetween(day, DateTime.MinValue.Day, DateTime.MaxValue.Day))
				return IsTimeValid(hour, minute, second);
			return false;
		}

		public static bool IsBetween(int value, int min, int max)
		{
			if (value >= min)
				return value <= max;
			return false;
		}

		public static DateTime GetDateFromFileName(string line)
		{
			for (var index = 0; index < line.Length; ++index)
			{
				if (line[index] == 46 && index + 9 < line.Length && (char.IsDigit(line[index + 1]) && char.IsDigit(line[index + 2])) && (char.IsDigit(line[index + 3]) && char.IsDigit(line[index + 4]) && (char.IsDigit(line[index + 5]) && char.IsDigit(line[index + 6]))) && (char.IsDigit(line[index + 7]) && char.IsDigit(line[index + 8])))
				{
					var year = (line[index + 1] - 48) * 1000 + (line[index + 2] - 48) * 100 + (line[index + 3] - 48) * 10 + line[index + 4] - 48;
					var month = (line[index + 5] - 48) * 10 + line[index + 6] - 48;
					var day = (line[index + 7] - 48) * 10 + line[index + 8] - 48;
					if (line[index + 9] != 46 || !char.IsDigit(line[index + 10]) || (!char.IsDigit(line[index + 11]) || !char.IsDigit(line[index + 12])) || (!char.IsDigit(line[index + 13]) || !char.IsDigit(line[index + 14]) || !char.IsDigit(line[index + 15])))
						return new DateTime(year, month, day, 0, 0, 0);
					var hour = (line[index + 10] - 48) * 10 + (line[index + 11] - 48);
					var minute = (line[index + 12] - 48) * 10 + (line[index + 13] - 48);
					var second = (line[index + 14] - 48) * 10 + (line[index + 15] - 48);
					return new DateTime(year, month, day, hour, minute, second);
				}
			}
			return DateTime.MinValue;
		}

		public static DateTime? GetDateTime(string line, ref int index, DateTime startDateTime)
		{
			if (index + 7 >= line.Length || !char.IsDigit(line[index]) || (!char.IsDigit(line[index + 1]) || !char.IsDigit(line[index + 3])) || (!char.IsDigit(line[index + 4]) || !char.IsDigit(line[index + 6]) || !char.IsDigit(line[index + 7])))
				return new DateTime?();
			var hour = (line[index] - 48) * 10 + line[index + 1] - 48;
			var minute = (line[index + 3] - 48) * 10 + line[index + 4] - 48;
			var second = (line[index + 6] - 48) * 10 + line[index + 7] - 48;
			if (!IsTimeValid(hour, minute, second))
				return new DateTime?();
			index = ParserHelper.GoToNextWord(line, index + 8);
			return hour < startDateTime.Hour ? new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.AddDays(1.0).Day, hour, minute, second) : new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, hour, minute, second);
		}
	}
}
