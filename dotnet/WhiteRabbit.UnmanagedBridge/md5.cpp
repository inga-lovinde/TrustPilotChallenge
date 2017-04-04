#include "stdafx.h"

#include "md5.h"

#include "intrin.h"
#include "immintrin.h"

#pragma unmanaged

typedef unsigned int MD5Word;

inline MD5Word Blend(MD5Word a, MD5Word b, MD5Word x)
{
    return (x & b) | (~x & a);
}

inline MD5Word Xor(MD5Word a, MD5Word b, MD5Word c)
{
    return a ^ b ^ c;
}

inline MD5Word I(MD5Word a, MD5Word b, MD5Word c)
{
    return a ^ (b | ~c);
}

template<int r>
inline MD5Word LeftRotate(MD5Word x)
{
    return _rotl(x, r);
}

template<int r>
inline MD5Word Step1(MD5Word a, MD5Word b, MD5Word c, MD5Word d, MD5Word k, MD5Word w)
{
    return b + LeftRotate<r>(k + a + Blend(d, c, b) + w);
}

template<int r>
inline MD5Word Step1(MD5Word a, MD5Word b, MD5Word c, MD5Word d, MD5Word k)
{
    return b + LeftRotate<r>(k + a + Blend(d, c, b));
}

template<int r>
inline MD5Word Step2(MD5Word a, MD5Word b, MD5Word c, MD5Word d, MD5Word k, MD5Word w)
{
    return c + LeftRotate<r>(k + a + Blend(d, c, b) + w);
}

template<int r>
inline MD5Word Step2(MD5Word a, MD5Word b, MD5Word c, MD5Word d, MD5Word k)
{
    return c + LeftRotate<r>(k + a + Blend(d, c, b));
}

template<int r>
inline MD5Word Step3(MD5Word a, MD5Word b, MD5Word c, MD5Word d, MD5Word k, MD5Word w)
{
    return b + LeftRotate<r>(k + a + Xor(b, c, d) + w);
}

template<int r>
inline MD5Word Step3(MD5Word a, MD5Word b, MD5Word c, MD5Word d, MD5Word k)
{
    return b + LeftRotate<r>(k + a + Xor(b, c, d));
}

template<int r>
inline MD5Word Step4(MD5Word a, MD5Word b, MD5Word c, MD5Word d, MD5Word k, MD5Word w)
{
    return b + LeftRotate<r>(k + a + I(c, b, d) + w);
}

template<int r>
inline MD5Word Step4(MD5Word a, MD5Word b, MD5Word c, MD5Word d, MD5Word k)
{
    return b + LeftRotate<r>(k + a + I(c, b, d));
}

