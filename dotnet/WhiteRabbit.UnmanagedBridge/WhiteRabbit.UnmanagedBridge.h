// WhiteRabbit.Unmanaged.h

#pragma once

using namespace System;

namespace WhiteRabbitUnmanagedBridge {

	public ref class MD5Unmanaged
	{
        public:
            literal int PhrasesPerSet = 16;
            static void ComputeMD5(unsigned int* input, unsigned int* output);
	};
}
