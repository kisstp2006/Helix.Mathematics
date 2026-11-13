using System;
using System.Text;
using Helix.Mathematics;
using static Helix.Mathematics.math;

namespace Helix.Mathematics.Tests
{
    static class Program
    {
        static int s_Failed;
        static int s_Checks;

        static void Check(bool condition, string what)
        {
            s_Checks++;
            if (condition)
                return;
            s_Failed++;
            Console.WriteLine("FAILED: " + what);
        }

        static bool Near(float a, float b, float tolerance = 1e-5f) => MathF.Abs(a - b) <= tolerance * MathF.Max(1.0f, MathF.Abs(b));
        static bool Near(double a, double b, double tolerance = 1e-12) => Math.Abs(a - b) <= tolerance * Math.Max(1.0, Math.Abs(b));
        static bool Near(float2 a, float2 b, float tolerance = 1e-5f) => all(abs(a - b) <= tolerance);
        static bool Near(float3 a, float3 b, float tolerance = 1e-5f) => all(abs(a - b) <= tolerance);
        static bool Near(float4 a, float4 b, float tolerance = 1e-5f) => all(abs(a - b) <= tolerance);
        static bool Near(float3x3 a, float3x3 b, float tolerance = 1e-5f) => Near(a.c0, b.c0, tolerance) && Near(a.c1, b.c1, tolerance) && Near(a.c2, b.c2, tolerance);
        static bool Near(float4x4 a, float4x4 b, float tolerance = 1e-5f) =>
            Near(a.c0, b.c0, tolerance) && Near(a.c1, b.c1, tolerance) && Near(a.c2, b.c2, tolerance) && Near(a.c3, b.c3, tolerance);
        static bool SameRotation(quaternion a, quaternion b) => Near(abs(dot(a, b)), 1.0f, 1e-5f);

        static int Main()
        {
            Types();
            Operators();
            Functions();
            Bits();
            Halves();
            Hashes();
            Matrices();
            Quaternions();
            Projections();
            RandomNumbers();
            Console.WriteLine($"{s_Checks - s_Failed} of {s_Checks} checks passed");
            return s_Failed == 0 ? 0 : 1;
        }

        static void Types()
        {
            var v = new float4(new float2(1, 2), 3, 4);
            Check(v.x == 1 && v.y == 2 && v.z == 3 && v.w == 4, "float4(float2, float, float)");
            Check(v.wzyx.Equals(new float4(4, 3, 2, 1)) && v.xxyy.Equals(new float4(1, 1, 2, 2)), "swizzle getters");
            v.zx = new float2(9, 8);
            Check(v.Equals(new float4(8, 2, 9, 4)), "a swizzle setter");
            Check(v[2] == 9 && new int3(5, 6, 7)[1] == 6, "indexers");
            bool threw = false;
            try
            {
                _ = v[4];
            }
            catch (ArgumentException)
            {
                threw = true;
            }
            Check(threw, "an index out of range throws");
            Check(new float3(1, 2, 3).ToString() == "float3(1f, 2f, 3f)", "float3.ToString: " + new float3(1, 2, 3));
            Check(new int2(-1, 2).ToString() == "int2(-1, 2)" && new uint2(1, 2).ToString() == "uint2(1, 2)", "int2 and uint2 ToString");
            Check(new double2(0.5, 2).ToString() == "double2(0.5, 2)" && new bool2(true, false).ToString() == "bool2(True, False)", "double2 and bool2 ToString");
            Check(new float2x2(1, 2, 3, 4).ToString() == "float2x2(1f, 2f,  3f, 4f)", "float2x2 ToString is row major: " + new float2x2(1, 2, 3, 4));
            Check(new float3(1.25f, 2, 3).ToString("F1", null) == "float3(1.3f, 2.0f, 3.0f)" || new float3(1.25f, 2, 3).ToString("F1", null) == "float3(1.2f, 2.0f, 3.0f)",
                  "formatted ToString");
            float3 fromInt = new int3(1, 2, 3);
            Check(fromInt.Equals(new float3(1, 2, 3)), "int3 converts to float3 implicitly");
            Check(((int3)new float3(1.9f, -1.9f, 0)).Equals(new int3(1, -1, 0)), "float3 to int3 truncates");
            Check(new float2(true).Equals(new float2(1, 1)) && new int2(new bool2(true, false)).Equals(new int2(1, 0)), "bools convert to 1 and 0");
            Check(float3x3.identity.c1.Equals(new float3(0, 1, 0)) && float4.zero.Equals(new float4(0)), "identity and zero");
            Check(new float3x3(1, 2, 3, 4, 5, 6, 7, 8, 9).c0.Equals(new float3(1, 4, 7)), "matrix element constructors are row major");
            Check(float3(1, 2, 3).Equals(new float3(1, 2, 3)) && int2x2(1).c1.Equals(new int2(1, 1)), "the constructor functions");
        }

