#include "stdafx.h"
#include "md5.h"

#include "intrin.h"

#pragma unmanaged

struct MD5Vector
{
    __m256i m_V0;
    __m256i m_V1;
    __forceinline MD5Vector() {}
    __forceinline MD5Vector(__m256i C0, __m256i C1) :m_V0(C0), m_V1(C1) {}

    __forceinline MD5Vector MXor(MD5Vector R) const
    {
        return MD5Vector(_mm256_xor_si256(m_V0, R.m_V0), _mm256_xor_si256(m_V1, R.m_V1));
    }

    __forceinline MD5Vector MAnd(MD5Vector R) const
    {
        return MD5Vector(_mm256_and_si256(m_V0, R.m_V0), _mm256_and_si256(m_V1, R.m_V1));
    }

    __forceinline MD5Vector MAndNot(MD5Vector R) const
    {
        return MD5Vector(_mm256_andnot_si256(m_V0, R.m_V0), _mm256_andnot_si256(m_V1, R.m_V1));
    }

    __forceinline const MD5Vector MOr(const MD5Vector R) const
    {
        return MD5Vector(_mm256_or_si256(m_V0, R.m_V0), _mm256_or_si256(m_V1, R.m_V1));
    }

    __forceinline const MD5Vector MAdd(const MD5Vector R) const
    {
        return MD5Vector(_mm256_add_epi32(m_V0, R.m_V0), _mm256_add_epi32(m_V1, R.m_V1));
    }

    __forceinline const MD5Vector MShiftLeft(const int shift) const
    {
        return MD5Vector(_mm256_slli_epi32(m_V0, shift), _mm256_slli_epi32(m_V1, shift));
    }

    __forceinline const MD5Vector MShiftRight(const int shift) const
    {
        return MD5Vector(_mm256_srli_epi32(m_V0, shift), _mm256_srli_epi32(m_V1, shift));
    }

    template<int imm8>
    __forceinline const MD5Vector Permute() const
    {
        return MD5Vector(_mm256_permute4x64_epi64(m_V0, imm8), _mm256_permute4x64_epi64(m_V1, imm8));
    }

    __forceinline const MD5Vector CompareEquality32(const __m256i other) const
    {
        return MD5Vector(_mm256_cmpeq_epi32(m_V0, other), _mm256_cmpeq_epi32(m_V1, other));
    }

    __forceinline void WriteMoveMask8(__int32 * output) const
    {
        output[0] = _mm256_movemask_epi8(m_V0);
        output[1] = _mm256_movemask_epi8(m_V1);
    }
};

__forceinline const MD5Vector OP_XOR(const MD5Vector a, const MD5Vector b) { return a.MXor(b); }
__forceinline const MD5Vector OP_AND(const MD5Vector a, const MD5Vector b) { return a.MAnd(b); }
__forceinline const MD5Vector OP_ANDNOT(const MD5Vector a, const MD5Vector b) { return a.MAndNot(b); }
__forceinline const MD5Vector OP_OR(const MD5Vector a, const MD5Vector b) { return a.MOr(b); }
__forceinline const MD5Vector OP_ADD(const MD5Vector a, const MD5Vector b) { return a.MAdd(b); }
template<int r>
__forceinline const MD5Vector OP_ROT(const MD5Vector a) { return OP_OR(a.MShiftLeft(r), a.MShiftRight(32 - (r))); }
__forceinline const MD5Vector OP_BLEND(const MD5Vector a, const MD5Vector b, const MD5Vector x) { return OP_OR(OP_AND(x, b), OP_ANDNOT(x, a)); }

