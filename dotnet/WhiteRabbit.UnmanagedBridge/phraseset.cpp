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

#define REPEAT_WORDS_SIMPLE1(phraseNumber) \
    { \
        PROCESS_WORD(phraseNumber, 0); \
    }

#define REPEAT_WORDS_SIMPLE2(phraseNumber) \
    { \
        REPEAT_WORDS_SIMPLE1(phraseNumber); \
        PROCESS_WORD(phraseNumber, 1); \
    }

#define REPEAT_WORDS_SIMPLE3(phraseNumber) \
    { \
        REPEAT_WORDS_SIMPLE2(phraseNumber); \
        PROCESS_WORD(phraseNumber, 2); \
    }

#define REPEAT_WORDS_SIMPLE4(phraseNumber) \
    { \
        REPEAT_WORDS_SIMPLE3(phraseNumber); \
        PROCESS_WORD(phraseNumber, 3); \
    }

#define REPEAT_WORDS_SIMPLE5(phraseNumber) \
    { \
        REPEAT_WORDS_SIMPLE4(phraseNumber); \
        PROCESS_WORD(phraseNumber, 4); \
    }

#define REPEAT_WORDS_SIMPLE6(phraseNumber) \
    { \
        REPEAT_WORDS_SIMPLE5(phraseNumber); \
        PROCESS_WORD(phraseNumber, 5); \
    }

#define REPEAT_WORDS_SIMPLE7(phraseNumber) \
    { \
        REPEAT_WORDS_SIMPLE6(phraseNumber); \
        PROCESS_WORD(phraseNumber, 6); \
    }

#define REPEAT_WORDS_SIMPLE8(phraseNumber) \
    { \
        REPEAT_WORDS_SIMPLE7(phraseNumber); \
        PROCESS_WORD(phraseNumber, 7); \
    }

#define REPEAT_WORDS_SIMPLE9(phraseNumber) \
    { \
        REPEAT_WORDS_SIMPLE8(phraseNumber); \
        PROCESS_WORD(phraseNumber, 8); \
    }

#define REPEAT_WORDS_SIMPLE10(phraseNumber) \
    { \
        REPEAT_WORDS_SIMPLE9(phraseNumber); \
        PROCESS_WORD(phraseNumber, 9); \
    }

#define REPEAT_WORDS(phraseNumber, repeater) \
    { \
        INIT_WORD(phraseNumber); \
        repeater(phraseNumber); \
        DONE_WORD(phraseNumber); \
    }

#define REPEAT_WORDS1(phraseNumber) REPEAT_WORDS(phraseNumber, REPEAT_WORDS_SIMPLE1)
#define REPEAT_WORDS2(phraseNumber) REPEAT_WORDS(phraseNumber, REPEAT_WORDS_SIMPLE2)
#define REPEAT_WORDS3(phraseNumber) REPEAT_WORDS(phraseNumber, REPEAT_WORDS_SIMPLE3)
#define REPEAT_WORDS4(phraseNumber) REPEAT_WORDS(phraseNumber, REPEAT_WORDS_SIMPLE4)
#define REPEAT_WORDS5(phraseNumber) REPEAT_WORDS(phraseNumber, REPEAT_WORDS_SIMPLE5)
#define REPEAT_WORDS6(phraseNumber) REPEAT_WORDS(phraseNumber, REPEAT_WORDS_SIMPLE6)
#define REPEAT_WORDS7(phraseNumber) REPEAT_WORDS(phraseNumber, REPEAT_WORDS_SIMPLE7)
#define REPEAT_WORDS8(phraseNumber) REPEAT_WORDS(phraseNumber, REPEAT_WORDS_SIMPLE8)
#define REPEAT_WORDS9(phraseNumber) REPEAT_WORDS(phraseNumber, REPEAT_WORDS_SIMPLE9)
#define REPEAT_WORDS10(phraseNumber) REPEAT_WORDS(phraseNumber, REPEAT_WORDS_SIMPLE10)


void fillPhraseSet(__int64* bufferPointer, unsigned __int64* allWordsPointer, __int32* wordIndexes, unsigned __int64* permutationsPointer, int permutationOffset, int numberOfCharacters, int numberOfWords)
{
    auto avx2buffer = (__m256i*)bufferPointer;

    switch (numberOfWords)
    {
    case 1:
        REPEAT_PHRASES(REPEAT_WORDS1);
        break;
    case 2:
        REPEAT_PHRASES(REPEAT_WORDS2);
        break;
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
    case 9:
        REPEAT_PHRASES(REPEAT_WORDS9);
        break;
    case 10:
        REPEAT_PHRASES(REPEAT_WORDS10);
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
