using System;
using System.Runtime.CompilerServices;

namespace Helix.Mathematics
{
    /// <summary>The functions of the library over its scalars, vectors, matrices and quaternions (generated ones in Generated/).</summary>
    public static partial class math
    {
        /// <summary>The order of the rotations about the axes of Euler angles.</summary>
        public enum RotationOrder : byte
        {
            XYZ,
            XZY,
            YXZ,
            YZX,
            ZXY,
            ZYX,
            Default = ZXY,
        }

        /// <summary>A component of the left or the right vector of a shuffle.</summary>
        public enum ShuffleComponent : byte
        {
            LeftX,
            LeftY,
            LeftZ,
            LeftW,
            RightX,
            RightY,
            RightZ,
            RightW,
        }

        public const double E_DBL = 2.71828182845904523536;
        public const double LOG2E_DBL = 1.44269504088896340736;
        public const double LOG10E_DBL = 0.434294481903251827651;
        public const double LN2_DBL = 0.693147180559945309417;
        public const double LN10_DBL = 2.30258509299404568402;
        public const double PI_DBL = 3.14159265358979323846;
        public const double PI2_DBL = PI_DBL * 2.0;
        public const double PIHALF_DBL = PI_DBL * 0.5;
        public const double TAU_DBL = PI2_DBL;
        public const double TODEGREES_DBL = 57.29577951308232;
        public const double TORADIANS_DBL = 0.017453292519943296;
        public const double SQRT2_DBL = 1.41421356237309504880;
        public const double EPSILON_DBL = 2.2204460492503131e-16;
        public const double INFINITY_DBL = double.PositiveInfinity;
        public const double NAN_DBL = double.NaN;
        public const double DBL_MIN_NORMAL = 2.2250738585072014e-308;

        public const float E = (float)E_DBL;
        public const float LOG2E = (float)LOG2E_DBL;
        public const float LOG10E = (float)LOG10E_DBL;
        public const float LN2 = (float)LN2_DBL;
        public const float LN10 = (float)LN10_DBL;
        public const float PI = (float)PI_DBL;
        public const float PI2 = (float)PI2_DBL;
        public const float PIHALF = (float)PIHALF_DBL;
        public const float TAU = (float)TAU_DBL;
        public const float TODEGREES = (float)TODEGREES_DBL;
        public const float TORADIANS = (float)TORADIANS_DBL;
        public const float SQRT2 = (float)SQRT2_DBL;
        /// <summary>The difference between 1.0f and the next float (not float.Epsilon).</summary>
        public const float EPSILON = 1.1920928955078125e-7f;
        public const float INFINITY = float.PositiveInfinity;
        public const float NAN = float.NaN;
        public const float FLT_MIN_NORMAL = 1.175494351e-38f;

        // ------------------------------------------------------------------------------------- bits

        static uint ReverseBits32(uint x)
        {
            x = ((x >> 1) & 0x55555555u) | ((x & 0x55555555u) << 1);
            x = ((x >> 2) & 0x33333333u) | ((x & 0x33333333u) << 2);
            x = ((x >> 4) & 0x0F0F0F0Fu) | ((x & 0x0F0F0F0Fu) << 4);
            x = ((x >> 8) & 0x00FF00FFu) | ((x & 0x00FF00FFu) << 8);
            return (x >> 16) | (x << 16);
        }

        static uint CeilPow2(uint x)
        {
            x -= 1;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x + 1;
        }

        // IEEE 754 binary16 to binary32 and back (round to nearest even, overflow to infinity).
        static float HalfBitsToFloat(uint x)
        {
            uint sign = (x & 0x8000u) << 16;
            uint exponent = (x >> 10) & 0x1Fu;
            uint mantissa = x & 0x3FFu;
            uint bits;
            if (exponent == 0)
            {
                if (mantissa == 0)
                    bits = sign;
                else
                {
                    // Subnormal: normalize.
                    exponent = 127 - 15 + 1;
                    while ((mantissa & 0x400u) == 0)
                    {
                        mantissa <<= 1;
                        exponent--;
                    }
                    bits = sign | (exponent << 23) | ((mantissa & 0x3FFu) << 13);
                }
            }
            else if (exponent == 0x1F)
                bits = sign | 0x7F800000u | (mantissa << 13);
            else
                bits = sign | ((exponent + 127 - 15) << 23) | (mantissa << 13);
            return BitConverter.Int32BitsToSingle((int)bits);
        }

