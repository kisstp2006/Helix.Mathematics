using System;

namespace Helix.Mathematics
{
    /// <summary>A 16 bit floating point value (IEEE 754 binary16).</summary>
    [Serializable]
    public struct half : IEquatable<half>, IFormattable
    {
        /// <summary>The raw bits.</summary>
        public ushort value;

        public static readonly half zero = new half();

        /// <summary>The largest finite half, as a float.</summary>
        public static float MaxValue => 65504.0f;
        /// <summary>The smallest finite half, as a float.</summary>
        public static float MinValue => -65504.0f;
        public static half MaxValueAsHalf => new half(MaxValue);
        public static half MinValueAsHalf => new half(MinValue);

        public half(half x) { value = x.value; }
        public half(float v) { value = (ushort)math.f32tof16(v); }
        public half(double v) { value = (ushort)math.f32tof16((float)v); }

        public static explicit operator half(float v) => new half(v);
        public static explicit operator half(double v) => new half(v);
        public static implicit operator float(half d) => math.f16tof32(d.value);
        public static implicit operator double(half d) => math.f16tof32(d.value);

        /// <summary>Bitwise equality (as the reference: NaNs with the same bits are equal, +0 and -0 are not).</summary>
        public static bool operator ==(half lhs, half rhs) => lhs.value == rhs.value;
        public static bool operator !=(half lhs, half rhs) => lhs.value != rhs.value;

        public readonly bool Equals(half rhs) => value == rhs.value;
        public override readonly bool Equals(object o) => o is half converted && Equals(converted);
        public override readonly int GetHashCode() => (int)math.hash(this);
        public override readonly string ToString() => math.f16tof32(value).ToString();
        public readonly string ToString(string format, IFormatProvider formatProvider) => math.f16tof32(value).ToString(format, formatProvider);
    }
}