__forceinline const MD5Vector CREATE_VECTOR(const int a) { return MD5Vector(_mm256_set1_epi32(a), _mm256_set1_epi32(a)); }
__forceinline const MD5Vector CREATE_VECTOR_FROM_INPUT(const unsigned __int32* input, const size_t offset)
{
    return MD5Vector(
        _mm256_set_epi32(
            input[offset + 7 * 8],
            input[offset + 6 * 8],
            input[offset + 5 * 8],
            input[offset + 4 * 8],
            input[offset + 3 * 8],
            input[offset + 2 * 8],
            input[offset + 1 * 8],
            input[offset + 0 * 8]),
        _mm256_set_epi32(
            input[offset + 15 * 8],
            input[offset + 14 * 8],
            input[offset + 13 * 8],
            input[offset + 12 * 8],
            input[offset + 11 * 8],
            input[offset + 10 * 8],
            input[offset + 9 * 8],
            input[offset + 8 * 8]));
}

#define WRITE_TO_OUTPUT(a, output, expected) \
    a.Permute<0 * 0x55>().CompareEquality32(*expected).WriteMoveMask8(output); \
    a.Permute<1 * 0x55>().CompareEquality32(*expected).WriteMoveMask8(output + 2); \
    a.Permute<2 * 0x55>().CompareEquality32(*expected).WriteMoveMask8(output + 4); \
    a.Permute<3 * 0x55>().CompareEquality32(*expected).WriteMoveMask8(output + 6); \
    output[8] = _mm256_movemask_epi8(_mm256_cmpeq_epi8(*((__m256i*)output), _mm256_setzero_si256()));

__forceinline void WriteToOutput(const MD5Vector a, __int32 * output, __m256i * expected)
{
    a.Permute<0 * 0x55>().CompareEquality32(*expected).WriteMoveMask8(output);
    a.Permute<1 * 0x55>().CompareEquality32(*expected).WriteMoveMask8(output);
    a.Permute<2 * 0x55>().CompareEquality32(*expected).WriteMoveMask8(output);
    a.Permute<3 * 0x55>().CompareEquality32(*expected).WriteMoveMask8(output);
    output[8] = _mm256_movemask_epi8(_mm256_cmpeq_epi8(*((__m256i*)output), _mm256_setzero_si256()));
}

const MD5Vector Ones = CREATE_VECTOR(0xffffffff);
__forceinline const MD5Vector OP_NEG(const MD5Vector a) { return OP_ANDNOT(a, Ones); }

__forceinline const MD5Vector Blend(const MD5Vector a, const MD5Vector b, const MD5Vector x) { return OP_BLEND(a, b, x); }
__forceinline const MD5Vector Xor(const MD5Vector a, const MD5Vector b, const MD5Vector c) { return OP_XOR(a, OP_XOR(b, c)); }
__forceinline const MD5Vector I(const MD5Vector a, const MD5Vector b, const MD5Vector c) { return OP_XOR(a, OP_OR(b, OP_NEG(c))); }

template<int r>
__forceinline const MD5Vector StepOuter(const MD5Vector a, const MD5Vector b, const MD5Vector x) { return OP_ADD(b, OP_ROT<r>(x)); }

template<int r, unsigned __int32 k>
__forceinline const MD5Vector Step1(const MD5Vector a, const MD5Vector b, const MD5Vector c, const MD5Vector d, const MD5Vector w) {
    return StepOuter<r>(a, b, OP_ADD(Blend(d, c, b), OP_ADD(CREATE_VECTOR(k), OP_ADD(a, w))));
}

template<int r, unsigned __int32 k>
__forceinline const MD5Vector Step1(const MD5Vector a, const MD5Vector b, const MD5Vector c, const MD5Vector d) {
    return StepOuter<r>(a, b, OP_ADD(Blend(d, c, b), OP_ADD(CREATE_VECTOR(k), a)));
}

template<int r, unsigned __int32 k>
__forceinline const MD5Vector Step2(const MD5Vector a, const MD5Vector b, const MD5Vector c, const MD5Vector d, const MD5Vector w) {
    return StepOuter<r>(a, c, OP_ADD(Blend(d, c, b), OP_ADD(CREATE_VECTOR(k), OP_ADD(a, w))));
}