        static uint FloatToHalfBits(float f)
        {
            uint x = (uint)BitConverter.SingleToInt32Bits(f);
            uint sign = (x >> 16) & 0x8000u;
            uint exponent = (x >> 23) & 0xFFu;
            uint mantissa = x & 0x7FFFFFu;
            if (exponent == 0xFF)
                return sign | 0x7C00u | (mantissa != 0 ? 0x200u | (mantissa >> 13) : 0u);
            int e = (int)exponent - 127 + 15;
            if (e >= 0x1F)
                return sign | 0x7C00u;
            if (e <= 0)
            {
                if (e < -10)
                    return sign;
                mantissa |= 0x800000u;
                int shift = 14 - e;
                uint half = mantissa >> shift;
                uint rest = mantissa & ((1u << shift) - 1);
                uint halfway = 1u << (shift - 1);
                if (rest > halfway || (rest == halfway && (half & 1u) != 0))
                    half++;
                return sign | half;
            }
            uint result = sign | ((uint)e << 10) | (mantissa >> 13);
            uint remainder = mantissa & 0x1FFFu;
            if (remainder > 0x1000u || (remainder == 0x1000u && (result & 1u) != 0))
                result++;
            return result;
        }

        static uint fold_to_uint(double x)
        {
            ulong bits = (ulong)BitConverter.DoubleToInt64Bits(x);
            return (uint)(bits >> 32) ^ (uint)bits;
        }

        static uint2 fold_to_uint(double2 x) => new uint2(fold_to_uint(x.x), fold_to_uint(x.y));
        static uint3 fold_to_uint(double3 x) => new uint3(fold_to_uint(x.x), fold_to_uint(x.y), fold_to_uint(x.z));
        static uint4 fold_to_uint(double4 x) => new uint4(fold_to_uint(x.x), fold_to_uint(x.y), fold_to_uint(x.z), fold_to_uint(x.w));

        /// <summary>A 32 bit hash of a block of memory (xxHash32).</summary>
        public static unsafe uint hash(void* pBuffer, int numBytes, uint seed = 0)
        {
            const uint P1 = 2654435761u, P2 = 2246822519u, P3 = 3266489917u, P4 = 668265263u, P5 = 374761393u;
            byte* p = (byte*)pBuffer;
            byte* end = p + numBytes;
            uint h;
            if (numBytes >= 16)
            {
                uint v1 = seed + P1 + P2, v2 = seed + P2, v3 = seed, v4 = seed - P1;
                byte* limit = end - 16;
                do
                {
                    v1 = rol(v1 + *(uint*)p * P2, 13) * P1;
                    v2 = rol(v2 + *(uint*)(p + 4) * P2, 13) * P1;
                    v3 = rol(v3 + *(uint*)(p + 8) * P2, 13) * P1;
                    v4 = rol(v4 + *(uint*)(p + 12) * P2, 13) * P1;
                    p += 16;
                } while (p <= limit);
                h = rol(v1, 1) + rol(v2, 7) + rol(v3, 12) + rol(v4, 18);
            }
            else
                h = seed + P5;
            h += (uint)numBytes;
            while (p + 4 <= end)
            {
                h = rol(h + *(uint*)p * P3, 17) * P4;
                p += 4;
            }
            while (p < end)
            {
                h = rol(h + *p * P5, 11) * P1;
                p++;
            }
            h ^= h >> 15;
            h *= P2;
            h ^= h >> 13;
            h *= P3;
            h ^= h >> 16;
            return h;
        }

        public static uint hash(half v) => (uint)v.value * 0x9E3779B1u + 0x7F4A7C15u;
        public static uint hash(quaternion q) => hash(q.value);
        public static uint4 hashwide(quaternion q) => hashwide(q.value);

