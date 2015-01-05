using System;
using System.Collections.Generic;

namespace Laconic.Codingame.Scrabble
{
    public class Program
    {
        private static readonly Dictionary<char, int> Scores
            = new Dictionary<char, int>
              {
                  {'e', 1},
                  {'a', 1},
                  {'i', 1},
                  {'o', 1},
                  {'n', 1},
                  {'r', 1},
                  {'t', 1},
                  {'l', 1},
                  {'s', 1},
                  {'u', 1},
                  {'d', 2},
                  {'g', 2},
                  {'b', 3},
                  {'c', 3},
                  {'m', 3},
                  {'p', 3},
                  {'f', 4},
                  {'h', 4},
                  {'v', 4},
                  {'w', 4},
                  {'y', 4},
                  {'k', 5},
                  {'j', 8},
                  {'x', 8},
                  {'q', 10},
                  {'z', 10},
              };

        public static void Main(string[] args)
        {
            var words = ReadWords();
            var letters = Console.ReadLine();
            var bestWord = FindBestWord(words, letters);

            Console.WriteLine(bestWord);
        }

        private static string FindBestWord(IEnumerable<string> words, string letters)
        {
            var bestScore = int.MinValue;
            var bestWord = "";
            foreach (var word in words)
            {
                var score = GetWordScore(letters, word);
                if (bestScore < score)
                {
                    bestScore = score;
                    bestWord = word;
                }
            }

            return bestWord;
        }

        private static int GetWordScore(string letters, string word)
        {
            if (word.Length > 7) return 0;

            var lettersAvailability = 0x7F;
            var score = 0;
            foreach (var c in word)
            {
                var isLetterAvailable = false;
                for (var i = 0; i < letters.Length; i++)
                {
                    if (letters[i] != c || (lettersAvailability & (1 << i)) == 0) continue;

                    isLetterAvailable = true;
                    lettersAvailability &= ~(1 << i);
                    break;
                }

                if (!isLetterAvailable) return 0;

                score += Scores[c];
            }

            return score;
        }

        private static string[] ReadWords()
        {
            var n = int.Parse(Console.ReadLine());
            var words = new string[n];
            for (var i = 0; i < n; i++)
            {
                words[i] = Console.ReadLine();
            }

            return words;
        }
    }
}