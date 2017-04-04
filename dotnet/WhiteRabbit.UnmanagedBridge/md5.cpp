#include "stdafx.h"

#include "md5.h"

#include "intrin.h"
#include "immintrin.h"

#pragma unmanaged

inline unsigned int Blend(unsigned int a, unsigned int b, unsigned int x)
{
    return (x & b) | (~x & a);
}

inline unsigned int Xor(unsigned int a, unsigned int b, unsigned int c)
{
    return a ^ b ^ c;
}

inline unsigned int I(unsigned int a, unsigned int b, unsigned int c)
{
    return a ^ (b | ~c);
}

inline unsigned int LeftRotate(unsigned int x, int left)
{
    return _rotl(x, left);
}

void md5(unsigned int * input, unsigned int* output)
{

    unsigned int a = 0x67452301;
    unsigned int b = 0xefcdab89;
    unsigned int c = 0x98badcfe;
    unsigned int d = 0x10325476;

    a = b + LeftRotate(0xd76aa478 + a + Blend(d, c, b) + input[0], 7);
    d = a + LeftRotate(0xe8c7b756 + d + Blend(c, b, a) + input[1], 12);
    c = d + LeftRotate(0x242070db + c + Blend(b, a, d) + input[2], 17);
    b = c + LeftRotate(0xc1bdceee + b + Blend(a, d, c) + input[3], 22);
    a = b + LeftRotate(0xf57c0faf + a + Blend(d, c, b) + input[4], 7);
    d = a + LeftRotate(0x4787c62a + d + Blend(c, b, a) + input[5], 12);
    c = d + LeftRotate(0xa8304613 + c + Blend(b, a, d) + input[6], 17);
    b = c + LeftRotate(0xfd469501 + b + Blend(a, d, c), 22);
    a = b + LeftRotate(0x698098d8 + a + Blend(d, c, b), 7);
    d = a + LeftRotate(0x8b44f7af + d + Blend(c, b, a), 12);
    c = d + LeftRotate(0xffff5bb1 + c + Blend(b, a, d), 17);
    b = c + LeftRotate(0x895cd7be + b + Blend(a, d, c), 22);
    a = b + LeftRotate(0x6b901122 + a + Blend(d, c, b), 7);
    d = a + LeftRotate(0xfd987193 + d + Blend(c, b, a), 12);
    c = d + LeftRotate(0xa679438e + c + Blend(b, a, d) + input[7], 17);
    b = c + LeftRotate(0x49b40821 + b + Blend(a, d, c), 22);

    a = b + LeftRotate(0xf61e2562 + a + Blend(c, b, d) + input[1], 5);
    d = a + LeftRotate(0xc040b340 + d + Blend(b, a, c) + input[6], 9);
    c = d + LeftRotate(0x265e5a51 + c + Blend(a, d, b), 14);
    b = c + LeftRotate(0xe9b6c7aa + b + Blend(d, c, a) + input[0], 20);
    a = b + LeftRotate(0xd62f105d + a + Blend(c, b, d) + input[5], 5);
    d = a + LeftRotate(0x02441453 + d + Blend(b, a, c), 9);
    c = d + LeftRotate(0xd8a1e681 + c + Blend(a, d, b), 14);
    b = c + LeftRotate(0xe7d3fbc8 + b + Blend(d, c, a) + input[4], 20);
    a = b + LeftRotate(0x21e1cde6 + a + Blend(c, b, d), 5);
    d = a + LeftRotate(0xc33707d6 + d + Blend(b, a, c) + input[7], 9);
    c = d + LeftRotate(0xf4d50d87 + c + Blend(a, d, b) + input[3], 14);
    b = c + LeftRotate(0x455a14ed + b + Blend(d, c, a), 20);
    a = b + LeftRotate(0xa9e3e905 + a + Blend(c, b, d), 5);
    d = a + LeftRotate(0xfcefa3f8 + d + Blend(b, a, c) + input[2], 9);
    c = d + LeftRotate(0x676f02d9 + c + Blend(a, d, b), 14);
    b = c + LeftRotate(0x8d2a4c8a + b + Blend(d, c, a), 20);

    a = b + LeftRotate(0xfffa3942 + a + Xor(b, c, d) + input[5], 4);
    d = a + LeftRotate(0x8771f681 + d + Xor(a, b, c), 11);
    c = d + LeftRotate(0x6d9d6122 + c + Xor(d, a, b), 16);
    b = c + LeftRotate(0xfde5380c + b + Xor(c, d, a) + input[7], 23);
    a = b + LeftRotate(0xa4beea44 + a + Xor(b, c, d) + input[1], 4);
    d = a + LeftRotate(0x4bdecfa9 + d + Xor(a, b, c) + input[4], 11);
    c = d + LeftRotate(0xf6bb4b60 + c + Xor(d, a, b), 16);
    b = c + LeftRotate(0xbebfbc70 + b + Xor(c, d, a), 23);
    a = b + LeftRotate(0x289b7ec6 + a + Xor(b, c, d), 4);
    d = a + LeftRotate(0xeaa127fa + d + Xor(a, b, c) + input[0], 11);
    c = d + LeftRotate(0xd4ef3085 + c + Xor(d, a, b) + input[3], 16);
    b = c + LeftRotate(0x04881d05 + b + Xor(c, d, a) + input[6], 23);
    a = b + LeftRotate(0xd9d4d039 + a + Xor(b, c, d), 4);
    d = a + LeftRotate(0xe6db99e5 + d + Xor(a, b, c), 11);
    c = d + LeftRotate(0x1fa27cf8 + c + Xor(d, a, b), 16);
    b = c + LeftRotate(0xc4ac5665 + b + Xor(c, d, a) + input[2], 23);

    a = b + LeftRotate(0xf4292244 + a + I(c, b, d) + input[0], 6);
    d = a + LeftRotate(0x432aff97 + d + I(b, a, c), 10);
    c = d + LeftRotate(0xab9423a7 + c + I(a, d, b) + input[7], 15);
    b = c + LeftRotate(0xfc93a039 + b + I(d, c, a) + input[5], 21);
    a = b + LeftRotate(0x655b59c3 + a + I(c, b, d), 6);
    d = a + LeftRotate(0x8f0ccc92 + d + I(b, a, c) + input[3], 10);
    c = d + LeftRotate(0xffeff47d + c + I(a, d, b), 15);
    b = c + LeftRotate(0x85845dd1 + b + I(d, c, a) + input[1], 21);
    a = b + LeftRotate(0x6fa87e4f + a + I(c, b, d), 6);
    d = a + LeftRotate(0xfe2ce6e0 + d + I(b, a, c), 10);
    c = d + LeftRotate(0xa3014314 + c + I(a, d, b) + input[6], 15);
    b = c + LeftRotate(0x4e0811a1 + b + I(d, c, a), 21);
    a = b + LeftRotate(0xf7537e82 + a + I(c, b, d) + input[4], 6);
    d = a + LeftRotate(0xbd3af235 + d + I(b, a, c), 10);
    c = d + LeftRotate(0x2ad7d2bb + c + I(a, d, b) + input[2], 15);
    b = c + LeftRotate(0xeb86d391 + b + I(d, c, a), 21);

    output[0] = 0x67452301 + a;
    output[1] = 0xefcdab89 + b;
    output[2] = 0x98badcfe + c;
    output[3] = 0x10325476 + d;
}
#pragma managed
