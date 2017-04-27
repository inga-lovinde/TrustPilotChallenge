#include "stdafx.h"
#include "phraseset.h"
#include "constants.h"

#include "intrin.h"

#pragma unmanaged

void fillPhraseSet(__int64* bufferPointer, __int64* allWordsPointer, __int32* wordIndexes, unsigned __int64* permutationsPointer, int permutationOffset, int numberOfCharacters, int numberOfWords)
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

#define PROCESS_WORD(phraseNumber) \
    { \
        auto currentWord = allWordsPointer + wordIndexes[permutations[phraseNumber] & 15] * 128; \
        permutations[phraseNumber] = permutations[phraseNumber] >> 4; \
        bufferPointer[phraseNumber * 4 + 0] |= currentWord[cumulativeWordOffsets[phraseNumber] + 0]; \
        bufferPointer[phraseNumber * 4 + 1] |= currentWord[cumulativeWordOffsets[phraseNumber] + 1]; \
        bufferPointer[phraseNumber * 4 + 2] |= currentWord[cumulativeWordOffsets[phraseNumber] + 2]; \
        bufferPointer[phraseNumber * 4 + 3] |= currentWord[cumulativeWordOffsets[phraseNumber] + 3]; \
        cumulativeWordOffsets[phraseNumber] += (__int32)currentWord[127]; \
    }

    for (auto j = 0; j < numberOfWords; j++)
    {
        PROCESS_WORD(0);
        PROCESS_WORD(1);
        PROCESS_WORD(2);
        PROCESS_WORD(3);
        PROCESS_WORD(4);
        PROCESS_WORD(5);
        PROCESS_WORD(6);
        PROCESS_WORD(7);
        PROCESS_WORD(8);
        PROCESS_WORD(9);
        PROCESS_WORD(10);
        PROCESS_WORD(11);
        PROCESS_WORD(12);
        PROCESS_WORD(13);
        PROCESS_WORD(14);
        PROCESS_WORD(15);
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

#pragma managed