        // ------------------------------------------------------------------------------------- constructors

        public static half half(half x) => new half(x);
        public static half half(float v) => new half(v);
        public static half half(double v) => new half(v);
        public static quaternion quaternion(float x, float y, float z, float w) => new quaternion(x, y, z, w);
        public static quaternion quaternion(float4 value) => new quaternion(value);
        public static quaternion quaternion(float3x3 m) => new quaternion(m);
        public static quaternion quaternion(float4x4 m) => new quaternion(m);
        public static float3x3 float3x3(quaternion q) => new float3x3(q);
        public static float3x3 float3x3(float4x4 f4x4) => new float3x3(f4x4);
        public static float4x4 float4x4(float3x3 rotation, float3 translation) => new float4x4(rotation, translation);
        public static float4x4 float4x4(quaternion rotation, float3 translation) => new float4x4(rotation, translation);

        // ------------------------------------------------------------------------------------- directions

        public static float3 up() => new float3(0.0f, 1.0f, 0.0f);
        public static float3 down() => new float3(0.0f, -1.0f, 0.0f);
        public static float3 forward() => new float3(0.0f, 0.0f, 1.0f);
        public static float3 back() => new float3(0.0f, 0.0f, -1.0f);
        public static float3 left() => new float3(-1.0f, 0.0f, 0.0f);
        public static float3 right() => new float3(1.0f, 0.0f, 0.0f);

        // ------------------------------------------------------------------------------------- quaternions

        public static quaternion conjugate(quaternion q) => new quaternion(q.value * new float4(-1.0f, -1.0f, -1.0f, 1.0f));

        public static quaternion inverse(quaternion q)
        {
            float4 x = q.value;
            return new quaternion(rcp(dot(x, x)) * x * new float4(-1.0f, -1.0f, -1.0f, 1.0f));
        }

        public static float dot(quaternion a, quaternion b) => dot(a.value, b.value);
        public static float length(quaternion q) => MathF.Sqrt(dot(q.value, q.value));
        public static float lengthsq(quaternion q) => dot(q.value, q.value);
        public static quaternion normalize(quaternion q) => new quaternion(rsqrt(dot(q.value, q.value)) * q.value);

        public static quaternion normalizesafe(quaternion q)
        {
            float len = dot(q.value, q.value);
            return new quaternion(select(global::Helix.Mathematics.quaternion.identity.value, q.value * rsqrt(len), len > FLT_MIN_NORMAL));
        }

        public static quaternion normalizesafe(quaternion q, quaternion defaultvalue)
        {
            float len = dot(q.value, q.value);
            return new quaternion(select(defaultvalue.value, q.value * rsqrt(len), len > FLT_MIN_NORMAL));
        }

        public static quaternion unitexp(quaternion q)
        {
            float rcpLen = rsqrt(dot(q.value.xyz, q.value.xyz));
            float len = rcp(rcpLen);
            sincos(len, out float s, out float c);
            return new quaternion(new float4(q.value.xyz * rcpLen * s, c));
        }

        public static quaternion exp(quaternion q)
        {
            float rcpLen = rsqrt(dot(q.value.xyz, q.value.xyz));
            float len = rcp(rcpLen);
            sincos(len, out float s, out float c);
            return new quaternion(new float4(q.value.xyz * rcpLen * s, c) * MathF.Exp(q.value.w));
        }

        public static quaternion unitlog(quaternion q)
        {
            float w = clamp(q.value.w, -1.0f, 1.0f);
            float s = MathF.Acos(w) * rsqrt(1.0f - w * w);
            return new quaternion(new float4(q.value.xyz * s, 0.0f));
        }

        public static quaternion log(quaternion q)
        {
            float vLenSq = dot(q.value.xyz, q.value.xyz);
            float qLenSq = vLenSq + q.value.w * q.value.w;
            float s = MathF.Acos(clamp(q.value.w * rsqrt(qLenSq), -1.0f, 1.0f)) * rsqrt(vLenSq);
            return new quaternion(new float4(q.value.xyz * s, 0.5f * MathF.Log(qLenSq)));
        }

