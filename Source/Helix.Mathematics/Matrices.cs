using System;
using static Helix.Mathematics.math;

namespace Helix.Mathematics
{
    // The rotation, scale and projection builders of the float matrices (column vectors: mul(m, v)).
    public partial struct float3x3
    {
        /// <summary>The rotation of a unit quaternion.</summary>
        public float3x3(quaternion q)
        {
            float4 v = q.value;
            float x2 = v.x + v.x, y2 = v.y + v.y, z2 = v.z + v.z;
            float xx = v.x * x2, yy = v.y * y2, zz = v.z * z2, xy = v.x * y2, xz = v.x * z2, yz = v.y * z2;
            float wx = v.w * x2, wy = v.w * y2, wz = v.w * z2;
            c0 = new float3(1.0f - (yy + zz), xy + wz, xz - wy);
            c1 = new float3(xy - wz, 1.0f - (xx + zz), yz + wx);
            c2 = new float3(xz + wy, yz - wx, 1.0f - (xx + yy));
        }

        /// <summary>The upper left 3x3 part.</summary>
        public float3x3(float4x4 f4x4) { c0 = f4x4.c0.xyz; c1 = f4x4.c1.xyz; c2 = f4x4.c2.xyz; }

        public static explicit operator float3x3(float4x4 f4x4) => new float3x3(f4x4);

        /// <summary>A rotation of angle radians about a unit axis.</summary>
        public static float3x3 AxisAngle(float3 axis, float angle)
        {
            sincos(angle, out float s, out float c);
            float t = 1.0f - c;
            float3 a = axis;
            return new float3x3(t * a.x * a.x + c, t * a.x * a.y - s * a.z, t * a.x * a.z + s * a.y,
                                t * a.x * a.y + s * a.z, t * a.y * a.y + c, t * a.y * a.z - s * a.x,
                                t * a.x * a.z - s * a.y, t * a.y * a.z + s * a.x, t * a.z * a.z + c);
        }

        public static float3x3 RotateX(float angle) { sincos(angle, out float s, out float c); return new float3x3(1, 0, 0, 0, c, -s, 0, s, c); }
        public static float3x3 RotateY(float angle) { sincos(angle, out float s, out float c); return new float3x3(c, 0, s, 0, 1, 0, -s, 0, c); }
        public static float3x3 RotateZ(float angle) { sincos(angle, out float s, out float c); return new float3x3(c, -s, 0, s, c, 0, 0, 0, 1); }

        public static float3x3 EulerXYZ(float3 xyz) => mul(RotateZ(xyz.z), mul(RotateY(xyz.y), RotateX(xyz.x)));
        public static float3x3 EulerXZY(float3 xyz) => mul(RotateY(xyz.y), mul(RotateZ(xyz.z), RotateX(xyz.x)));
        public static float3x3 EulerYXZ(float3 xyz) => mul(RotateZ(xyz.z), mul(RotateX(xyz.x), RotateY(xyz.y)));
        public static float3x3 EulerYZX(float3 xyz) => mul(RotateX(xyz.x), mul(RotateZ(xyz.z), RotateY(xyz.y)));
        public static float3x3 EulerZXY(float3 xyz) => mul(RotateY(xyz.y), mul(RotateX(xyz.x), RotateZ(xyz.z)));
        public static float3x3 EulerZYX(float3 xyz) => mul(RotateX(xyz.x), mul(RotateY(xyz.y), RotateZ(xyz.z)));
        public static float3x3 EulerXYZ(float x, float y, float z) => EulerXYZ(new float3(x, y, z));
        public static float3x3 EulerXZY(float x, float y, float z) => EulerXZY(new float3(x, y, z));
        public static float3x3 EulerYXZ(float x, float y, float z) => EulerYXZ(new float3(x, y, z));
        public static float3x3 EulerYZX(float x, float y, float z) => EulerYZX(new float3(x, y, z));
        public static float3x3 EulerZXY(float x, float y, float z) => EulerZXY(new float3(x, y, z));
        public static float3x3 EulerZYX(float x, float y, float z) => EulerZYX(new float3(x, y, z));

