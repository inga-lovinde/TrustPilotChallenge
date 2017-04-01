namespace WhiteRabbit
{
    // Anagram representation optimized for MD5
    internal class Phrase
    {
        public readonly uint[] Buffer = new uint[8];

        public unsafe Phrase(byte[][] words, int numberOfCharacters)
        {
            fixed (uint* bufferPointer = this.Buffer)
            {
                var length = numberOfCharacters + words.Length - 1;

                byte[] currentWord = words[0];
                var j = 0;
                var wordIndex = 0;
                var currentPointer = (byte*)bufferPointer;
                byte* lastPointer = currentPointer + length;
                for (; currentPointer < lastPointer; currentPointer++)
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
                *currentPointer = 128;

                bufferPointer[7] = (uint)(length << 3);
            }
        }

        public unsafe byte[] GetBytes()
        {
            fixed(uint* bufferPointer = this.Buffer)
            {
                var length = bufferPointer[7] >> 3;
                var result = new byte[length];
                for (var i = 0; i < length; i++)
                {
                    result[i] = ((byte*)bufferPointer)[i];
                }

                return result;
            }
        }
    }
}
