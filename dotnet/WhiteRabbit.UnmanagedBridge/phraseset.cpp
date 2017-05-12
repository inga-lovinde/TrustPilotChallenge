#include "stdafx.h"
#include "phraseset.h"
#include "constants.h"

#include "intrin.h"

#pragma unmanaged

template<int numberOfWords>
class Processor
{
public:
    template<int wordNumber>
    static __forceinline const __m256i ProcessWord(const __m256i phrase, const unsigned __int64 cumulativeWordOffset, const unsigned __int64 permutation, unsigned __int64* allWordsPointer, __int32* wordIndexes)
    {
        auto currentWord = allWordsPointer + wordIndexes[_bextr_u64(permutation, 4 * wordNumber, 4)] * 128;

        return ProcessWord<wordNumber + 1>(
            _mm256_xor_si256(phrase, *(__m256i*)(currentWord + cumulativeWordOffset)),
            cumulativeWordOffset + currentWord[127],
            permutation,
            allWordsPointer,
            wordIndexes);
    }

    template<>
    static __forceinline const __m256i ProcessWord<numberOfWords>(const __m256i phrase, const unsigned __int64 cumulativeWordOffset, const unsigned __int64 permutation, unsigned __int64* allWordsPointer, __int32* wordIndexes)
    {
        return phrase;
    }

    template<int phraseNumber>
    static __forceinline void ProcessWordsForPhrase(__m256i* avx2initialBuffer, __m256i* avx2buffer, unsigned __int64* allWordsPointer, __int32* wordIndexes, unsigned __int64* permutationsPointer)
    {
        avx2buffer[phraseNumber] = ProcessWord<0>(*avx2initialBuffer, 0, permutationsPointer[phraseNumber], allWordsPointer, wordIndexes);
        ProcessWordsForPhrase<phraseNumber + 1>(avx2initialBuffer, avx2buffer, allWordsPointer, wordIndexes, permutationsPointer);
    }

    template<>
    static __forceinline void ProcessWordsForPhrase<PHRASES_PER_SET>(__m256i* avx2initialBuffer, __m256i* avx2buffer, unsigned __int64* allWordsPointer, __int32* wordIndexes, unsigned __int64* permutationsPointer)
    {
        return;
    }
};

void fillPhraseSet(unsigned __int64* initialBufferPointer, unsigned __int64* bufferPointer, unsigned __int64* allWordsPointer, __int32* wordIndexes, unsigned __int64* permutationsPointer, int numberOfWords)
{
    auto avx2initialBuffer = (__m256i*)initialBufferPointer;
    auto avx2buffer = (__m256i*)bufferPointer;

    switch (numberOfWords)
    {
    case 1:
        Processor<1>::ProcessWordsForPhrase<0>(avx2initialBuffer, avx2buffer, allWordsPointer, wordIndexes, permutationsPointer);
        break;
    case 2:
        Processor<2>::ProcessWordsForPhrase<0>(avx2initialBuffer, avx2buffer, allWordsPointer, wordIndexes, permutationsPointer);
        break;
    case 3:
        Processor<3>::ProcessWordsForPhrase<0>(avx2initialBuffer, avx2buffer, allWordsPointer, wordIndexes, permutationsPointer);
        break;
    case 4:
        Processor<4>::ProcessWordsForPhrase<0>(avx2initialBuffer, avx2buffer, allWordsPointer, wordIndexes, permutationsPointer);
        break;
    case 5:
        Processor<5>::ProcessWordsForPhrase<0>(avx2initialBuffer, avx2buffer, allWordsPointer, wordIndexes, permutationsPointer);
        break;
    case 6:
        Processor<6>::ProcessWordsForPhrase<0>(avx2initialBuffer, avx2buffer, allWordsPointer, wordIndexes, permutationsPointer);
        break;
    case 7:
        Processor<7>::ProcessWordsForPhrase<0>(avx2initialBuffer, avx2buffer, allWordsPointer, wordIndexes, permutationsPointer);
        break;
    case 8:
        Processor<8>::ProcessWordsForPhrase<0>(avx2initialBuffer, avx2buffer, allWordsPointer, wordIndexes, permutationsPointer);
        break;
    case 9:
        Processor<9>::ProcessWordsForPhrase<0>(avx2initialBuffer, avx2buffer, allWordsPointer, wordIndexes, permutationsPointer);
        break;
    case 10:
        Processor<10>::ProcessWordsForPhrase<0>(avx2initialBuffer, avx2buffer, allWordsPointer, wordIndexes, permutationsPointer);
        break;
    }
}

#pragma managed