        public static float3x3 Euler(float3 xyz, RotationOrder order = RotationOrder.ZXY) => order switch
        {
            RotationOrder.XYZ => EulerXYZ(xyz),
            RotationOrder.XZY => EulerXZY(xyz),
            RotationOrder.YXZ => EulerYXZ(xyz),
            RotationOrder.YZX => EulerYZX(xyz),
            RotationOrder.ZXY => EulerZXY(xyz),
            RotationOrder.ZYX => EulerZYX(xyz),
            _ => identity,
        };

        public static float3x3 Euler(float x, float y, float z, RotationOrder order = RotationOrder.ZXY) => Euler(new float3(x, y, z), order);

        public static float3x3 Scale(float s) => new float3x3(s, 0, 0, 0, s, 0, 0, 0, s);
        public static float3x3 Scale(float x, float y, float z) => new float3x3(x, 0, 0, 0, y, 0, 0, 0, z);
        public static float3x3 Scale(float3 v) => Scale(v.x, v.y, v.z);

        /// <summary>Columns: right, up, forward; forward and up need not be normalized but must not be parallel.</summary>
        public static float3x3 LookRotation(float3 forward, float3 up)
        {
            float3 f = normalize(forward);
            float3 t = normalize(cross(up, f));
            return new float3x3(t, cross(f, t), f);
        }

        /// <summary>LookRotation, identity when forward or up is degenerate or they are parallel.</summary>
        public static float3x3 LookRotationSafe(float3 forward, float3 up)
        {
            float forwardLengthSq = dot(forward, forward);
            float upLengthSq = dot(up, up);
            forward *= rsqrt(forwardLengthSq);
            up *= rsqrt(upLengthSq);
            float3 t = cross(up, forward);
            float tLengthSq = dot(t, t);
            t *= rsqrt(tLengthSq);
            float mn = MathF.Min(MathF.Min(forwardLengthSq, upLengthSq), tLengthSq);
            float mx = MathF.Max(MathF.Max(forwardLengthSq, upLengthSq), tLengthSq);
            bool accept = mn > 1e-35f && mx < 1e35f && float.IsFinite(forwardLengthSq) && float.IsFinite(upLengthSq) && float.IsFinite(tLengthSq);
            return accept ? new float3x3(t, cross(forward, t), forward) : identity;
        }
    }

    public partial struct float4x4
    {
        /// <summary>A rotation and a translation.</summary>
        public float4x4(float3x3 rotation, float3 translation)
        {
            c0 = new float4(rotation.c0, 0.0f);
            c1 = new float4(rotation.c1, 0.0f);
            c2 = new float4(rotation.c2, 0.0f);
            c3 = new float4(translation, 1.0f);
        }

        public float4x4(quaternion rotation, float3 translation) : this(new float3x3(rotation), translation) { }

        static float4x4 From(float3x3 r) => new float4x4(r, new float3(0.0f, 0.0f, 0.0f));

        public static float4x4 AxisAngle(float3 axis, float angle) => From(float3x3.AxisAngle(axis, angle));
        public static float4x4 RotateX(float angle) => From(float3x3.RotateX(angle));
        public static float4x4 RotateY(float angle) => From(float3x3.RotateY(angle));
        public static float4x4 RotateZ(float angle) => From(float3x3.RotateZ(angle));
        public static float4x4 EulerXYZ(float3 xyz) => From(float3x3.EulerXYZ(xyz));
        public static float4x4 EulerXZY(float3 xyz) => From(float3x3.EulerXZY(xyz));
        public static float4x4 EulerYXZ(float3 xyz) => From(float3x3.EulerYXZ(xyz));
        public static float4x4 EulerYZX(float3 xyz) => From(float3x3.EulerYZX(xyz));
        public static float4x4 EulerZXY(float3 xyz) => From(float3x3.EulerZXY(xyz));
        public static float4x4 EulerZYX(float3 xyz) => From(float3x3.EulerZYX(xyz));
        public static float4x4 EulerXYZ(float x, float y, float z) => EulerXYZ(new float3(x, y, z));
        public static float4x4 EulerXZY(float x, float y, float z) => EulerXZY(new float3(x, y, z));
        public static float4x4 EulerYXZ(float x, float y, float z) => EulerYXZ(new float3(x, y, z));
        public static float4x4 EulerYZX(float x, float y, float z) => EulerYZX(new float3(x, y, z));
        public static float4x4 EulerZXY(float x, float y, float z) => EulerZXY(new float3(x, y, z));
        public static float4x4 EulerZYX(float x, float y, float z) => EulerZYX(new float3(x, y, z));
        public static float4x4 Euler(float3 xyz, RotationOrder order = RotationOrder.ZXY) => From(float3x3.Euler(xyz, order));
        public static float4x4 Euler(float x, float y, float z, RotationOrder order = RotationOrder.ZXY) => Euler(new float3(x, y, z), order);
        public static float4x4 Scale(float s) => new float4x4(s, 0, 0, 0, 0, s, 0, 0, 0, 0, s, 0, 0, 0, 0, 1);
        public static float4x4 Scale(float x, float y, float z) => new float4x4(x, 0, 0, 0, 0, y, 0, 0, 0, 0, z, 0, 0, 0, 0, 1);
        public static float4x4 Scale(float3 scales) => Scale(scales.x, scales.y, scales.z);
        public static float4x4 Translate(float3 vector) => new float4x4(new float4(1, 0, 0, 0), new float4(0, 1, 0, 0), new float4(0, 0, 1, 0), new float4(vector, 1));

