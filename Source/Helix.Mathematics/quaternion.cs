using System;
using static Helix.Mathematics.math;

namespace Helix.Mathematics
{
    /// <summary>A rotation as a unit quaternion (x, y, z the vector part, w the scalar part).</summary>
    [Serializable]
    public partial struct quaternion : IEquatable<quaternion>, IFormattable
    {
        public float4 value;

        public static readonly quaternion identity = new quaternion(0.0f, 0.0f, 0.0f, 1.0f);

        public quaternion(float x, float y, float z, float w) { value = new float4(x, y, z, w); }
        public quaternion(float4 value) { this.value = value; }

        /// <summary>The rotation of an orthonormal matrix.</summary>
        public quaternion(float3x3 m)
        {
            float3 u = m.c0, v = m.c1, w = m.c2;
            float trace = u.x + v.y + w.z;
            if (trace > 0.0f)
            {
                float s = MathF.Sqrt(trace + 1.0f) * 2.0f;
                value = new float4((v.z - w.y) / s, (w.x - u.z) / s, (u.y - v.x) / s, 0.25f * s);
            }
            else if (u.x > v.y && u.x > w.z)
            {
                float s = MathF.Sqrt(1.0f + u.x - v.y - w.z) * 2.0f;
                value = new float4(0.25f * s, (v.x + u.y) / s, (w.x + u.z) / s, (v.z - w.y) / s);
            }
            else if (v.y > w.z)
            {
                float s = MathF.Sqrt(1.0f + v.y - u.x - w.z) * 2.0f;
                value = new float4((v.x + u.y) / s, 0.25f * s, (w.y + v.z) / s, (w.x - u.z) / s);
            }
            else
            {
                float s = MathF.Sqrt(1.0f + w.z - u.x - v.y) * 2.0f;
                value = new float4((w.x + u.z) / s, (w.y + v.z) / s, 0.25f * s, (u.y - v.x) / s);
            }
            value = normalize(value);
        }

        /// <summary>The rotation of the upper left 3x3 part.</summary>
        public quaternion(float4x4 m) : this(new float3x3(m.c0.xyz, m.c1.xyz, m.c2.xyz)) { }

        public static implicit operator quaternion(float4 v) => new quaternion(v);

        /// <summary>A rotation of angle radians about a unit axis.</summary>
        public static quaternion AxisAngle(float3 axis, float angle)
        {
            sincos(0.5f * angle, out float s, out float c);
            return new quaternion(new float4(axis * s, c));
        }

        public static quaternion RotateX(float angle) { sincos(0.5f * angle, out float s, out float c); return new quaternion(s, 0.0f, 0.0f, c); }
        public static quaternion RotateY(float angle) { sincos(0.5f * angle, out float s, out float c); return new quaternion(0.0f, s, 0.0f, c); }
        public static quaternion RotateZ(float angle) { sincos(0.5f * angle, out float s, out float c); return new quaternion(0.0f, 0.0f, s, c); }

        // The rotation about the axis of `first`, then `second`, then `third`.
        static quaternion Compose(quaternion first, quaternion second, quaternion third) => mul(third, mul(second, first));

        public static quaternion EulerXYZ(float3 xyz) => Compose(RotateX(xyz.x), RotateY(xyz.y), RotateZ(xyz.z));
        public static quaternion EulerXZY(float3 xyz) => Compose(RotateX(xyz.x), RotateZ(xyz.z), RotateY(xyz.y));
        public static quaternion EulerYXZ(float3 xyz) => Compose(RotateY(xyz.y), RotateX(xyz.x), RotateZ(xyz.z));
        public static quaternion EulerYZX(float3 xyz) => Compose(RotateY(xyz.y), RotateZ(xyz.z), RotateX(xyz.x));
        public static quaternion EulerZXY(float3 xyz) => Compose(RotateZ(xyz.z), RotateX(xyz.x), RotateY(xyz.y));
        public static quaternion EulerZYX(float3 xyz) => Compose(RotateZ(xyz.z), RotateY(xyz.y), RotateX(xyz.x));
        public static quaternion EulerXYZ(float x, float y, float z) => EulerXYZ(new float3(x, y, z));
        public static quaternion EulerXZY(float x, float y, float z) => EulerXZY(new float3(x, y, z));
        public static quaternion EulerYXZ(float x, float y, float z) => EulerYXZ(new float3(x, y, z));
        public static quaternion EulerYZX(float x, float y, float z) => EulerYZX(new float3(x, y, z));
        public static quaternion EulerZXY(float x, float y, float z) => EulerZXY(new float3(x, y, z));
        public static quaternion EulerZYX(float x, float y, float z) => EulerZYX(new float3(x, y, z));

        public static quaternion Euler(float3 xyz, RotationOrder order = RotationOrder.ZXY) => order switch
        {
            RotationOrder.XYZ => EulerXYZ(xyz),
            RotationOrder.XZY => EulerXZY(xyz),
            RotationOrder.YXZ => EulerYXZ(xyz),
            RotationOrder.YZX => EulerYZX(xyz),
            RotationOrder.ZXY => EulerZXY(xyz),
            RotationOrder.ZYX => EulerZYX(xyz),
            _ => identity,
        };

        public static quaternion Euler(float x, float y, float z, RotationOrder order = RotationOrder.ZXY) => Euler(new float3(x, y, z), order);

        /// <summary>The rotation whose forward (z) is `forward` and whose up (y) is as near `up` as it can be.</summary>
        public static quaternion LookRotation(float3 forward, float3 up) => new quaternion(float3x3.LookRotation(forward, up));

        /// <summary>LookRotation, identity when forward or up is degenerate or they are parallel.</summary>
        public static quaternion LookRotationSafe(float3 forward, float3 up) => new quaternion(float3x3.LookRotationSafe(forward, up));

        public readonly bool Equals(quaternion x) => value.x == x.value.x && value.y == x.value.y && value.z == x.value.z && value.w == x.value.w;
        public override readonly bool Equals(object x) => x is quaternion converted && Equals(converted);
        public override readonly int GetHashCode() => (int)hash(this);
        public override readonly string ToString() => string.Format("quaternion({0}f, {1}f, {2}f, {3}f)", value.x, value.y, value.z, value.w);
        public readonly string ToString(string format, IFormatProvider formatProvider) =>
            string.Format("quaternion({0}f, {1}f, {2}f, {3}f)", value.x.ToString(format, formatProvider), value.y.ToString(format, formatProvider),
                          value.z.ToString(format, formatProvider), value.w.ToString(format, formatProvider));
    }
}