        /// <summary>The rotation b, then a.</summary>
        public static quaternion mul(quaternion a, quaternion b)
        {
            float4 p = a.value, q = b.value;
            return new quaternion(p.w * q.x + p.x * q.w + p.y * q.z - p.z * q.y, p.w * q.y - p.x * q.z + p.y * q.w + p.z * q.x,
                                  p.w * q.z + p.x * q.y - p.y * q.x + p.z * q.w, p.w * q.w - p.x * q.x - p.y * q.y - p.z * q.z);
        }

        public static float3 mul(quaternion q, float3 v)
        {
            float3 t = 2.0f * cross(q.value.xyz, v);
            return v + q.value.w * t + cross(q.value.xyz, t);
        }

        public static float3 rotate(quaternion q, float3 v) => mul(q, v);

        public static quaternion nlerp(quaternion q1, quaternion q2, float t)
        {
            if (dot(q1, q2) < 0.0f)
                q2.value = -q2.value;
            return normalize(new quaternion(lerp(q1.value, q2.value, t)));
        }

        public static quaternion slerp(quaternion q1, quaternion q2, float t)
        {
            float dt = dot(q1, q2);
            if (dt < 0.0f)
            {
                dt = -dt;
                q2.value = -q2.value;
            }
            if (dt < 0.9995f)
            {
                float angle = MathF.Acos(dt);
                float s = rsqrt(1.0f - dt * dt);
                float w1 = MathF.Sin(angle * (1.0f - t)) * s;
                float w2 = MathF.Sin(angle * t) * s;
                return new quaternion(q1.value * w1 + q2.value * w2);
            }
            return nlerp(q1, q2, t);
        }

        /// <summary>The angle in radians between two unit quaternions.</summary>
        public static float angle(quaternion q1, quaternion q2)
        {
            float diff = MathF.Asin(length(normalize(mul(conjugate(q1), q2)).value.xyz));
            return diff + diff;
        }

        public static float3 forward(quaternion q) => mul(q, new float3(0.0f, 0.0f, 1.0f));

        // The Euler angles of a rotation, in radians: M = R(k) * R(j) * R(i) for the order i, then j, then k.
        static float3 EulerOf(quaternion q, int i, int j, int k)
        {
            float3x3 m = new float3x3(q);
            float At(int row, int col) => m[col][row];
            float parity = (j == (i + 1) % 3) ? 1.0f : -1.0f;
            float sb = clamp(-parity * At(k, i), -1.0f, 1.0f);
            float b = MathF.Asin(sb);
            float a, c;
            if (MathF.Abs(sb) < 0.9999999f)
            {
                a = MathF.Atan2(parity * At(k, j), At(k, k));
                c = MathF.Atan2(parity * At(j, i), At(i, i));
            }
            else
            {
                // Gimbal lock: the last rotation is folded into the first.
                c = 0.0f;
                a = MathF.Atan2(-parity * At(j, k), At(j, j));
            }
            float3 result = default;
            result[i] = a;
            result[j] = b;
            result[k] = c;
            return result;
        }

        public static float3 EulerXYZ(quaternion q) => EulerOf(q, 0, 1, 2);
        public static float3 EulerXZY(quaternion q) => EulerOf(q, 0, 2, 1);
        public static float3 EulerYXZ(quaternion q) => EulerOf(q, 1, 0, 2);
        public static float3 EulerYZX(quaternion q) => EulerOf(q, 1, 2, 0);
        public static float3 EulerZXY(quaternion q) => EulerOf(q, 2, 0, 1);
        public static float3 EulerZYX(quaternion q) => EulerOf(q, 2, 1, 0);

        public static float3 Euler(quaternion q, RotationOrder order = RotationOrder.Default) => order switch
        {
            RotationOrder.XYZ => EulerXYZ(q),
            RotationOrder.XZY => EulerXZY(q),
            RotationOrder.YXZ => EulerYXZ(q),
            RotationOrder.YZX => EulerYZX(q),
            RotationOrder.ZXY => EulerZXY(q),
            RotationOrder.ZYX => EulerZYX(q),
            _ => default(float3),
        };
    }
}