        static void Operators()
        {
            var a = new float3(1, 2, 3);
            var b = new float3(4, 5, 6);
            Check((a + b).Equals(new float3(5, 7, 9)) && (a * 2).Equals(new float3(2, 4, 6)) && (6 / b).Equals(new float3(1.5f, 1.2f, 1)), "arithmetic");
            Check(all((a < b) == new bool3(true)) && any(a == 2) && !any(a > 5), "comparisons give bool vectors");
            Check(((new int3(1, 2, 3) << 2) | 1).Equals(new int3(5, 9, 13)) && (~new uint2(0, 1)).Equals(new uint2(0xFFFFFFFF, 0xFFFFFFFE)), "integer bit operators");
            Check((!new bool2(true, false)).Equals(new bool2(false, true)) && (new bool2(true, false) ^ true).Equals(new bool2(false, true)), "bool operators");
            var m = new float2x2(1, 2, 3, 4);
            Check((m * m).Equals(new float2x2(1, 4, 9, 16)), "matrix * is componentwise");
            float3 c = a;
            c++;
            Check(c.Equals(new float3(2, 3, 4)) && (-a).Equals(new float3(-1, -2, -3)) && (new float3(5, 7, 9) % 4).Equals(new float3(1, 3, 1)), "++, unary - and %");
        }

