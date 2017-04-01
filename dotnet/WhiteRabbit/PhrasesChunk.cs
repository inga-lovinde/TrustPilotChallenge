namespace WhiteRabbit
{
    using System.Diagnostics;

    internal unsafe struct PhrasesChunk
    {
        public fixed byte Buffers[4 * 32];

        public PhrasesChunk(byte[][] words0, byte[][] words1, byte[][] words2, byte[][] words3, int numberOfCharacters)
        {
            Debug.Assert(numberOfCharacters <= 28);

            fixed (byte* bufferPointer = this.Buffers)
            {
                WriteWordsToBuffer(bufferPointer + 0 * 32, words0, numberOfCharacters);
                WriteWordsToBuffer(bufferPointer + 1 * 32, words1, numberOfCharacters);
                WriteWordsToBuffer(bufferPointer + 2 * 32, words2, numberOfCharacters);
                WriteWordsToBuffer(bufferPointer + 3 * 32, words3, numberOfCharacters);
            }
        }

        public byte[] GetBytes(int wordIndex)
        {
            Debug.Assert(wordIndex >= 0);
            Debug.Assert(wordIndex < 4);

            fixed (byte* bufferPointer = this.Buffers)
            {
                return ReadBytesFromBuffer(bufferPointer + wordIndex * 32);
            }
        }

        private static void WriteWordsToBuffer(byte* bufferPointer, byte[][] words, int numberOfCharacters)
        {
            var length = numberOfCharacters + words.Length - 1;

            byte* end = bufferPointer + length;
            byte[] currentWord = words[0];
            var j = 0;
            var wordIndex = 0;
            for (var currentPointer = bufferPointer; currentPointer < end; currentPointer++)
            {
                if (j >= currentWord.Length)
                {
                    j = 0;
                    wordIndex++;
                    currentWord = words[wordIndex];
                }

                *currentPointer = currentWord[j];
                j++;
            }

            bufferPointer[31] = (byte)length;
        }

        private static byte[] ReadBytesFromBuffer(byte* bufferPointer)
        {
            var length = bufferPointer[31];
            var result = new byte[length];
            for (var i = 0; i < length; i++)
            {
                result[i] = bufferPointer[i];
            }

            return result;
        }
    }
}