template<int r, unsigned __int32 k>
__forceinline const MD5Vector Step2(const MD5Vector a, const MD5Vector b, const MD5Vector c, const MD5Vector d) {
    return StepOuter<r>(a, c, OP_ADD(Blend(d, c, b), OP_ADD(CREATE_VECTOR(k), a)));
}

template<int r, unsigned __int32 k>
__forceinline const MD5Vector Step3(const MD5Vector a, const MD5Vector b, const MD5Vector c, const MD5Vector d, const MD5Vector w) {
    return StepOuter<r>(a, b, OP_ADD(Xor(b, c, d), OP_ADD(CREATE_VECTOR(k), OP_ADD(a, w))));
}

template<int r, unsigned __int32 k>
__forceinline const MD5Vector Step3(const MD5Vector a, const MD5Vector b, const MD5Vector c, const MD5Vector d) {
    return StepOuter<r>(a, b, OP_ADD(Xor(b, c, d), OP_ADD(CREATE_VECTOR(k), a)));
}

template<int r, unsigned __int32 k>
__forceinline const MD5Vector Step4(const MD5Vector a, const MD5Vector b, const MD5Vector c, const MD5Vector d, const MD5Vector w) {
    return StepOuter<r>(a, b, OP_ADD(I(c, b, d), OP_ADD(CREATE_VECTOR(k), OP_ADD(a, w))));
}

template<int r, unsigned __int32 k>
__forceinline const MD5Vector Step4(const MD5Vector a, const MD5Vector b, const MD5Vector c, const MD5Vector d) {
    return StepOuter<r>(a, b, OP_ADD(I(c, b, d), OP_ADD(CREATE_VECTOR(k), a)));
}

