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
            static void FillPhraseSet(unsigned __int64* initialBufferPointer, unsigned __int64* bufferPointer, unsigned __int64* allWordsPointer, __int32* wordIndexes, unsigned __int64* permutationsPointer, int numberOfWords);
	};
}
