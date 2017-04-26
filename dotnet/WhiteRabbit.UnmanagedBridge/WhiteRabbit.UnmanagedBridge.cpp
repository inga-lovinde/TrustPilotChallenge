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

template<int phraseOffset>
struct Filler
{
    static void FillPhraseSetLength(unsigned __int32* uintBuffer, const unsigned __int32 lengthInBits)
    {
        *uintBuffer = lengthInBits;
        Filler<phraseOffset + 1>::FillPhraseSetLength(uintBuffer + 8, lengthInBits);
    }
};

template<>
struct Filler<PHRASES_PER_SET>
{
    static void FillPhraseSetLength(const unsigned __int32* uintBuffer, const unsigned __int32 lengthInBits)
    {
    }
};

void WhiteRabbitUnmanagedBridge::MD5Unmanaged::FillPhraseSet(__int64* bufferPointer, __int64* allWordsPointer, __int32* wordIndexes, unsigned __int64* permutationsPointer, int permutationOffset, int numberOfCharacters, int numberOfWords)
{
    unsigned __int64 permutations[PHRASES_PER_SET];
    __int32 cumulativeWordOffsets[PHRASES_PER_SET];
    __int32 permutationOffsetInBytes = permutationOffset * sizeof(*permutations);
#define INIT_DATA(phraseNumber) \
    permutations[phraseNumber] = *(unsigned __int64*)(((char*)permutationsPointer) + permutationOffsetInBytes + phraseNumber * sizeof(*permutations)); \
    cumulativeWordOffsets[phraseNumber] = 0;

    INIT_DATA(0);
    INIT_DATA(1);
    INIT_DATA(2);
    INIT_DATA(3);
    INIT_DATA(4);
    INIT_DATA(5);
    INIT_DATA(6);
    INIT_DATA(7);
    INIT_DATA(8);
    INIT_DATA(9);
    INIT_DATA(10);
    INIT_DATA(11);
    INIT_DATA(12);
    INIT_DATA(13);
    INIT_DATA(14);
    INIT_DATA(15);

    for (auto j = 0; j < numberOfWords; j++)
    {
        for (auto i = 0; i < PHRASES_PER_SET; i++)
        {
            auto currentWord = allWordsPointer + wordIndexes[permutations[i] & 15] * 128;
            permutations[i] = permutations[i] >> 4;
            bufferPointer[i * 4 + 0] |= currentWord[cumulativeWordOffsets[i] + 0];
            bufferPointer[i * 4 + 1] |= currentWord[cumulativeWordOffsets[i] + 1];
            bufferPointer[i * 4 + 2] |= currentWord[cumulativeWordOffsets[i] + 2];
            bufferPointer[i * 4 + 3] |= currentWord[cumulativeWordOffsets[i] + 3];
            cumulativeWordOffsets[i] += (__int32)currentWord[127];
        }
    }

    auto length = numberOfCharacters + numberOfWords - 1;
    auto lengthInBits = (unsigned __int32)(length << 3);

#define FILL_PHRASE_LAST_BYTE(phraseNumber, byteBuffer) ((unsigned char*)bufferPointer)[length + phraseNumber * 32] = 128;
#define FILL_PHRASE_SET_LENGTH(phraseNumber, uintBuffer, lengthInBits) ((unsigned __int32*)bufferPointer)[7 + phraseNumber * 8] = lengthInBits;

    FILL_PHRASE_LAST_BYTE(0, byteBuffer);
    FILL_PHRASE_LAST_BYTE(1, byteBuffer);
    FILL_PHRASE_LAST_BYTE(2, byteBuffer);
    FILL_PHRASE_LAST_BYTE(3, byteBuffer);
    FILL_PHRASE_SET_LENGTH(0, uintBuffer, lengthInBits);
    FILL_PHRASE_SET_LENGTH(1, uintBuffer, lengthInBits);
    FILL_PHRASE_SET_LENGTH(2, uintBuffer, lengthInBits);
    FILL_PHRASE_SET_LENGTH(3, uintBuffer, lengthInBits);

    FILL_PHRASE_LAST_BYTE(4, byteBuffer);
    FILL_PHRASE_LAST_BYTE(5, byteBuffer);
    FILL_PHRASE_LAST_BYTE(6, byteBuffer);
    FILL_PHRASE_LAST_BYTE(7, byteBuffer);
    FILL_PHRASE_SET_LENGTH(4, uintBuffer, lengthInBits);
    FILL_PHRASE_SET_LENGTH(5, uintBuffer, lengthInBits);
    FILL_PHRASE_SET_LENGTH(6, uintBuffer, lengthInBits);
    FILL_PHRASE_SET_LENGTH(7, uintBuffer, lengthInBits);

    FILL_PHRASE_LAST_BYTE(8, byteBuffer);
    FILL_PHRASE_LAST_BYTE(9, byteBuffer);
    FILL_PHRASE_LAST_BYTE(10, byteBuffer);
    FILL_PHRASE_LAST_BYTE(11, byteBuffer);
    FILL_PHRASE_SET_LENGTH(8, uintBuffer, lengthInBits);
    FILL_PHRASE_SET_LENGTH(9, uintBuffer, lengthInBits);
    FILL_PHRASE_SET_LENGTH(10, uintBuffer, lengthInBits);
    FILL_PHRASE_SET_LENGTH(11, uintBuffer, lengthInBits);

    FILL_PHRASE_LAST_BYTE(12, byteBuffer);
    FILL_PHRASE_LAST_BYTE(13, byteBuffer);
    FILL_PHRASE_LAST_BYTE(14, byteBuffer);
    FILL_PHRASE_LAST_BYTE(15, byteBuffer);
    FILL_PHRASE_SET_LENGTH(12, uintBuffer, lengthInBits);
    FILL_PHRASE_SET_LENGTH(13, uintBuffer, lengthInBits);
    FILL_PHRASE_SET_LENGTH(14, uintBuffer, lengthInBits);
    FILL_PHRASE_SET_LENGTH(15, uintBuffer, lengthInBits);
}
