using System;
using static Helix.Mathematics.math;

namespace Helix.Mathematics
{
    /// <summary>A small random number generator (xorshift32); a seed gives the same sequence on every platform.</summary>
    [Serializable]
    public partial struct Random
    {
        /// <summary>The state; never zero.</summary>
        public uint state;

        public Random(uint seed = 0x6E624EB7u)
        {
            state = seed;
            CheckInitState();
            NextState();
        }

        /// <summary>A generator for an index (such as a job's), from a hash of it.</summary>
        public static Random CreateFromIndex(uint index)
        {
            CheckIndexForHash(index);
            return new Random(WangHash(index + 62u));
        }

        static uint WangHash(uint n)
        {
            n = (n ^ 61u) ^ (n >> 16);
            n *= 9u;
            n = n ^ (n >> 4);
            n *= 0x27d4eb2du;
            n = n ^ (n >> 15);
            return n;
        }

        public void InitState(uint seed = 0x6E624EB7u)
        {
            state = seed;
            NextState();
        }

        uint NextState()
        {
            CheckState();
            uint t = state;
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return t;
        }

        double NextDoubleBits()
        {
            ulong sx = ((ulong)NextState() << 20) ^ NextState();
            return BitConverter.Int64BitsToDouble((long)(0x3ff0000000000000ul | sx)) - 1.0;
        }

        public bool NextBool() => (NextState() & 1u) == 1u;
        public bool2 NextBool2() { uint v = NextState(); return (new uint2(v) & new uint2(1u, 2u)) == 0u; }
        public bool3 NextBool3() { uint v = NextState(); return (new uint3(v) & new uint3(1u, 2u, 4u)) == 0u; }
        public bool4 NextBool4() { uint v = NextState(); return (new uint4(v) & new uint4(1u, 2u, 4u, 8u)) == 0u; }

        /// <summary>A uniformly random int in [-2147483648, 2147483647].</summary>
        public int NextInt() => (int)NextState() ^ -2147483648;
        /// <summary>A uniformly random int in [0, max).</summary>
        public int NextInt(int max) { CheckNextIntMax(max); return (int)((NextState() * (ulong)max) >> 32); }
        /// <summary>A uniformly random int in [min, max).</summary>
        public int NextInt(int min, int max) { CheckNextIntMinMax(min, max); uint range = (uint)(max - min); return (int)(NextState() * (ulong)range >> 32) + min; }
        public uint NextUInt() => NextState() - 1u;
        public uint NextUInt(uint max) => (uint)((NextState() * (ulong)max) >> 32);
        public uint NextUInt(uint min, uint max) { CheckNextUIntMinMax(min, max); uint range = max - min; return (uint)(NextState() * (ulong)range >> 32) + min; }
        /// <summary>A uniformly random float in [0, 1).</summary>
        public float NextFloat() => asfloat(0x3f800000u | (NextState() >> 9)) - 1.0f;
        public float NextFloat(float max) => NextFloat() * max;
        public float NextFloat(float min, float max) => NextFloat() * (max - min) + min;
        /// <summary>A uniformly random double in [0, 1).</summary>
        public double NextDouble() => NextDoubleBits();
        public double NextDouble(double max) => NextDouble() * max;
        public double NextDouble(double min, double max) => NextDouble() * (max - min) + min;

        /// <summary>A uniformly random unit vector.</summary>
        public float2 NextFloat2Direction()
        {
            float angle = NextFloat() * PI * 2.0f;
            sincos(angle, out float s, out float c);
            return new float2(c, s);
        }

        public double2 NextDouble2Direction()
        {
            double angle = NextDouble() * PI_DBL * 2.0;
            sincos(angle, out double s, out double c);
            return new double2(c, s);
        }

        public float3 NextFloat3Direction()
        {
            float2 rnd = NextFloat2();
            float z = rnd.x * 2.0f - 1.0f;
            float r = MathF.Sqrt(MathF.Max(1.0f - z * z, 0.0f));
            float angle = rnd.y * PI * 2.0f;
            sincos(angle, out float s, out float c);
            return new float3(c * r, s * r, z);
        }

        public double3 NextDouble3Direction()
        {
            double2 rnd = NextDouble2();
            double z = rnd.x * 2.0 - 1.0;
            double r = Math.Sqrt(Math.Max(1.0 - z * z, 0.0));
            double angle = rnd.y * PI_DBL * 2.0;
            sincos(angle, out double s, out double c);
            return new double3(c * r, s * r, z);
        }

        /// <summary>A uniformly random rotation.</summary>
        public quaternion NextQuaternionRotation()
        {
            float3 rnd = NextFloat3(new float3(2.0f * PI, 2.0f * PI, 1.0f));
            float u1 = rnd.z;
            float2 thetaRho = rnd.xy;
            float i = MathF.Sqrt(1.0f - u1), j = MathF.Sqrt(u1);
            sincos(thetaRho, out float2 s, out float2 c);
            var q = new quaternion(i * s.x, i * c.x, j * s.y, j * c.y);
            return new quaternion(select(q.value, -q.value, q.value.w < 0.0f));
        }

        void CheckInitState()
        {
            if (state == 0)
                throw new ArgumentException("Seed must be non-zero");
        }

        static void CheckIndexForHash(uint index)
        {
            if (index == uint.MaxValue)
                throw new ArgumentException("Index must not be uint.MaxValue");
        }

        void CheckState()
        {
            if (state == 0)
                throw new ArgumentException("Invalid state 0. Random object has not been properly initialized");
        }

        static void CheckNextIntMax(int max)
        {
            if (max < 0)
                throw new ArgumentException("max must be positive");
        }

        static void CheckNextIntMinMax(int min, int max)
        {
            if (min > max)
                throw new ArgumentException("min must be less than or equal to max");
        }

        static void CheckNextUIntMinMax(uint min, uint max)
        {
            if (min > max)
                throw new ArgumentException("min must be less than or equal to max");
        }
    }
}
