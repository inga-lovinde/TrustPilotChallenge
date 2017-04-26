// WhiteRabbit.Unmanaged.h

#pragma once

using namespace System;

namespace WhiteRabbitUnmanagedBridge {

	public ref class MD5Unmanaged
	{
        public:
            literal int PhrasesPerSet = 16;
            static void ComputeMD5(unsigned int* input, unsigned int* output);
            static void FillPhraseSet(__int64* bufferPointer, __int64* allWordsPointer, __int32* wordIndexes, unsigned __int64* permutationsPointer, int permutationOffset, int numberOfCharacters, int numberOfWords);
	};
}