void md5(unsigned __int32 * input, unsigned __int32 * expected)
{
    MD5Vector a = CREATE_VECTOR(0x67452301);
    MD5Vector b = CREATE_VECTOR(0xefcdab89);
    MD5Vector c = CREATE_VECTOR(0x98badcfe);
    MD5Vector d = CREATE_VECTOR(0x10325476);

    MD5Vector inputVector0 = CREATE_VECTOR_FROM_INPUT(input, 0);
    MD5Vector inputVector1 = CREATE_VECTOR_FROM_INPUT(input, 1);
    MD5Vector inputVector2 = CREATE_VECTOR_FROM_INPUT(input, 2);
    MD5Vector inputVector3 = CREATE_VECTOR_FROM_INPUT(input, 3);
    MD5Vector inputVector4 = CREATE_VECTOR_FROM_INPUT(input, 4);
    MD5Vector inputVector5 = CREATE_VECTOR_FROM_INPUT(input, 5);
    MD5Vector inputVector6 = CREATE_VECTOR_FROM_INPUT(input, 6);
    MD5Vector inputVector7 = CREATE_VECTOR_FROM_INPUT(input, 7);

    a = Step1< 7, 0xd76aa478>(a, b, c, d, inputVector0);
    d = Step1<12, 0xe8c7b756>(d, a, b, c, inputVector1);
    c = Step1<17, 0x242070db>(c, d, a, b, inputVector2);
    b = Step1<22, 0xc1bdceee>(b, c, d, a, inputVector3);
    a = Step1< 7, 0xf57c0faf>(a, b, c, d, inputVector4);
    d = Step1<12, 0x4787c62a>(d, a, b, c, inputVector5);
    c = Step1<17, 0xa8304613>(c, d, a, b, inputVector6);
    b = Step1<22, 0xfd469501>(b, c, d, a);
    a = Step1< 7, 0x698098d8>(a, b, c, d);
    d = Step1<12, 0x8b44f7af>(d, a, b, c);
    c = Step1<17, 0xffff5bb1>(c, d, a, b);
    b = Step1<22, 0x895cd7be>(b, c, d, a);
    a = Step1< 7, 0x6b901122>(a, b, c, d);
    d = Step1<12, 0xfd987193>(d, a, b, c);
    c = Step1<17, 0xa679438e>(c, d, a, b, inputVector7);
    b = Step1<22, 0x49b40821>(b, c, d, a);

    a = Step2< 5, 0xf61e2562>(a, d, b, c, inputVector1);
    d = Step2< 9, 0xc040b340>(d, c, a, b, inputVector6);
    c = Step2<14, 0x265e5a51>(c, b, d, a);
    b = Step2<20, 0xe9b6c7aa>(b, a, c, d, inputVector0);
    a = Step2< 5, 0xd62f105d>(a, d, b, c, inputVector5);
    d = Step2< 9, 0x02441453>(d, c, a, b);
    c = Step2<14, 0xd8a1e681>(c, b, d, a);
    b = Step2<20, 0xe7d3fbc8>(b, a, c, d, inputVector4);
    a = Step2< 5, 0x21e1cde6>(a, d, b, c);
    d = Step2< 9, 0xc33707d6>(d, c, a, b, inputVector7);
    c = Step2<14, 0xf4d50d87>(c, b, d, a, inputVector3);
    b = Step2<20, 0x455a14ed>(b, a, c, d);
    a = Step2< 5, 0xa9e3e905>(a, d, b, c);
    d = Step2< 9, 0xfcefa3f8>(d, c, a, b, inputVector2);
    c = Step2<14, 0x676f02d9>(c, b, d, a);
    b = Step2<20, 0x8d2a4c8a>(b, a, c, d);

    a = Step3< 4, 0xfffa3942>(a, b, c, d, inputVector5);
    d = Step3<11, 0x8771f681>(d, a, b, c);
    c = Step3<16, 0x6d9d6122>(c, d, a, b);
    b = Step3<23, 0xfde5380c>(b, c, d, a, inputVector7);
    a = Step3< 4, 0xa4beea44>(a, b, c, d, inputVector1);
    d = Step3<11, 0x4bdecfa9>(d, a, b, c, inputVector4);
    c = Step3<16, 0xf6bb4b60>(c, d, a, b);
    b = Step3<23, 0xbebfbc70>(b, c, d, a);
    a = Step3< 4, 0x289b7ec6>(a, b, c, d);
    d = Step3<11, 0xeaa127fa>(d, a, b, c, inputVector0);
    c = Step3<16, 0xd4ef3085>(c, d, a, b, inputVector3);
    b = Step3<23, 0x04881d05>(b, c, d, a, inputVector6);
    a = Step3< 4, 0xd9d4d039>(a, b, c, d);
    d = Step3<11, 0xe6db99e5>(d, a, b, c);
    c = Step3<16, 0x1fa27cf8>(c, d, a, b);
    b = Step3<23, 0xc4ac5665>(b, c, d, a, inputVector2);

    a = Step4< 6, 0xf4292244>(a, b, c, d, inputVector0);
    d = Step4<10, 0x432aff97>(d, a, b, c);
    c = Step4<15, 0xab9423a7>(c, d, a, b, inputVector7);
    b = Step4<21, 0xfc93a039>(b, c, d, a, inputVector5);
    a = Step4< 6, 0x655b59c3>(a, b, c, d);
    d = Step4<10, 0x8f0ccc92>(d, a, b, c, inputVector3);
    c = Step4<15, 0xffeff47d>(c, d, a, b);
    b = Step4<21, 0x85845dd1>(b, c, d, a, inputVector1);
    a = Step4< 6, 0x6fa87e4f>(a, b, c, d);
    d = Step4<10, 0xfe2ce6e0>(d, a, b, c);
    c = Step4<15, 0xa3014314>(c, d, a, b, inputVector6);
    b = Step4<21, 0x4e0811a1>(b, c, d, a);
    a = Step4< 6, 0xf7537e82>(a, b, c, d, inputVector4);

    a = OP_ADD(CREATE_VECTOR(0x67452301), a);

    WRITE_TO_OUTPUT(a, ((__int32*)input), ((__m256i*)expected));
}
#pragma managed
