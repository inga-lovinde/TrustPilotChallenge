#include "stdafx.h"
#include "md5.h"

#include "intrin.h"

#pragma unmanaged

#if AVX2
typedef __m256i MD5Vector;

#define OP_XOR(a, b) _mm256_xor_si256(a, b)
#define OP_AND(a, b) _mm256_and_si256(a, b)
#define OP_ANDNOT(a, b) _mm256_andnot_si256(a, b)
#define OP_OR(a, b) _mm256_or_si256(a, b)
#define OP_ADD(a, b) _mm256_add_epi32(a, b)
#define OP_ROT(a, r) OP_OR(_mm256_slli_epi32(a, r), _mm256_srli_epi32(a, 32 - (r)))
#define OP_BLEND(a, b, x) OP_OR(OP_AND(x, b), OP_ANDNOT(x, a))

#define CREATE_VECTOR(a) _mm256_set1_epi32(a)
#define CREATE_VECTOR_FROM_INPUT(input, offset) _mm256_set1_epi32(input[offset])

#define WRITE_TO_OUTPUT(a, output) \
    output[0] = a.m256i_u32[0];
#elif SIMD
typedef __m128i MD5Vector;

#define OP_XOR(a, b) _mm_xor_si128(a, b)
#define OP_AND(a, b) _mm_and_si128(a, b)
#define OP_ANDNOT(a, b) _mm_andnot_si128(a, b)
#define OP_OR(a, b) _mm_or_si128(a, b)
#define OP_ADD(a, b) _mm_add_epi32(a, b)
#define OP_ROT(a, r) OP_OR(_mm_slli_epi32(a, r), _mm_srli_epi32(a, 32 - (r)))
#define OP_BLEND(a, b, x) OP_OR(OP_AND(x, b), OP_ANDNOT(x, a))

#define CREATE_VECTOR(a) _mm_set1_epi32(a)
#define CREATE_VECTOR_FROM_INPUT(input, offset) _mm_set1_epi32(input[offset])

#define WRITE_TO_OUTPUT(a, output) \
    output[0] = a.m128i_u32[0];
#else
typedef unsigned int MD5Vector;

#define OP_XOR(a, b) (a) ^ (b)
#define OP_AND(a, b) (a) & (b)
#define OP_ANDNOT(a, b) ~(a) & (b)
#define OP_OR(a, b) (a) | (b)
#define OP_ADD(a, b) (a) + (b)
#define OP_ROT(a, r) _rotl(a, r)
#define OP_BLEND(a, b, x) ((x) & (b)) | (~(x) & (a))

#define CREATE_VECTOR(a) a
#define CREATE_VECTOR_FROM_INPUT(input, offset) (input[offset])

#define WRITE_TO_OUTPUT(a, output) \
    output[0] = a;
#endif

#define OP_NEG(a) OP_ANDNOT(a, CREATE_VECTOR(0xffffffff))

typedef struct {
    unsigned int K[64];
    unsigned int Init[4];
} MD5Parameters;

static const MD5Parameters Parameters = {
    {
        0xd76aa478,
        0xe8c7b756,
        0x242070db,
        0xc1bdceee,
        0xf57c0faf,
        0x4787c62a,
        0xa8304613,
        0xfd469501,
        0x698098d8,
        0x8b44f7af,
        0xffff5bb1,
        0x895cd7be,
        0x6b901122,
        0xfd987193,
        0xa679438e,
        0x49b40821,
        0xf61e2562,
        0xc040b340,
        0x265e5a51,
        0xe9b6c7aa,
        0xd62f105d,
        0x02441453,
        0xd8a1e681,
        0xe7d3fbc8,
        0x21e1cde6,
        0xc33707d6,
        0xf4d50d87,
        0x455a14ed,
        0xa9e3e905,
        0xfcefa3f8,
        0x676f02d9,
        0x8d2a4c8a,
        0xfffa3942,
        0x8771f681,
        0x6d9d6122,
        0xfde5380c,
        0xa4beea44,
        0x4bdecfa9,
        0xf6bb4b60,
        0xbebfbc70,
        0x289b7ec6,
        0xeaa127fa,
        0xd4ef3085,
        0x04881d05,
        0xd9d4d039,
        0xe6db99e5,
        0x1fa27cf8,
        0xc4ac5665,
        0xf4292244,
        0x432aff97,
        0xab9423a7,
        0xfc93a039,
        0x655b59c3,
        0x8f0ccc92,
        0xffeff47d,
        0x85845dd1,
        0x6fa87e4f,
        0xfe2ce6e0,
        0xa3014314,
        0x4e0811a1,
        0xf7537e82,
        0xbd3af235,
        0x2ad7d2bb,
        0xeb86d391,
    },
    {
        0x67452301,
        0xefcdab89,
        0x98badcfe,
        0x10325476,
    },
};