void md5(unsigned int * input, unsigned int * output)
{

    MD5Word a = 0x67452301;
    MD5Word b = 0xefcdab89;
    MD5Word c = 0x98badcfe;
    MD5Word d = 0x10325476;

    a = Step1<7>(a, b, c, d, 0xd76aa478, input[0]);
    d = Step1<12>(d, a, b, c, 0xe8c7b756, input[1]);
    c = Step1<17>(c, d, a, b, 0x242070db, input[2]);
    b = Step1<22>(b, c, d, a, 0xc1bdceee, input[3]);
    a = Step1<7>(a, b, c, d, 0xf57c0faf, input[4]);
    d = Step1<12>(d, a, b, c, 0x4787c62a, input[5]);
    c = Step1<17>(c, d, a, b, 0xa8304613, input[6]);
    b = Step1<22>(b, c, d, a, 0xfd469501);
    a = Step1<7>(a, b, c, d, 0x698098d8);
    d = Step1<12>(d, a, b, c, 0x8b44f7af);
    c = Step1<17>(c, d, a, b, 0xffff5bb1);
    b = Step1<22>(b, c, d, a, 0x895cd7be);
    a = Step1<7>(a, b, c, d, 0x6b901122);
    d = Step1<12>(d, a, b, c, 0xfd987193);
    c = Step1<17>(c, d, a, b, 0xa679438e, input[7]);
    b = Step1<22>(b, c, d, a, 0x49b40821);

    a = Step2<5>(a, d, b, c, 0xf61e2562, input[1]);
    d = Step2<9>(d, c, a, b, 0xc040b340, input[6]);
    c = Step2<14>(c, b, d, a, 0x265e5a51);
    b = Step2<20>(b, a, c, d, 0xe9b6c7aa, input[0]);
    a = Step2<5>(a, d, b, c, 0xd62f105d, input[5]);
    d = Step2<9>(d, c, a, b, 0x02441453);
    c = Step2<14>(c, b, d, a, 0xd8a1e681);
    b = Step2<20>(b, a, c, d, 0xe7d3fbc8, input[4]);
    a = Step2<5>(a, d, b, c, 0x21e1cde6);
    d = Step2<9>(d, c, a, b, 0xc33707d6, input[7]);
    c = Step2<14>(c, b, d, a, 0xf4d50d87, input[3]);
    b = Step2<20>(b, a, c, d, 0x455a14ed);
    a = Step2<5>(a, d, b, c, 0xa9e3e905);
    d = Step2<9>(d, c, a, b, 0xfcefa3f8, input[2]);
    c = Step2<14>(c, b, d, a, 0x676f02d9);
    b = Step2<20>(b, a, c, d, 0x8d2a4c8a);

    a = Step3<4>(a, b, c, d, 0xfffa3942, input[5]);
    d = Step3<11>(d, a, b, c, 0x8771f681);
    c = Step3<16>(c, d, a, b, 0x6d9d6122);
    b = Step3<23>(b, c, d, a, 0xfde5380c, input[7]);
    a = Step3<4>(a, b, c, d, 0xa4beea44, input[1]);
    d = Step3<11>(d, a, b, c, 0x4bdecfa9, input[4]);
    c = Step3<16>(c, d, a, b, 0xf6bb4b60);
    b = Step3<23>(b, c, d, a, 0xbebfbc70);
    a = Step3<4>(a, b, c, d, 0x289b7ec6);
    d = Step3<11>(d, a, b, c, 0xeaa127fa, input[0]);
    c = Step3<16>(c, d, a, b, 0xd4ef3085, input[3]);
    b = Step3<23>(b, c, d, a, 0x04881d05, input[6]);
    a = Step3<4>(a, b, c, d, 0xd9d4d039);
    d = Step3<11>(d, a, b, c, 0xe6db99e5);
    c = Step3<16>(c, d, a, b, 0x1fa27cf8);
    b = Step3<23>(b, c, d, a, 0xc4ac5665, input[2]);

    a = Step4<6>(a, b, c, d, 0xf4292244, input[0]);
    d = Step4<10>(d, a, b, c, 0x432aff97);
    c = Step4<15>(c, d, a, b, 0xab9423a7, input[7]);
    b = Step4<21>(b, c, d, a, 0xfc93a039, input[5]);
    a = Step4<6>(a, b, c, d, 0x655b59c3);
    d = Step4<10>(d, a, b, c, 0x8f0ccc92, input[3]);
    c = Step4<15>(c, d, a, b, 0xffeff47d);
    b = Step4<21>(b, c, d, a, 0x85845dd1, input[1]);
    a = Step4<6>(a, b, c, d, 0x6fa87e4f);
    d = Step4<10>(d, a, b, c, 0xfe2ce6e0);
    c = Step4<15>(c, d, a, b, 0xa3014314, input[6]);
    b = Step4<21>(b, c, d, a, 0x4e0811a1);
    a = Step4<6>(a, b, c, d, 0xf7537e82, input[4]);
    d = Step4<10>(d, a, b, c, 0xbd3af235);
    c = Step4<15>(c, d, a, b, 0x2ad7d2bb, input[2]);
    b = Step4<21>(b, c, d, a, 0xeb86d391);

    output[0] = 0x67452301 + a;
    output[1] = 0xefcdab89 + b;
    output[2] = 0x98badcfe + c;
    output[3] = 0x10325476 + d;
}
#pragma managed