        static void Functions()
        {
            Check(dot(new float3(1, 2, 3), new float3(4, 5, 6)) == 32 && cross(new float3(1, 0, 0), new float3(0, 1, 0)).Equals(new float3(0, 0, 1)), "dot and cross");
            Check(Near(length(new float3(3, 4, 0)), 5) && Near(normalize(new float2(3, 4)), new float2(0.6f, 0.8f)), "length and normalize");
            Check(normalizesafe(new float3(0)).Equals(new float3(0)) && normalizesafe(new float3(0), new float3(1, 0, 0)).Equals(new float3(1, 0, 0)), "normalizesafe");
            Check(lerp(2.0f, 4.0f, 0.25f) == 2.5f && unlerp(2.0f, 4.0f, 3.0f) == 0.5f && remap(0.0f, 10.0f, 0.0f, 100.0f, 2.5f) == 25.0f, "lerp, unlerp, remap");
            Check(clamp(5, 0, 3) == 3 && clamp(new float2(-1, 2), 0, 1).Equals(new float2(0, 1)) && saturate(1.5f) == 1, "clamp and saturate");
            Check(smoothstep(0.0f, 1.0f, 0.5f) == 0.5f && step(1.0f, 2.0f) == 1 && step(3.0f, 2.0f) == 0, "smoothstep and step");
            Check(select(1, 2, true) == 2 && select(new int2(1, 2), new int2(3, 4), new bool2(true, false)).Equals(new int2(3, 2)), "select");
            Check(cmin(new int4(4, 2, 8, 6)) == 2 && cmax(new float3(1, 9, 3)) == 9 && csum(new uint3(1, 2, 3)) == 6, "cmin, cmax, csum");
            Check(Near(degrees(PI), 180.0f) && Near(radians(90.0f), PIHALF) && Near(TAU, PI2), "angles");
            Check(Near(reflect(new float2(1, -1), new float2(0, 1)), new float2(1, 1)) && sign(-3) == -1 && sign(0.0f) == 0, "reflect and sign");
            Check(isnan(NAN) && isinf(INFINITY) && !isfinite(INFINITY) && isfinite(1.0f) && all(isnan(new double2(NAN_DBL))), "isnan, isinf, isfinite");
            Check(frac(1.25f) == 0.25f && Near(modf(2.75f, out float whole), 0.75f) && whole == 2.0f, "frac and modf");
            Check(shuffle(new float4(1, 2, 3, 4), new float4(5, 6, 7, 8), ShuffleComponent.LeftW, ShuffleComponent.RightX).Equals(new float2(4, 5)), "shuffle");
            Check(bitmask(new bool4(true, false, true, true)) == 13, "bitmask");
            Check(Near(project(new float2(2, 2), new float2(1, 0)).x, 2) && all(isnan(project(new float2(1, 1), new float2(0)))) &&
                  projectsafe(new float2(1, 1), new float2(0), new float2(7)).Equals(new float2(7)), "project and projectsafe");
            orthonormal_basis(normalize(new float3(1, 2, 3)), out float3 b1, out float3 b2);
            Check(Near(dot(b1, b2), 0) && Near(length(b1), 1) && Near(dot(b1, normalize(new float3(1, 2, 3))), 0), "orthonormal_basis");
            unsafe
            {
                int* output = stackalloc int[4];
                Check(compress(output, 0, new int4(1, 2, 3, 4), new bool4(false, true, false, true)) == 2 && output[0] == 2 && output[1] == 4, "compress");
            }
        }

        static void Bits()
        {
            Check(countbits(0xF0u) == 4 && lzcnt(1) == 31 && tzcnt(8) == 3 && tzcnt(0) == 32 && lzcnt(0L) == 64, "bit counts");
            Check(reversebits(1u) == 0x80000000u && reversebits(1L) == long.MinValue, "reversebits");
            Check(rol(0x80000001u, 1) == 3u && ror(3u, 1) == 0x80000001u, "rol and ror");
            Check(ceilpow2(5) == 8 && ceilpow2(8u) == 8 && ceillog2(5) == 3 && floorlog2(5) == 2 && ispow2(64) && !ispow2(0), "powers of two");
            Check(asint(1.0f) == 0x3F800000 && asfloat(0x40000000) == 2.0f && asuint(-1) == 0xFFFFFFFFu && asdouble(aslong(1.5)) == 1.5, "reinterpretation");
            Check(chgsign(2.0f, -1.0f) == -2.0f && chgsign(-2.0f, -1.0f) == 2.0f, "chgsign");
        }

        static void Halves()
        {
            Check(f32tof16(1.0f) == 0x3C00 && f32tof16(-2.0f) == 0xC000 && f32tof16(0.1f) == 0x2E66 && f32tof16(65504.0f) == 0x7BFF, "f32tof16");
            Check(f32tof16(1e6f) == 0x7C00 && f32tof16(5.960464477539063e-8f) == 0x0001, "f32tof16 overflow and subnormals");
            Check(f16tof32(0x3C00) == 1.0f && f16tof32(0x0001) == 5.960464477539063e-8f && float.IsNaN(f16tof32(0x7E00)), "f16tof32");
            var h = new half(3.5f);
            float back = h;
            Check(back == 3.5f && half.MaxValue == 65504.0f && new half4(new float4(1, 2, 3, 4)).w == new half(4.0f), "half conversions");
            Check(new half2(new half(1.0f), new half(2.0f)).ToString() == "half2(1h, 2h)", "half2 ToString");
        }

