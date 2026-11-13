# Helix.Mathematics

The math library of the Helix engine for data oriented C# (jobs and the containers of Helix.Collections): the vector,
matrix and quaternion types and the `math` class with the same names, members and results as the reference's, in the
namespace `Helix.Mathematics`. It is plain C# with no dependencies.

## Contents

| Path | Content |
|---|---|
| `Source/Helix.Mathematics/Generated/` | `bool`, `int`, `uint`, `float`, `double` vectors (2 to 4 components) and matrices (2x2 to 4x4), `half2` to `half4`, the `math` functions over them and the vector forms of `Random` (written by the generator) |
| `Source/Helix.Mathematics/math.cs` | Constants, `math.RotationOrder`, `math.ShuffleComponent`, the quaternion functions, Euler angles of a rotation, `hash` of memory (xxHash32), half conversions |
| `Source/Helix.Mathematics/quaternion.cs`, `half.cs`, `Random.cs`, `Matrices.cs` | `quaternion`, `half`, `Random` (xorshift32), the rotation, scale and projection builders of `float3x3` and `float4x4` |
| `Tests/Helix.Mathematics.Tests/` | A console test program: formats, operators, functions, bits, halves, hashes, matrices, rotations, projections, random sequences |
| `Tools/Generate.py` | The generator (`python Tools/Generate.py`); its output is checked in |

## Use

```csharp
using Helix.Mathematics;
using static Helix.Mathematics.math;

float3 p = float3(1, 2, 3);
quaternion q = quaternion.EulerXYZ(0, radians(90), 0);
float4x4 m = float4x4.TRS(p, q, 1);
float3 moved = transform(m, float3(0, 0, 1));
var random = new Random(1234);
float r = random.NextFloat(0, 10);
```

- Vectors have fields `x` to `w`, every swizzle (`v.zyx`; the ones without repeated components can be set), an indexer,
  componentwise operators (comparisons give bool vectors; combine them with `any`, `all`, `select`), and conversions:
  widening ones implicit, narrowing ones explicit.
- Matrices are stored by columns (`c0` to `c3`); constructors that take elements take them row by row. `*` is
  componentwise, `mul(a, b)` is the matrix product, `mul(m, v)` transforms a column vector.
- Rotations are right handed with column vectors; Euler orders say which axis turns first (`EulerXYZ`: x, then y, then
  z), the default order is ZXY. Projections map view space looking along -z to clip depth -1 to 1.
- `Random` gives the same sequence for a seed on every platform; a seed of zero is an error.
- Text: `float3(1f, 2f, 3f)`, `int2(1, 2)`, matrices row by row (`float2x2(1f, 2f,  3f, 4f)`).

## Build

```
git clone https://github.com/kisstp2006/Helix.Mathematics
dotnet build Helix.Mathematics/Source/Helix.Mathematics -c Release
dotnet run --project Helix.Mathematics/Tests/Helix.Mathematics.Tests -c Release
```

The .NET 11 SDK is needed. Other Helix repositories take it as a submodule.

## License

GPL-3.0, see `LICENSE`.
