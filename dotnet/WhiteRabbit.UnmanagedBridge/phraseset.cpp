#include "stdafx.h"
#include "phraseset.h"
#include "constants.h"

#include "intrin.h"

#pragma unmanaged

#define REPEAT(macro) \
    macro(0); \
    macro(1); \
    macro(2); \
    macro(3); \
    macro(4); \
    macro(5); \
    macro(6); \
    macro(7);


void fillPhraseSet(__int64* bufferPointer, unsigned __int64* allWordsPointer, __int32* wordIndexes, unsigned __int64* permutationsPointer, int permutationOffset, int numberOfCharacters, int numberOfWords)
{
    unsigned __int64 permutations[PHRASES_PER_SET];
    unsigned __int64 cumulativeWordOffsets = 0;

    auto avx2buffer = (__m256i*)bufferPointer;

#define INIT_DATA(phraseNumber) \
    permutations[phraseNumber] = permutationsPointer[permutationOffset + phraseNumber]; \

    REPEAT(INIT_DATA);

#define PROCESS_WORD(phraseNumber) \
    { \
        auto currentWord = allWordsPointer + wordIndexes[permutations[phraseNumber] % 16] * 128; \
        permutations[phraseNumber] >>= 4; \
        avx2buffer[phraseNumber] = _mm256_or_si256(avx2buffer[phraseNumber], *(__m256i*)(currentWord + ((cumulativeWordOffsets >> (8 * (phraseNumber % 8))) % 256))); \
        cumulativeWordOffsets += (((unsigned __int64*)currentWord)[127]) << (8 * (phraseNumber % 8)); \
    }

    for (auto j = 0; j < numberOfWords; j++)
    {
        REPEAT(PROCESS_WORD);
    }

    auto length = numberOfCharacters + numberOfWords - 1;
    auto lengthInBits = (unsigned __int32)(length << 3);

#define FILL_PHRASE_LAST_BYTE(phraseNumber) ((unsigned char*)bufferPointer)[length + phraseNumber * 32] = 128;
#define FILL_PHRASE_SET_LENGTH(phraseNumber) ((unsigned __int32*)bufferPointer)[7 + phraseNumber * 8] = lengthInBits;

    REPEAT(FILL_PHRASE_LAST_BYTE);
    REPEAT(FILL_PHRASE_SET_LENGTH);
}

#pragma managed
