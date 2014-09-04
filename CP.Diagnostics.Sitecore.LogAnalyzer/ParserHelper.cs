namespace CP.Diagnostics.Sitecore.LogAnalyzer
{
	public static class ParserHelper
	{
		public static int GoToNextWord(string text, int index)
		{
			while (index < text.Length && (text[index] == 32 || text[index] == 10 || (text[index] == 13 || text[index] == 9)))
				++index;
			return index;
		}

		public static int GoToPrevWord(string text, int index)
		{
			while (index - 1 > 0 && index - 1 < text.Length && (text[index - 1] == 32 || text[index - 1] == 10 || (text[index - 1] == 13 || text[index - 1] == 9)))
				--index;
			return index;
		}

		public static int GoToNextWordOnTheSameLine(string text, int index)
		{
			while (index < text.Length && (text[index] == 32 || text[index] == 9))
				++index;
			return index;
		}

		public static int GetNextSpaceInTheSameLine(string text, int index)
		{
			var index1 = text.IndexOfAny(new[] { ' ', '\t', '\n', '\r' }, index);
			if (index1 != -1 && text[index1] != 32 && text[index1] != 9)
				index1 = -1;
			return index1;
		}
	}
}
