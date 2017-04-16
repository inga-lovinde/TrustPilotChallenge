// This is the main DLL file.

#include "stdafx.h"

#include "WhiteRabbit.UnmanagedBridge.h"
#include "md5.h"

void WhiteRabbitUnmanagedBridge::MD5Unmanaged::ComputeMD5(unsigned __int32 * input, unsigned __int32 * output)
{
#if AVX2
    md5(input + 0 * 8 * 8, output + 0 * 8);
    md5(input + 1 * 8 * 8, output + 0 * 8);
#elif SIMD
    md5(input + 0 * 8 * 4, output + 0 * 4);
    md5(input + 1 * 8 * 4, output + 1 * 4);
    if (input[2 * 8 * 4] != 0)
    {
        md5(input + 2 * 8 * 4, output + 0 * 4);
        md5(input + 3 * 8 * 4, output + 1 * 4);
    }
#else
    for (int i = 0; i < 16; i++)
    {
        md5(input + i * 8, output + i);
    }
#endif
}
