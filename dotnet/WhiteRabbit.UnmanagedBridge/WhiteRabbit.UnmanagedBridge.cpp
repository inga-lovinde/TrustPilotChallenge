// This is the main DLL file.

#include "stdafx.h"

#include "WhiteRabbit.UnmanagedBridge.h"
#include "md5.h"

void WhiteRabbitUnmanagedBridge::MD5Unmanaged::ComputeMD5(unsigned __int32 * input, unsigned __int32 * output)
{
#if AVX2
    md5(input + 0 * 8 * 8, output + 0 * 8);
#elif SIMD
    md5(input + 0 * 8 * 4, output + 0 * 4);
    md5(input + 1 * 8 * 4, output + 1 * 4);
#else
    md5(input + 0 * 8, output + 0);
    md5(input + 1 * 8, output + 1);
    md5(input + 2 * 8, output + 2);
    md5(input + 3 * 8, output + 3);
    md5(input + 4 * 8, output + 4);
    md5(input + 5 * 8, output + 5);
    md5(input + 6 * 8, output + 6);
    md5(input + 7 * 8, output + 7);
#endif
}
