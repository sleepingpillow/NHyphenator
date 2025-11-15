using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using NHyphenator.Loaders;

namespace NHyphenator
{
    public class Hyphenator
    {
        private const char Marker = '.';
        private readonly bool _hyphenateLastWord;
        private readonly bool _sortPatterns;
        private readonly string _hyphenateSymbol;
        private readonly int _minLetterCount;
        private readonly int _minWordLength;
        private Dictionary<string, int[]> _exceptions = new Dictionary<string, int[]>();
        private List<Pattern> _patterns;
        private static readonly Regex CreateMaskRegex = new Regex(@"\w", RegexOptions.Compiled);

        /// <summary>
        /// Implementation of Frank Liang's hyphenation algorithm
        /// </summary>
        /// <param name="language">Language for load hyphenation patterns</param>
        /// <param name="hyphenateSymbol">Symbol used for denote hyphenation</param>
        /// <param name="minWordLength">Minimum word length for hyphenation word</param>
        /// <param name="minLetterCount">Minimum number of characters left on line</param>
        /// <param name="hyphenateLastWord">Hyphenate last word, NOTE: this option works only if input text contains more than one word</param>
        [Obsolete("Please, load language patterns via Loader")]
        public Hyphenator(HyphenatePatternsLanguage language, string hyphenateSymbol = "&shy;", int minWordLength = 5,
            int minLetterCount = 3, bool hyphenateLastWord = false)
        {
            this._hyphenateSymbol = hyphenateSymbol;
            this._minWordLength = minWordLength;
            this._minLetterCount = minLetterCount >= 0 ? minLetterCount : 0;
            this._hyphenateLastWord = hyphenateLastWord;
            LoadPatterns(new ResourceHyphenatePatternsLoader(language));
        }

        /// <summary>
        /// Implementation of Frank Liang's hyphenation algorithm
        /// </summary>
        /// <param name="loader">ILoader for load hyphenation patterns</param>
        /// <param name="hyphenateSymbol">Symbol used for denote hyphenation</param>
        /// <param name="minWordLength">Minimum word length for hyphenation word</param>
        /// <param name="minLetterCount">Minimum number of characters left on line</param>
        /// <param name="hyphenateLastWord">Hyphenate last word, NOTE: this option works only if input text contains more than one word</param>
        /// <param name="sortPatterns">Sort patterns before using, can be needed for some languages like German, Portuguese, etc. </param>
        public Hyphenator(IHyphenatePatternsLoader loader,
            string hyphenateSymbol = "&shy;",
            int minWordLength = 5,
            int minLetterCount = 3,
            bool hyphenateLastWord = false,
            bool sortPatterns = false)
        {
            _hyphenateSymbol = hyphenateSymbol;
            _minWordLength = minWordLength;
            _minLetterCount = minLetterCount >= 0 ? minLetterCount : 0;
            _hyphenateLastWord = hyphenateLastWord;
            _sortPatterns = sortPatterns;
            LoadPatterns(loader);
        }

        private void LoadPatterns(IHyphenatePatternsLoader loader)
        {
            CreatePatterns(loader.LoadPatterns() ?? "", loader.LoadExceptions() ?? "");
        }

        private void CreatePatterns(string patternsString, string exceptionsString)
        {
            if (string.IsNullOrWhiteSpace(patternsString))
                throw new ArgumentException("Patterns must contain at least one pattern");

            var sep = new[] {' ', '\n', '\r'};
            _patterns = patternsString.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                .Select(CreatePattern)
                .ToList();

            if (_sortPatterns)
            {
                _patterns = _patterns.OrderBy(x => x).ToList();
            }

            _exceptions = exceptionsString.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                .ToDictionary(x => x.Replace("-", ""), CreateHyphenateMaskFromExceptionString);
        }


        public string HyphenateText(string text)
        {
            if (_hyphenateLastWord == false)
            {
                string lastWord = FindLastWord(text);
                if (lastWord.Length > 0)
                    text = text.Remove(text.Length - lastWord.Length);

                var result = HyphenateWordsInText(text);

                return result.Append(lastWord).ToString();
            }

            return HyphenateWordsInText(text).ToString();
        }

