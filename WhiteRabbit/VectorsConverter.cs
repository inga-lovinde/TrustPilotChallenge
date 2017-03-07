namespace WhiteRabbit
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// Converts strings to vectors containing chars count, based on a source string.
    /// E.g. for source string "abc", string "a" is converted to [1, 0, 0], while string "bcb" is converted to [0, 2, 1].
    /// </summary>
    internal class VectorsConverter
    {
        public VectorsConverter(string sourceString)
        {
            var rawNumberOfOccurrences = sourceString.GroupBy(ch => ch).ToDictionary(group => group.Key, group => group.Count());
            this.IntToChar = rawNumberOfOccurrences.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Key).ToArray();
            this.CharToInt = Enumerable.Range(0, this.IntToChar.Length).ToDictionary(i => this.IntToChar[i], i => i);
        }

        private Dictionary<char, int> CharToInt { get; }

        private char[] IntToChar { get; }

        public Vector<byte>? GetVector(string word)
        {
            if (word.Any(ch => !this.CharToInt.ContainsKey(ch)))
            {
                return null;
            }

            var arr = new byte[16];
            foreach (var ch in word)
            {
                arr[this.CharToInt[ch]]++;
            }

            return new Vector<byte>(arr);
        }

        public string GetString(Vector<byte> vector)
        {
            return new string(Enumerable.Range(0, this.IntToChar.Length).SelectMany(i => Enumerable.Repeat(this.IntToChar[i], (int)vector[i])).ToArray());
        }
    }
}
