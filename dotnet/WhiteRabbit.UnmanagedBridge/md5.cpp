#include "stdafx.h"

#include "md5.h"

#include "intrin.h"
#include "immintrin.h"

#pragma unmanaged

#define OP_XOR(a, b) (a ^ b)
#define OP_AND(a, b) (a & b)
#define OP_OR(a, b) (a | b)
#define OP_NEG(a) (~a)
#define OP_ADD(a, b) (a + b)
#define OP_ROT(a, r) (_rotl(a, r))
#define OP_BLEND(a, b, x) (OP_OR(OP_AND(x, b), OP_AND(OP_NEG(x), a)))

typedef unsigned int MD5Vector;

#define CREATE_VECTOR(a) (a)
#define CREATE_VECTOR_FROM_INPUT(input, offset) (input[offset])

typedef struct {
    MD5Vector K[64];
    MD5Vector Init[4];
} MD5Parameters;

static const MD5Parameters Parameters = {
    {
        CREATE_VECTOR(0xd76aa478),
        CREATE_VECTOR(0xe8c7b756),
        CREATE_VECTOR(0x242070db),
        CREATE_VECTOR(0xc1bdceee),
        CREATE_VECTOR(0xf57c0faf),
        CREATE_VECTOR(0x4787c62a),
        CREATE_VECTOR(0xa8304613),
        CREATE_VECTOR(0xfd469501),
        CREATE_VECTOR(0x698098d8),
        CREATE_VECTOR(0x8b44f7af),
        CREATE_VECTOR(0xffff5bb1),
        CREATE_VECTOR(0x895cd7be),
        CREATE_VECTOR(0x6b901122),
        CREATE_VECTOR(0xfd987193),
        CREATE_VECTOR(0xa679438e),
        CREATE_VECTOR(0x49b40821),
        CREATE_VECTOR(0xf61e2562),
        CREATE_VECTOR(0xc040b340),
        CREATE_VECTOR(0x265e5a51),
        CREATE_VECTOR(0xe9b6c7aa),
        CREATE_VECTOR(0xd62f105d),
        CREATE_VECTOR(0x02441453),
        CREATE_VECTOR(0xd8a1e681),
        CREATE_VECTOR(0xe7d3fbc8),
        CREATE_VECTOR(0x21e1cde6),
        CREATE_VECTOR(0xc33707d6),
        CREATE_VECTOR(0xf4d50d87),
        CREATE_VECTOR(0x455a14ed),
        CREATE_VECTOR(0xa9e3e905),
        CREATE_VECTOR(0xfcefa3f8),
        CREATE_VECTOR(0x676f02d9),
        CREATE_VECTOR(0x8d2a4c8a),
        CREATE_VECTOR(0xfffa3942),
        CREATE_VECTOR(0x8771f681),
        CREATE_VECTOR(0x6d9d6122),
        CREATE_VECTOR(0xfde5380c),
        CREATE_VECTOR(0xa4beea44),
        CREATE_VECTOR(0x4bdecfa9),
        CREATE_VECTOR(0xf6bb4b60),
        CREATE_VECTOR(0xbebfbc70),
        CREATE_VECTOR(0x289b7ec6),
        CREATE_VECTOR(0xeaa127fa),
        CREATE_VECTOR(0xd4ef3085),
        CREATE_VECTOR(0x04881d05),
        CREATE_VECTOR(0xd9d4d039),
        CREATE_VECTOR(0xe6db99e5),
        CREATE_VECTOR(0x1fa27cf8),
        CREATE_VECTOR(0xc4ac5665),
        CREATE_VECTOR(0xf4292244),
        CREATE_VECTOR(0x432aff97),
        CREATE_VECTOR(0xab9423a7),
        CREATE_VECTOR(0xfc93a039),
        CREATE_VECTOR(0x655b59c3),
        CREATE_VECTOR(0x8f0ccc92),
        CREATE_VECTOR(0xffeff47d),
        CREATE_VECTOR(0x85845dd1),
        CREATE_VECTOR(0x6fa87e4f),
        CREATE_VECTOR(0xfe2ce6e0),
        CREATE_VECTOR(0xa3014314),
        CREATE_VECTOR(0x4e0811a1),
        CREATE_VECTOR(0xf7537e82),
        CREATE_VECTOR(0xbd3af235),
        CREATE_VECTOR(0x2ad7d2bb),
        CREATE_VECTOR(0xeb86d391),
    },
    {
        CREATE_VECTOR(0x67452301),
        CREATE_VECTOR(0xefcdab89),
        CREATE_VECTOR(0x98badcfe),
        CREATE_VECTOR(0x10325476),
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
inline MD5Vector Step1(MD5Vector a, MD5Vector b, MD5Vector c, MD5Vector d, MD5Vector k, MD5Vector w)
{
    return OP_ADD(b, LeftRotate<r>(OP_ADD(Blend(d, c, b), OP_ADD(k, OP_ADD(a, w)))));
}

template<int r>
inline MD5Vector Step1(MD5Vector a, MD5Vector b, MD5Vector c, MD5Vector d, MD5Vector k)
{
    return OP_ADD(b, LeftRotate<r>(OP_ADD(Blend(d, c, b), OP_ADD(k, a))));
}

template<int r>
inline MD5Vector Step2(MD5Vector a, MD5Vector b, MD5Vector c, MD5Vector d, MD5Vector k, MD5Vector w)
{
    return OP_ADD(c, LeftRotate<r>(OP_ADD(Blend(d, c, b), OP_ADD(k, OP_ADD(a, w)))));
}

template<int r>
inline MD5Vector Step2(MD5Vector a, MD5Vector b, MD5Vector c, MD5Vector d, MD5Vector k)
{
    return OP_ADD(c, LeftRotate<r>(OP_ADD(Blend(d, c, b), OP_ADD(k, a))));
}

template<int r>
inline MD5Vector Step3(MD5Vector a, MD5Vector b, MD5Vector c, MD5Vector d, MD5Vector k, MD5Vector w)
{
    return OP_ADD(b, LeftRotate<r>(OP_ADD(Xor(b, c, d), OP_ADD(k, OP_ADD(a, w)))));
}

template<int r>
inline MD5Vector Step3(MD5Vector a, MD5Vector b, MD5Vector c, MD5Vector d, MD5Vector k)
{
    return OP_ADD(b, LeftRotate<r>(OP_ADD(Xor(b, c, d), OP_ADD(k, a))));
}

template<int r>
inline MD5Vector Step4(MD5Vector a, MD5Vector b, MD5Vector c, MD5Vector d, MD5Vector k, MD5Vector w)
{
    return OP_ADD(b, LeftRotate<r>(OP_ADD(I(c, b, d), OP_ADD(k, OP_ADD(a, w)))));
}

template<int r>
inline MD5Vector Step4(MD5Vector a, MD5Vector b, MD5Vector c, MD5Vector d, MD5Vector k)
{
    return OP_ADD(b, LeftRotate<r>(OP_ADD(I(c, b, d), OP_ADD(k, a))));
}

void md5(unsigned int * input, unsigned int * output)
{
    MD5Vector a = Parameters.Init[0];
    MD5Vector b = Parameters.Init[1];
    MD5Vector c = Parameters.Init[2];
    MD5Vector d = Parameters.Init[3];

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

    output[0] = OP_ADD(Parameters.Init[0], a);
}
#pragma managed
