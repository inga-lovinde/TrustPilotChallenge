namespace WhiteRabbit
{
    using System.Runtime.CompilerServices;
    using WhiteRabbitUnmanagedBridge;

    internal static class MD5Digest
    {
        // It only returns first component of MD5 hash
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint[] Compute(PhraseSet input)
        {
            var result = new uint[Constants.PhrasesPerSet];
            fixed (uint* resultPointer = result)
            {
                MD5Unmanaged.ComputeMD5((uint*)input.Buffer, resultPointer);
            }

            return result;
        }
    }
}