inline MD5Vector Blend(MD5Vector a, MD5Vector b, MD5Vector x)
{
    return OP_BLEND(a, b, x);
}

inline MD5Vector Xor(MD5Vector a, MD5Vector b, MD5Vector c)
{
    return OP_XOR(a, OP_XOR(b, c));
}

inline MD5Vector I(MD5Vector a, MD5Vector b, MD5Vector c)
{
    return OP_XOR(a, OP_OR(b, OP_NEG(c)));
}

template<int r>
inline MD5Vector LeftRotate(MD5Vector x)
{
    return OP_ROT(x, r);
}

template<int r>
inline MD5Vector Step1(MD5Vector a, MD5Vector b, MD5Vector c, MD5Vector d, unsigned int k, MD5Vector w)
{
    return OP_ADD(b, LeftRotate<r>(OP_ADD(Blend(d, c, b), OP_ADD(CREATE_VECTOR(k), OP_ADD(a, w)))));
}

template<int r>
inline MD5Vector Step1(MD5Vector a, MD5Vector b, MD5Vector c, MD5Vector d, unsigned int k)
{
    return OP_ADD(b, LeftRotate<r>(OP_ADD(Blend(d, c, b), OP_ADD(CREATE_VECTOR(k), a))));
}

template<int r>
inline MD5Vector Step2(MD5Vector a, MD5Vector b, MD5Vector c, MD5Vector d, unsigned int k, MD5Vector w)
{
    return OP_ADD(c, LeftRotate<r>(OP_ADD(Blend(d, c, b), OP_ADD(CREATE_VECTOR(k), OP_ADD(a, w)))));
}

template<int r>
inline MD5Vector Step2(MD5Vector a, MD5Vector b, MD5Vector c, MD5Vector d, unsigned int k)
{
    return OP_ADD(c, LeftRotate<r>(OP_ADD(Blend(d, c, b), OP_ADD(CREATE_VECTOR(k), a))));
}

template<int r>
inline MD5Vector Step3(MD5Vector a, MD5Vector b, MD5Vector c, MD5Vector d, unsigned int k, MD5Vector w)
{
    return OP_ADD(b, LeftRotate<r>(OP_ADD(Xor(b, c, d), OP_ADD(CREATE_VECTOR(k), OP_ADD(a, w)))));
}

template<int r>
inline MD5Vector Step3(MD5Vector a, MD5Vector b, MD5Vector c, MD5Vector d, unsigned int k)
{
    return OP_ADD(b, LeftRotate<r>(OP_ADD(Xor(b, c, d), OP_ADD(CREATE_VECTOR(k), a))));
}

template<int r>
inline MD5Vector Step4(MD5Vector a, MD5Vector b, MD5Vector c, MD5Vector d, unsigned int k, MD5Vector w)
{
    return OP_ADD(b, LeftRotate<r>(OP_ADD(I(c, b, d), OP_ADD(CREATE_VECTOR(k), OP_ADD(a, w)))));
}

template<int r>
inline MD5Vector Step4(MD5Vector a, MD5Vector b, MD5Vector c, MD5Vector d, unsigned int k)
{
    return OP_ADD(b, LeftRotate<r>(OP_ADD(I(c, b, d), OP_ADD(CREATE_VECTOR(k), a))));
}

