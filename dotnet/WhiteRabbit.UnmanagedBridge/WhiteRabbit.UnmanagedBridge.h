// WhiteRabbit.Unmanaged.h

#pragma once

#include "constants.h"

using namespace System;

namespace WhiteRabbitUnmanagedBridge {

	public ref class MD5Unmanaged
	{
        public:
            literal int PhrasesPerSet = PHRASES_PER_SET;
            static void ComputeMD5(unsigned int* input);
            static void FillPhraseSet(__int64* bufferPointer, __int64* allWordsPointer, __int32* wordIndexes, unsigned __int64* permutationsPointer, int numberOfCharacters, int numberOfWords);
	};
}
