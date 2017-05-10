// This is the main DLL file.

#include "stdafx.h"

#include "WhiteRabbit.UnmanagedBridge.h"
#include "md5.h"
#include "phraseset.h"

void WhiteRabbitUnmanagedBridge::MD5Unmanaged::ComputeMD5(unsigned __int32 * input)
{
#if AVX2
    md5(input + 0 * 8 * 8);
    md5(input + 1 * 8 * 8);
#elif SIMD
    md5(input + 0 * 8 * 4);
    md5(input + 1 * 8 * 4);
    if (input[2 * 8 * 4] != 0)
    {
        md5(input + 2 * 8 * 4);
        md5(input + 3 * 8 * 4);
    }
#else
    for (int i = 0; i < 16; i++)
    {
        md5(input + i * 8);
    }
#endif
}

void WhiteRabbitUnmanagedBridge::MD5Unmanaged::FillPhraseSet(unsigned __int64* initialBufferPointer, unsigned __int64* bufferPointer, unsigned __int64* allWordsPointer, __int32* wordIndexes, unsigned __int64* permutationsPointer, int numberOfWords)
{
    fillPhraseSet(initialBufferPointer, bufferPointer, allWordsPointer, wordIndexes, permutationsPointer, numberOfWords);
}