void md5(unsigned int * input, unsigned int * output)
{
    MD5Vector a = CREATE_VECTOR(Parameters.Init[0]);
    MD5Vector b = CREATE_VECTOR(Parameters.Init[1]);
    MD5Vector c = CREATE_VECTOR(Parameters.Init[2]);
    MD5Vector d = CREATE_VECTOR(Parameters.Init[3]);

    MD5Vector inputVectors[8] = {
        CREATE_VECTOR_FROM_INPUT(input, 0),
        CREATE_VECTOR_FROM_INPUT(input, 1),
        CREATE_VECTOR_FROM_INPUT(input, 2),
        CREATE_VECTOR_FROM_INPUT(input, 3),
        CREATE_VECTOR_FROM_INPUT(input, 4),
        CREATE_VECTOR_FROM_INPUT(input, 5),
        CREATE_VECTOR_FROM_INPUT(input, 6),
        CREATE_VECTOR_FROM_INPUT(input, 7),
    };

    a = Step1< 7>(a, b, c, d, Parameters.K[ 0], inputVectors[0]);
    d = Step1<12>(d, a, b, c, Parameters.K[ 1], inputVectors[1]);
    c = Step1<17>(c, d, a, b, Parameters.K[ 2], inputVectors[2]);
    b = Step1<22>(b, c, d, a, Parameters.K[ 3], inputVectors[3]);
    a = Step1< 7>(a, b, c, d, Parameters.K[ 4], inputVectors[4]);
    d = Step1<12>(d, a, b, c, Parameters.K[ 5], inputVectors[5]);
    c = Step1<17>(c, d, a, b, Parameters.K[ 6], inputVectors[6]);
    b = Step1<22>(b, c, d, a, Parameters.K[ 7]);
    a = Step1< 7>(a, b, c, d, Parameters.K[ 8]);
    d = Step1<12>(d, a, b, c, Parameters.K[ 9]);
    c = Step1<17>(c, d, a, b, Parameters.K[10]);
    b = Step1<22>(b, c, d, a, Parameters.K[11]);
    a = Step1< 7>(a, b, c, d, Parameters.K[12]);
    d = Step1<12>(d, a, b, c, Parameters.K[13]);
    c = Step1<17>(c, d, a, b, Parameters.K[14], inputVectors[7]);
    b = Step1<22>(b, c, d, a, Parameters.K[15]);

    a = Step2< 5>(a, d, b, c, Parameters.K[16], inputVectors[1]);
    d = Step2< 9>(d, c, a, b, Parameters.K[17], inputVectors[6]);
    c = Step2<14>(c, b, d, a, Parameters.K[18]);
    b = Step2<20>(b, a, c, d, Parameters.K[19], inputVectors[0]);
    a = Step2< 5>(a, d, b, c, Parameters.K[20], inputVectors[5]);
    d = Step2< 9>(d, c, a, b, Parameters.K[21]);
    c = Step2<14>(c, b, d, a, Parameters.K[22]);
    b = Step2<20>(b, a, c, d, Parameters.K[23], inputVectors[4]);
    a = Step2< 5>(a, d, b, c, Parameters.K[24]);
    d = Step2< 9>(d, c, a, b, Parameters.K[25], inputVectors[7]);
    c = Step2<14>(c, b, d, a, Parameters.K[26], inputVectors[3]);
    b = Step2<20>(b, a, c, d, Parameters.K[27]);
    a = Step2< 5>(a, d, b, c, Parameters.K[28]);
    d = Step2< 9>(d, c, a, b, Parameters.K[29], inputVectors[2]);
    c = Step2<14>(c, b, d, a, Parameters.K[30]);
    b = Step2<20>(b, a, c, d, Parameters.K[31]);

    a = Step3< 4>(a, b, c, d, Parameters.K[32], inputVectors[5]);
    d = Step3<11>(d, a, b, c, Parameters.K[33]);
    c = Step3<16>(c, d, a, b, Parameters.K[34]);
    b = Step3<23>(b, c, d, a, Parameters.K[35], inputVectors[7]);
    a = Step3< 4>(a, b, c, d, Parameters.K[36], inputVectors[1]);
    d = Step3<11>(d, a, b, c, Parameters.K[37], inputVectors[4]);
    c = Step3<16>(c, d, a, b, Parameters.K[38]);
    b = Step3<23>(b, c, d, a, Parameters.K[39]);
    a = Step3< 4>(a, b, c, d, Parameters.K[40]);
    d = Step3<11>(d, a, b, c, Parameters.K[41], inputVectors[0]);
    c = Step3<16>(c, d, a, b, Parameters.K[42], inputVectors[3]);
    b = Step3<23>(b, c, d, a, Parameters.K[43], inputVectors[6]);
    a = Step3< 4>(a, b, c, d, Parameters.K[44]);
    d = Step3<11>(d, a, b, c, Parameters.K[45]);
    c = Step3<16>(c, d, a, b, Parameters.K[46]);
    b = Step3<23>(b, c, d, a, Parameters.K[47], inputVectors[2]);

    a = Step4< 6>(a, b, c, d, Parameters.K[48], inputVectors[0]);
    d = Step4<10>(d, a, b, c, Parameters.K[49]);
    c = Step4<15>(c, d, a, b, Parameters.K[50], inputVectors[7]);
    b = Step4<21>(b, c, d, a, Parameters.K[51], inputVectors[5]);
    a = Step4< 6>(a, b, c, d, Parameters.K[52]);
    d = Step4<10>(d, a, b, c, Parameters.K[53], inputVectors[3]);
    c = Step4<15>(c, d, a, b, Parameters.K[54]);
    b = Step4<21>(b, c, d, a, Parameters.K[55], inputVectors[1]);
    a = Step4< 6>(a, b, c, d, Parameters.K[56]);
    d = Step4<10>(d, a, b, c, Parameters.K[57]);
    c = Step4<15>(c, d, a, b, Parameters.K[58], inputVectors[6]);
    b = Step4<21>(b, c, d, a, Parameters.K[59]);
    a = Step4< 6>(a, b, c, d, Parameters.K[60], inputVectors[4]);

    a = OP_ADD(CREATE_VECTOR(Parameters.Init[0]), a);

    WRITE_TO_OUTPUT(a, output);
}
#pragma managed
