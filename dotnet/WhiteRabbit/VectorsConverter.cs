namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// Converts strings to vectors containing chars count, based on a source string.
    /// E.g. for source string "abc", string "a" is converted to [1, 0, 0], while string "bcb" is converted to [0, 2, 1].
    /// </summary>
    internal sealed class VectorsConverter
    {
        public VectorsConverter(byte[] sourceString)
        {
            var rawNumberOfOccurrences = sourceString.GroupBy(ch => ch).ToDictionary(group => group.Key, group => group.Count());
            this.IntToChar = rawNumberOfOccurrences.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Key).ToArray();

            if (this.IntToChar.Length > Vector<byte>.Count)
            {
                throw new ArgumentException($"String should not contain more than {Vector<byte>.Count} different characters", nameof(sourceString));
            }

            this.CharToInt = Enumerable.Range(0, this.IntToChar.Length).ToDictionary(i => this.IntToChar[i], i => i);
        }

        private Dictionary<byte, int> CharToInt { get; }

        private byte[] IntToChar { get; }

        public Vector<byte>? GetVector(byte[] word)
        {
            if (word.Any(ch => !this.CharToInt.ContainsKey(ch)))
            {
                return null;
            }

            var arr = new byte[Vector<byte>.Count];
            foreach (var ch in word)
            {
                arr[this.CharToInt[ch]]++;
            }

            return new Vector<byte>(arr);
        }
    }
}
