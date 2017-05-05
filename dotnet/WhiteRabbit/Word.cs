namespace WhiteRabbit
{
    internal unsafe struct Word
    {
        public fixed long Buffers[128];

        public unsafe Word(byte[] word)
        {
            var tmpWord = new byte[word.Length + 1];
            tmpWord[word.Length] = (byte)' ';
            for (var i = 0; i < word.Length; i++)
            {
                tmpWord[i] = word[i];
            }

            fixed (long* buffersPointer = this.Buffers)
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

                buffersPointer[127] = tmpWord.Length * 4;
            }
        }

        public unsafe byte[] Original
        {
            get
            {
                fixed (long* buffersPointer = this.Buffers)
                {
                    var length = buffersPointer[127] / 4;
                    var result = new byte[length];
                    for (var i = 0; i < length; i++)
                    {
                        result[i] = ((byte*)buffersPointer)[i];
                    }

                    return result;
                }
            }
        }

        private static Word Empty { get; } = new Word();
    }
}
