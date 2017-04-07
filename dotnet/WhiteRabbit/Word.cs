namespace WhiteRabbit
{
    class Word
    {
        public byte[] Original;

        public long[] Buffers { get; }

        public int LengthX4 { get; }

        public unsafe Word(byte[] word)
        {
            var tmpWord = new byte[word.Length + 1];
            tmpWord[word.Length] = (byte)' ';
            for (var i = 0; i < word.Length; i++)
            {
                tmpWord[i] = word[i];
            }

            this.Original = tmpWord;

            var buffers = new long[128];
            fixed (long* buffersPointer = buffers)
            {
                for (var i = 0; i < 32; i++)
                {
                    var bytePointer = (byte*)(buffersPointer + 4 * i);
                    var endPointer = bytePointer + 32;
                    var currentPointer = bytePointer + i;
                    for (var j = 0; j < tmpWord.Length && currentPointer < endPointer; j++, currentPointer++)
                    {
                        *currentPointer = tmpWord[j];
                    }
                }
            }

            this.Buffers = buffers;
            this.LengthX4 = tmpWord.Length * 4;
        }
    }
}