        static unsafe void Hashes()
        {
            byte[] abc = Encoding.ASCII.GetBytes("abc");
            fixed (byte* p = abc)
                Check(hash(p, 3) == 0x32D153FFu, "xxHash32 of abc");
            Check(hash(null, 0) == 0x02CC5D05u, "xxHash32 of nothing");
            byte[] longer = Encoding.ASCII.GetBytes("Nobody inspects the spammish repetition");
            fixed (byte* p = longer)
                Check(hash(p, longer.Length) == 0xE2293B2Fu, "xxHash32 of 39 bytes");
            Check(hash(new float3(1, 2, 3)) == hash(new float3(1, 2, 3)) && hash(new float3(1, 2, 3)) != hash(new float3(3, 2, 1)), "vector hashes");
            Check(new int2(1, 2).GetHashCode() == new int2(1, 2).GetHashCode(), "GetHashCode");
        }

        static void Matrices()
        {
            var m = new float3x3(2, 0, 1, 1, 3, 0, 0, 1, 4);
            Check(Near(mul(inverse(m), m), float3x3.identity), "float3x3 inverse");
            Check(Near(determinant(m), 25.0f), "float3x3 determinant");
            var m4 = new float4x4(2, 0, 1, 3, 1, 3, 0, 1, 0, 1, 4, 2, 1, 0, 0, 1);
            Check(Near(mul(m4, inverse(m4)), float4x4.identity, 1e-4f) && Near(mul(inverse(m4), m4), float4x4.identity, 1e-4f), "float4x4 inverse");
            Check(Near(determinant(m4), determinant(transpose(m4))), "determinant of the transpose");
            var m2 = new double2x2(4, 7, 2, 6);
            var i2 = mul(inverse(m2), m2);
            Check(Near(i2.c0.x, 1.0) && Near(i2.c1.x, 0.0) && Near(determinant(m2), 10.0), "double2x2 inverse and determinant");
            Check(transpose(new int2x3(1, 2, 3, 4, 5, 6)).Equals(new int3x2(1, 4, 2, 5, 3, 6)), "transpose");
            Check(mul(new float2x2(1, 2, 3, 4), new float2(1, 1)).Equals(new float2(3, 7)) && mul(new float2(1, 1), new float2x2(1, 2, 3, 4)).Equals(new float2(4, 6)),
                  "matrix times vector and vector times matrix");
            var rt = new float4x4(float3x3.RotateY(0.7f), new float3(1, 2, 3));
            Check(Near(fastinverse(rt), inverse(rt), 1e-5f), "fastinverse");
            Check(Near(transform(rt, new float3(0)), new float3(1, 2, 3)) && Near(rotate(rt, new float3(0, 1, 0)), new float3(0, 1, 0)), "transform and rotate");
            Check(determinant(new int3x3(1, 2, 3, 4, 5, 6, 7, 8, 10)) == -3, "int3x3 determinant");
        }