        /// <summary>Translation, rotation and scale (scale first).</summary>
        public static float4x4 TRS(float3 translation, quaternion rotation, float3 scale)
        {
            float3x3 r = new float3x3(rotation);
            return new float4x4(new float4(r.c0 * scale.x, 0.0f), new float4(r.c1 * scale.y, 0.0f), new float4(r.c2 * scale.z, 0.0f), new float4(translation, 1.0f));
        }

        /// <summary>The transform of an object at eye looking at target.</summary>
        public static float4x4 LookAt(float3 eye, float3 target, float3 up) => new float4x4(float3x3.LookRotation(normalize(target - eye), up), eye);

        /// <summary>An orthographic projection (right handed view space, clip depth -1 to 1).</summary>
        public static float4x4 Ortho(float width, float height, float near, float far)
        {
            float rcpdx = 1.0f / width, rcpdy = 1.0f / height, rcpdz = 1.0f / (far - near);
            return new float4x4(2.0f * rcpdx, 0.0f, 0.0f, 0.0f, 0.0f, 2.0f * rcpdy, 0.0f, 0.0f, 0.0f, 0.0f, -2.0f * rcpdz, -(far + near) * rcpdz,
                                0.0f, 0.0f, 0.0f, 1.0f);
        }

        public static float4x4 OrthoOffCenter(float left, float right, float bottom, float top, float near, float far)
        {
            float rcpdx = 1.0f / (right - left), rcpdy = 1.0f / (top - bottom), rcpdz = 1.0f / (far - near);
            return new float4x4(2.0f * rcpdx, 0.0f, 0.0f, -(right + left) * rcpdx, 0.0f, 2.0f * rcpdy, 0.0f, -(top + bottom) * rcpdy, 0.0f, 0.0f,
                                -2.0f * rcpdz, -(far + near) * rcpdz, 0.0f, 0.0f, 0.0f, 1.0f);
        }

        /// <summary>A perspective projection from a vertical field of view in radians.</summary>
        public static float4x4 PerspectiveFov(float verticalFov, float aspect, float near, float far)
        {
            float cotangent = 1.0f / MathF.Tan(verticalFov * 0.5f);
            float rcpdz = 1.0f / (near - far);
            return new float4x4(cotangent / aspect, 0.0f, 0.0f, 0.0f, 0.0f, cotangent, 0.0f, 0.0f, 0.0f, 0.0f, (far + near) * rcpdz,
                                2.0f * near * far * rcpdz, 0.0f, 0.0f, -1.0f, 0.0f);
        }

        public static float4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
        {
            float rcpdz = 1.0f / (near - far), rcpWidth = 1.0f / (right - left), rcpHeight = 1.0f / (top - bottom);
            return new float4x4(2.0f * near * rcpWidth, 0.0f, (left + right) * rcpWidth, 0.0f, 0.0f, 2.0f * near * rcpHeight, (bottom + top) * rcpHeight, 0.0f,
                                0.0f, 0.0f, (far + near) * rcpdz, 2.0f * far * near * rcpdz, 0.0f, 0.0f, -1.0f, 0.0f);
        }
    }
}
