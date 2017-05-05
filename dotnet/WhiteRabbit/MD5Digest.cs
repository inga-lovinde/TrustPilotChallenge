namespace WhiteRabbit
{
    using System.Runtime.CompilerServices;
    using WhiteRabbitUnmanagedBridge;

    internal static class MD5Digest
    {
        // It only returns first component of MD5 hash
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Compute(PhraseSet input)
        {
            fixed (uint* inputBuffer = input.Buffer)
            {
                MD5Unmanaged.ComputeMD5(inputBuffer);
            }
        }
    }
}