        static void Quaternions()
        {
            float3 axis = normalize(new float3(1, 2, 3));
            quaternion q = quaternion.AxisAngle(axis, 1.2f);
            float3 v = new float3(0.3f, -1, 2);
            Check(Near(mul(q, v), mul(float3x3.AxisAngle(axis, 1.2f), v)) && Near(mul(new float3x3(q), v), mul(q, v)), "AxisAngle as matrix and quaternion");
            Check(SameRotation(new quaternion(new float3x3(q)), q), "quaternion from a matrix");
            float3 angles = new float3(0.3f, -0.6f, 1.1f);
            foreach (RotationOrder order in new[] { RotationOrder.XYZ, RotationOrder.XZY, RotationOrder.YXZ, RotationOrder.YZX, RotationOrder.ZXY, RotationOrder.ZYX })
            {
                quaternion e = quaternion.Euler(angles, order);
                Check(Near(new float3x3(e), float3x3.Euler(angles, order)), "Euler " + order + " as matrix and quaternion");
                Check(Near(Euler(e, order), angles, 1e-4f), "Euler angles of " + order + " back: " + Euler(e, order));
            }
            Check(SameRotation(quaternion.EulerXYZ(0, 0, PIHALF), quaternion.RotateZ(PIHALF)), "EulerXYZ about z");
            Check(Near(mul(quaternion.EulerXYZ(PIHALF, PIHALF, 0), new float3(0, 0, 1)), new float3(0, -1, 0)), "XYZ turns about x first");
            Check(SameRotation(mul(q, inverse(q)), quaternion.identity) && SameRotation(mul(q, conjugate(q)), quaternion.identity), "inverse and conjugate");
            Check(SameRotation(slerp(quaternion.identity, q, 0), quaternion.identity) && SameRotation(slerp(quaternion.identity, q, 1), q), "slerp end points");
            Check(Near(angle(quaternion.identity, q), 1.2f, 1e-4f), "angle: " + angle(quaternion.identity, q));
            quaternion look = quaternion.LookRotation(new float3(1, 0, 0), new float3(0, 1, 0));
            Check(Near(mul(look, new float3(0, 0, 1)), new float3(1, 0, 0)) && Near(forward(look), new float3(1, 0, 0)), "LookRotation");
            Check(float3x3.LookRotationSafe(new float3(0), new float3(0, 1, 0)).Equals(float3x3.identity), "LookRotationSafe of a zero forward");
            Check(SameRotation(unitexp(unitlog(q)), q) && SameRotation(exp(log(q)), q), "log and exp");
            Check(new quaternion(1, 2, 3, 4).ToString() == "quaternion(1f, 2f, 3f, 4f)", "quaternion ToString");
        }

        static void Projections()
        {
            float4x4 p = float4x4.PerspectiveFov(PIHALF, 1.0f, 1.0f, 10.0f);
            float4 near = mul(p, new float4(0, 0, -1, 1));
            float4 far = mul(p, new float4(0, 0, -10, 1));
            Check(Near(near.z / near.w, -1.0f) && Near(far.z / far.w, 1.0f), "PerspectiveFov maps near and far to -1 and 1");
            float4x4 o = float4x4.OrthoOffCenter(-2, 2, -1, 1, 1, 3);
            Check(Near(mul(o, new float4(2, 1, -3, 1)), new float4(1, 1, 1, 1)), "OrthoOffCenter");
            float4x4 trs = float4x4.TRS(new float3(1, 2, 3), quaternion.RotateZ(PIHALF), new float3(2));
            Check(Near(transform(trs, new float3(1, 0, 0)), new float3(1, 4, 3)), "TRS");
            float4x4 look = float4x4.LookAt(new float3(0, 0, -5), new float3(0), new float3(0, 1, 0));
            Check(Near(transform(look, new float3(0, 0, 5)), new float3(0)), "LookAt");
        }

        static void RandomNumbers()
        {
            var a = new Random(12345);
            var b = new Random(12345);
            bool same = true, inRange = true;
            for (int i = 0; i < 1000; i++)
            {
                same &= a.NextUInt() == b.NextUInt();
                float f = a.NextFloat(2, 3);
                int n = a.NextInt(-5, 5);
                inRange &= f >= 2 && f < 3 && n >= -5 && n < 5 && all(a.NextFloat3() < 1);
                b.NextFloat(2, 3);
                b.NextInt(-5, 5);
                b.NextFloat3();
                inRange &= Near(length(a.NextFloat3Direction()), 1.0f, 1e-4f) && Near(length(a.NextQuaternionRotation().value), 1.0f, 1e-4f);
                b.NextFloat3Direction();
                b.NextQuaternionRotation();
            }
            Check(same && inRange, "a seed gives the same sequence, in range");
            Check(Random.CreateFromIndex(1).state != Random.CreateFromIndex(2).state, "CreateFromIndex");
            bool threw = false;
            try
            {
                new Random(0);
            }
            catch (ArgumentException)
            {
                threw = true;
            }
            Check(threw, "seed zero throws");
            var c = new Random(1);
            uint first = c.NextUInt();
            var d = new Random();
            d.InitState(1);
            Check(d.NextUInt() == first, "InitState");
        }
    }
}
