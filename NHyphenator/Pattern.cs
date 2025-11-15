using System;
using System.Collections.Generic;
using System.Linq;

namespace NHyphenator
{
	internal sealed class Pattern : IComparer<Pattern>, IComparable<Pattern>
	{
		private readonly string str;
		private readonly int[] levels;

		public int GetLevelByIndex(int index)
		{
			return levels[index];
		}

		public int GetLevelsCount()
		{
			return levels.Length;
		}

		public Pattern(string str, IEnumerable<int> levels)
		{
			this.str = str;
			this.levels = levels.ToArray();
		}


		public Pattern(string str)
		{
			this.str = str;
			levels = Array.Empty<int>();
		}

		public static int Compare(Pattern x, Pattern y)
		{
			bool first = x.str.Length < y.str.Length;
			int minSize = first ? x.str.Length : y.str.Length;
			for (var i = 0; i < minSize; ++i)
			{
				if (x.str[i] < y.str[i])
					return -1;
				if (x.str[i] > y.str[i])
					return 1;
			}
			return first ? -1 : 1;
		}
		
		// Optimized comparison that avoids creating temporary Pattern objects
		public static int Compare(ReadOnlySpan<char> span, Pattern pattern)
		{
			bool spanIsSmaller = span.Length < pattern.str.Length;
			int minSize = spanIsSmaller ? span.Length : pattern.str.Length;
			for (var i = 0; i < minSize; ++i)
			{
				if (span[i] < pattern.str[i])
					return -1;
				if (span[i] > pattern.str[i])
					return 1;
			}
			return spanIsSmaller ? -1 : 1;
		}
		
		// Optimized comparison that avoids creating temporary Pattern objects (reversed order)
		public static int Compare(Pattern pattern, ReadOnlySpan<char> span)
		{
			bool patternIsSmaller = pattern.str.Length < span.Length;
			int minSize = patternIsSmaller ? pattern.str.Length : span.Length;
			for (var i = 0; i < minSize; ++i)
			{
				if (pattern.str[i] < span[i])
					return -1;
				if (pattern.str[i] > span[i])
					return 1;
			}
			return patternIsSmaller ? -1 : 1;
		}

		int IComparer<Pattern>.Compare(Pattern x, Pattern y)
		{
			return Compare(x, y);
		}

		public int CompareTo(Pattern other)
		{
			return Compare(this, other);
		}
	}
}