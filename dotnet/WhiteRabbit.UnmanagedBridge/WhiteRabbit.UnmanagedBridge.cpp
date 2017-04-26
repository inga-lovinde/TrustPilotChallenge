// This is the main DLL file.

#include "stdafx.h"

#include "WhiteRabbit.UnmanagedBridge.h"
#include "md5.h"

void WhiteRabbitUnmanagedBridge::MD5Unmanaged::ComputeMD5(unsigned __int32 * input, unsigned __int32 * output)
{
#if AVX2
    md5(input + 0 * 8 * 8, output + 0 * 8);
    md5(input + 1 * 8 * 8, output + 1 * 8);
#elif SIMD
    md5(input + 0 * 8 * 4, output + 0 * 4);
    md5(input + 1 * 8 * 4, output + 1 * 4);
    if (input[2 * 8 * 4] != 0)
    {
        md5(input + 2 * 8 * 4, output + 0 * 4);
        md5(input + 3 * 8 * 4, output + 1 * 4);
    }
#else
    for (int i = 0; i < 16; i++)
    {
        md5(input + i * 8, output + i);
    }
#endif
}

void WhiteRabbitUnmanagedBridge::MD5Unmanaged::FillPhraseSet(__int64* bufferPointer, __int64* allWordsPointer, __int32* wordIndexes, unsigned __int64* permutationsPointer, int permutationOffset, int numberOfCharacters, int numberOfWords)
{
    __int64* longBuffer = (__int64*)bufferPointer;

    auto currentPermutationPointer = permutationsPointer + permutationOffset;
    for (auto i = 0; i < PHRASES_PER_SET; i++, currentPermutationPointer++)
    {
        auto permutation = *currentPermutationPointer;
        if (permutation == 0)
        {
            continue;
        }

        auto cumulativeWordOffsetX4 = 0;
        for (auto j = 0; j < numberOfWords; j++)
        {
            auto currentWord = allWordsPointer + wordIndexes[permutation & 15] * 128;
            permutation = permutation >> 4;
            longBuffer[0] |= currentWord[cumulativeWordOffsetX4 + 0];
            longBuffer[1] |= currentWord[cumulativeWordOffsetX4 + 1];
            longBuffer[2] ^= currentWord[cumulativeWordOffsetX4 + 2];
            longBuffer[3] ^= currentWord[cumulativeWordOffsetX4 + 3];
            cumulativeWordOffsetX4 += (__int32)currentWord[127];
        }

        longBuffer += 4;
    }

    auto length = numberOfCharacters + numberOfWords - 1;
    unsigned char* byteBuffer = ((unsigned char*)bufferPointer) + length;
    for (auto i = 0; i < PHRASES_PER_SET; i++)
    {
        *byteBuffer = 128;
        byteBuffer += 32;
    }

    auto lengthInBits = (unsigned __int32)(length << 3);
    unsigned int* uintBuffer = ((unsigned __int32*)bufferPointer) + 7;
    for (auto i = 0; i < PHRASES_PER_SET; i++)
    {
        *uintBuffer = lengthInBits;
        uintBuffer += 8;
    }
}
