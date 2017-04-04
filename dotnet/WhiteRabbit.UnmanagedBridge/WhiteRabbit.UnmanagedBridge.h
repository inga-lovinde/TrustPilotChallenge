// WhiteRabbit.Unmanaged.h

#pragma once

using namespace System;

namespace WhiteRabbitUnmanagedBridge {

	public ref class MD5Unmanaged
	{
        public:
            static void ComputeMD5(unsigned int* input, unsigned int* output);
	};
}