        private StringBuilder HyphenateWordsInText(string text)
        {
            var currentWord = new StringBuilder();
            var result = new StringBuilder();
            foreach (char c in text)
            {
                if (char.IsLetter(c))
                    currentWord.Append(c);
                else
                {
                    if (currentWord.Length > 0)
                    {
                        result.Append(HyphenateWord(currentWord.ToString()));
                        currentWord.Clear();
                    }

                    result.Append(c);
                }
            }

            if (currentWord.Length > 0)
            {
                result.Append(HyphenateWord(currentWord.ToString()));
            }
            return result;
        }

        private string FindLastWord(string phrase)
        {
            if (phrase.Length == 0)
                return string.Empty;
            
            int lastLetterPos = -1;
            
            // Find the position of the last letter
            for (int i = phrase.Length - 1; i >= 0; i--)
            {
                if (char.IsLetter(phrase[i]))
                {
                    lastLetterPos = i;
                    break;
                }
            }
            
            if (lastLetterPos == -1)
                return string.Empty;
            
            // Find the start of the word containing the last letter
            int wordStart = lastLetterPos;
            bool foundNonLetter = false;
            for (int i = lastLetterPos - 1; i >= 0; i--)
            {
                if (char.IsLetter(phrase[i]))
                {
                    wordStart = i;
                }
                else
                {
                    // Found a non-letter before the word
                    foundNonLetter = true;
                    break;
                }
            }
            
            // If we didn't find a non-letter, this means the entire phrase is just one word
            // In this case, return empty string (so the word gets hyphenated)
            if (!foundNonLetter && wordStart == 0)
                return string.Empty;
            
            // Return from wordStart to end of phrase (includes trailing punctuation)
            return phrase.Substring(wordStart);
        }

        private string HyphenateWord(string originalWord)
        {
            if (IsNotValidForHyphenate(originalWord))
                return originalWord;

            string word = originalWord.ToLowerInvariant();
            int[] hyphenationMask;
            int maskLength;
            bool isFromException;
            
            if (_exceptions.ContainsKey(word))
            {
                hyphenationMask = _exceptions[word];
                maskLength = hyphenationMask.Length;
                isFromException = true;
            }
            else
            {
                int[] levels = GenerateLevelsForWord(word, out int levelsLength);
                hyphenationMask = CreateHyphenateMaskFromLevels(levels, levelsLength, out maskLength);
                CorrectMask(hyphenationMask, maskLength);
                
                // Return levels array to pool after we're done with it
                ArrayPool<int>.Shared.Return(levels);
                isFromException = false;
            }

            string result = HyphenateByMask(originalWord, hyphenationMask, maskLength);
            
            // Return mask array to pool if it was allocated by us (not from exceptions)
            if (!isFromException)
            {
                ArrayPool<int>.Shared.Return(hyphenationMask);
            }
            
            return result;
        }

        private void CorrectMask(int[] hyphenationMask, int maskLength)
        {
            if (maskLength > _minLetterCount)
            {
                Array.Clear(hyphenationMask, 0, _minLetterCount);
                var correctionLength = _minLetterCount > 0 ? _minLetterCount - 1 : 0;
                Array.Clear(hyphenationMask, maskLength - correctionLength, correctionLength);
            }
            else
                Array.Clear(hyphenationMask, 0, maskLength);
        }

        private bool IsNotValidForHyphenate(string originalWord)
        {
            return originalWord.Length <= _minWordLength;
        }

