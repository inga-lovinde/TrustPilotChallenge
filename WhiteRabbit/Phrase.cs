namespace WhiteRabbit
{
    internal unsafe struct Phrase
    {
        private const byte SPACE = 32;

        public fixed byte Buffer[32];

        public Phrase(byte[][] words, int numberOfCharacters)
        {
            fixed (byte* bufferPointer = this.Buffer)
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
                        *currentPointer = SPACE;
                        j = 0;
                        wordIndex++;
                        currentWord = words[wordIndex];
                    }
                    else
                    {
                        *currentPointer = currentWord[j];
                        j++;
                    }
                }

                bufferPointer[31] = (byte)length;
            }
        }

        public byte[] GetBytes()
        {
            fixed(byte* bufferPointer = this.Buffer)
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
}
