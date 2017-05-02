#include "stdafx.h"
#include "phraseset.h"
#include "constants.h"

#include "intrin.h"

#pragma unmanaged

#define REPEAT_PHRASES(macro) \
    macro(0); \
    macro(1); \
    macro(2); \
    macro(3); \
    macro(4); \
    macro(5); \
    macro(6); \
    macro(7); \
    macro(8); \
    macro(9); \
    macro(10); \
    macro(11); \
    macro(12); \
    macro(13); \
    macro(14); \
    macro(15);

#define INIT_WORD(phraseNumber) \
    auto permutation = permutationsPointer[permutationOffset + phraseNumber]; \
    unsigned __int64 cumulativeWordOffset = 0; \
    auto phrase = avx2buffer[phraseNumber];

#define PROCESS_WORD(phraseNumber, wordNumber) \
    { \
        auto currentWord = allWordsPointer + wordIndexes[permutation % 16] * 128; \
        phrase = _mm256_or_si256(phrase, *(__m256i*)(currentWord + cumulativeWordOffset)); \
        permutation >>= 4; \
        cumulativeWordOffset += currentWord[127]; \
    }

#define DONE_WORD(phraseNumber) \
    avx2buffer[phraseNumber] = phrase;

#define REPEAT_WORDS3(phraseNumber) \
    { \
        INIT_WORD(phraseNumber); \
        PROCESS_WORD(phraseNumber, 0); \
        PROCESS_WORD(phraseNumber, 1); \
        PROCESS_WORD(phraseNumber, 2); \
        DONE_WORD(phraseNumber); \
    }

#define REPEAT_WORDS4(phraseNumber) \
    { \
        INIT_WORD(phraseNumber); \
        PROCESS_WORD(phraseNumber, 0); \
        PROCESS_WORD(phraseNumber, 1); \
        PROCESS_WORD(phraseNumber, 2); \
        PROCESS_WORD(phraseNumber, 3); \
        DONE_WORD(phraseNumber); \
    }

#define REPEAT_WORDS5(phraseNumber) \
    { \
        INIT_WORD(phraseNumber); \
        PROCESS_WORD(phraseNumber, 0); \
        PROCESS_WORD(phraseNumber, 1); \
        PROCESS_WORD(phraseNumber, 2); \
        PROCESS_WORD(phraseNumber, 3); \
        PROCESS_WORD(phraseNumber, 4); \
        DONE_WORD(phraseNumber); \
    }

#define REPEAT_WORDS6(phraseNumber) \
    { \
        INIT_WORD(phraseNumber); \
        PROCESS_WORD(phraseNumber, 0); \
        PROCESS_WORD(phraseNumber, 1); \
        PROCESS_WORD(phraseNumber, 2); \
        PROCESS_WORD(phraseNumber, 3); \
        PROCESS_WORD(phraseNumber, 4); \
        PROCESS_WORD(phraseNumber, 5); \
        DONE_WORD(phraseNumber); \
    }

#define REPEAT_WORDS7(phraseNumber) \
    { \
        INIT_WORD(phraseNumber); \
        PROCESS_WORD(phraseNumber, 0); \
        PROCESS_WORD(phraseNumber, 1); \
        PROCESS_WORD(phraseNumber, 2); \
        PROCESS_WORD(phraseNumber, 3); \
        PROCESS_WORD(phraseNumber, 4); \
        PROCESS_WORD(phraseNumber, 5); \
        PROCESS_WORD(phraseNumber, 6); \
        DONE_WORD(phraseNumber); \
    }

#define REPEAT_WORDS8(phraseNumber) \
    { \
        INIT_WORD(phraseNumber); \
        PROCESS_WORD(phraseNumber, 0); \
        PROCESS_WORD(phraseNumber, 1); \
        PROCESS_WORD(phraseNumber, 2); \
        PROCESS_WORD(phraseNumber, 3); \
        PROCESS_WORD(phraseNumber, 4); \
        PROCESS_WORD(phraseNumber, 5); \
        PROCESS_WORD(phraseNumber, 6); \
        PROCESS_WORD(phraseNumber, 7); \
        DONE_WORD(phraseNumber); \
    }


void fillPhraseSet(__int64* bufferPointer, unsigned __int64* allWordsPointer, __int32* wordIndexes, unsigned __int64* permutationsPointer, int permutationOffset, int numberOfCharacters, int numberOfWords)
{
    auto avx2buffer = (__m256i*)bufferPointer;

    switch (numberOfWords)
    {
    case 3:
        REPEAT_PHRASES(REPEAT_WORDS3);
        break;
    case 4:
        REPEAT_PHRASES(REPEAT_WORDS4);
        break;
    case 5:
        REPEAT_PHRASES(REPEAT_WORDS5);
        break;
    case 6:
        REPEAT_PHRASES(REPEAT_WORDS6);
        break;
    case 7:
        REPEAT_PHRASES(REPEAT_WORDS7);
        break;
    case 8:
        REPEAT_PHRASES(REPEAT_WORDS8);
        break;
    }

    auto length = numberOfCharacters + numberOfWords - 1;
    auto lengthInBits = (unsigned __int32)(length << 3);

#define FILL_PHRASE_LAST_BYTE(phraseNumber) ((unsigned char*)bufferPointer)[length + phraseNumber * 32] = 128;
#define FILL_PHRASE_SET_LENGTH(phraseNumber) ((unsigned __int32*)bufferPointer)[7 + phraseNumber * 8] = lengthInBits;

    REPEAT_PHRASES(FILL_PHRASE_LAST_BYTE);
    REPEAT_PHRASES(FILL_PHRASE_SET_LENGTH);
}

#pragma managed