        private int[] GenerateLevelsForWord(string word, out int levelsLength)
        {
            // Use string.Create to avoid StringBuilder allocation
            string wordString = string.Create(word.Length + 2, word, (span, w) =>
            {
                span[0] = Marker;
                w.AsSpan().CopyTo(span.Slice(1));
                span[span.Length - 1] = Marker;
            });
            
            levelsLength = wordString.Length;
            // Rent array from pool instead of allocating
            int[] levels = ArrayPool<int>.Shared.Rent(levelsLength);
            // Clear the rented array to ensure it starts with zeros
            Array.Clear(levels, 0, levelsLength);
            
            // Get direct access to the list's underlying array for faster access
            Span<Pattern> patternsSpan = CollectionsMarshal.AsSpan(_patterns);
            ReadOnlySpan<char> wordSpan = wordString.AsSpan();
            
            for (int i = 0; i < wordString.Length - 2; ++i)
            {
                int patternIndex = 0;
                for (int count = 1; count <= wordString.Length - i; ++count)
                {
                    // Use span slice instead of Substring to avoid allocations
                    ReadOnlySpan<char> patternSlice = wordSpan.Slice(i, count);
                    if (Pattern.Compare(patternSlice, patternsSpan[patternIndex]) < 0)
                        continue;
                    
                    // FindIndex with span comparison - manual loop to avoid lambda capture
                    patternIndex = FindPatternIndex(patternIndex, patternSlice);
                    
                    if (patternIndex == -1)
                        break;
                    if (Pattern.Compare(patternSlice, patternsSpan[patternIndex]) >= 0)
                        for (int levelIndex = 0;
                            levelIndex < patternsSpan[patternIndex].GetLevelsCount() - 1;
                            ++levelIndex)
                        {
                            int level = patternsSpan[patternIndex].GetLevelByIndex(levelIndex);
                            if (level > levels[i + levelIndex])
                                levels[i + levelIndex] = level;
                        }
                }
            }

            return levels;
        }
        
        private int FindPatternIndex(int startIndex, ReadOnlySpan<char> patternSlice)
        {
            for (int i = startIndex; i < _patterns.Count; i++)
            {
                if (Pattern.Compare(_patterns[i], patternSlice) > 0)
                    return i;
            }
            return -1;
        }

        private static int[] CreateHyphenateMaskFromLevels(int[] levels, int levelsLength, out int maskLength)
        {
            maskLength = levelsLength - 2;
            // Rent array from pool instead of allocating
            var hyphenationMask = ArrayPool<int>.Shared.Rent(maskLength);
            // Clear the rented array to ensure correct initialization
            Array.Clear(hyphenationMask, 0, maskLength);
            
            for (int i = 0; i < maskLength; i++)
            {
                if (i != 0 && levels[i + 1] % 2 != 0)
                    hyphenationMask[i] = 1;
                else
                    hyphenationMask[i] = 0;
            }

            return hyphenationMask;
        }

        private string HyphenateByMask(string originalWord, int[] hyphenationMask, int maskLength)
        {
            // Count hyphen positions to calculate exact length needed
            int hyphenCount = 0;
            for (int i = 0; i < maskLength; i++)
            {
                if (hyphenationMask[i] > 0)
                    hyphenCount++;
            }
            
            if (hyphenCount == 0)
                return originalWord; // No hyphens needed, return original
            
            // Use string.Create for zero-copy string building
            int hyphenSymbolLength = _hyphenateSymbol.Length;
            int resultLength = originalWord.Length + (hyphenCount * hyphenSymbolLength);
            
            return string.Create(resultLength, (originalWord, hyphenationMask, _hyphenateSymbol, maskLength), (span, state) =>
            {
                int pos = 0;
                for (int i = 0; i < state.originalWord.Length; i++)
                {
                    if (i < state.maskLength && state.hyphenationMask[i] > 0)
                    {
                        state._hyphenateSymbol.AsSpan().CopyTo(span.Slice(pos));
                        pos += state._hyphenateSymbol.Length;
                    }
                    span[pos++] = state.originalWord[i];
                }
            });
        }

        private int[] CreateHyphenateMaskFromExceptionString(string s)
        {
            int[] array = CreateMaskRegex.Split(s)
                .Select(c => c == "-" ? 1 : 0)
                .ToArray();
            return array;
        }

        private Pattern CreatePattern(string pattern)
        {
            var levels = new List<int>(pattern.Length);
            var resultStr = new StringBuilder(pattern.Length);
            bool waitDigit = true;
            foreach (char c in pattern)
            {
                if (char.IsDigit(c))
                {
                    levels.Add(c - '0'); // Faster than Int32.Parse
                    waitDigit = false;
                }
                else
                {
                    if (waitDigit)
                        levels.Add(0);
                    resultStr.Append(c);
                    waitDigit = true;
                }
            }

            if (waitDigit)
                levels.Add(0);

            return new Pattern(resultStr.ToString(), levels);
        }
    }
}