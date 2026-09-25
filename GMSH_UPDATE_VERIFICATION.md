# Gmsh.Net 4.15.2.1 — integration and regression verification

Date: 2026-09-25. Windows x64, .NET Framework 4.7.2, Visual Studio 2022.

GMesh assembly and file version: **1.0.1.3**.

## Dependency

GMesh and UnitTest reference the local assembly
`../Gmsh.Net/src/GMsh.Net/bin/Release/netstandard2.0/GMsh.Net.dll` (4.15.2.1).
Build the sibling Gmsh.Net project in Release before building Geometry.
Both consumers target x64 and copy `../Gmsh.Net/dll/windows/gmsh-4.15.dll`.
Obsolete GMsh.Net 1.0.484.3 and UnsafeEx references were removed.

## Corrections

### Native crash: explicit compatibility workaround

The original `RectangularWithShapeEmb14` terminated the native process with
`0xC0000374` (heap corruption). It also crashed alone in Debug.
Gmsh 4.15.2's PackingOfParallelograms pipeline invokes `UntangleTris`
unconditionally, even when GMesh's `Optimize` is false; this cannot be handled
with a managed exception handler. See the
[4.15.2 native generator](https://github.com/live-clones/gmsh/blob/gmsh_4_15_2/src/mesh/Generator.cpp#L637).

GMesh now substitutes FrontalDelaunayForQuads **only for PackingOfParallelograms
on native version 4.15.2**, retaining the requested recombination and constraints.
It reports the substitution in `GMeshGenerateMeshStatus.Warnings` and does not
mutate the caller's options. This is a workaround in GMesh, not a repair of the
upstream native DLL; callers requesting Packing on this version receive a
different meshing algorithm. The former crash test remains enabled.

Generation also ignores personal Gmsh configuration files and rejects non-finite,
zero or negative target/maximum/local mesh sizes and invalid minimum bounds
before entering native code.

### Test expectations after the native upgrade

- **Blossom:** 4.15.2 can recombine the old failing geometry directly. The test
  now checks area, quadrilateral majority and the node at the crossing of both
  constraints rather than requiring an upstream failure/fallback warning.
  The existing Simple retry implementation is unchanged.
- **ComplexShapes1:** four embedded points explicitly request size 5. The
  previous test incorrectly used 10 as the minimum. It now derives the minimum
  from the requested local sizes; the maximum stays unchanged.
- **RectangularWithTwoLines2:** triangles beside the acute crossing can have
  smaller areas with valid edge lengths. Its minimum area bound now accounts
  for `abs(sin(crossingAngle))`, from the triangle area formula. This adjustment
  applies only to that test; edge-length, maximum-area and average-size checks
  remain, with an additional total-area assertion of 20000. The meshing options
  and resulting mesh are unchanged.
- **Clone timing:** a single cold measurement intermittently took 6 ms against
  a 5 ms limit. The test now warms up cloning, checks equality for each of seven
  samples and applies the same 5 ms limit to their median.

## 50 new tests

`UnitTest/Mesh/GMesh415RegressionTest.cs` contains exactly 50 independent
`[TestMethod]` methods, registered in `UnitTest.csproj`.

| Coverage | Tests |
| --- | ---: |
| Rectangles, triangles, concavity, winding, XY/XZ/YZ/inclined planes, offsets and scales | 11 |
| One and two holes | 2 |
| Disconnected, adjacent and intersecting surfaces | 3 |
| Meshing algorithms, including the Packing workaround | 7 |
| Four recombination algorithms | 4 |
| Refinement, renumbering, local IDs, transfinite and optimization options | 5 |
| Embedded points, lines, crossings, shapes and local refinement | 8 |
| Repeated calls, recovery after exceptions, concurrent calls | 3 |
| Options cloning and input immutability | 2 |
| Null arguments and invalid mesh sizes | 3 |
| Mesh cloning and managed refinement | 2 |
| **Total** | **50** |

The shared mesh checks cover every returned mesh: nonempty output, finite
coordinates, unique node IDs, valid face references, no repeated face nodes,
no orphan vertices, positive face areas and conservation of analytical area.
Constraint checks additionally verify mapped vertices, ordered endpoints and
that line segments correspond to actual face edges.

## Verification

- Build: Debug and Release succeeded.
- Targeted integration run: **54/54 passed** (50 new tests plus the four original
  problem cases).
- Full Release suite: **784/784 passed**, zero failed or skipped.
- Full Debug suite: **784/784 passed**, zero failed or skipped.

Result files are under `TestResults/`: `gmsh-50-regressions.trx`,
`gmsh-415-final-release.trx` and `gmsh-415-final-debug.trx`.
Earlier `gmsh-4.15-*` results document the original failures, not the final state.

## Reproduction

Use Visual Studio's `MSBuild.exe` and `vstest.console.exe`:

```powershell
MSBuild.exe GPCGeometry.sln /t:Build /p:Configuration=Release /v:minimal
MSBuild.exe GPCGeometry.sln /t:Build /p:Configuration=Debug /v:minimal
vstest.console.exe UnitTest\bin\Release\UnitTest.dll /Platform:x64 /InIsolation /Blame '/Logger:trx;LogFileName=gmsh-415-final-release.trx' /ResultsDirectory:TestResults
vstest.console.exe UnitTest\bin\Debug\UnitTest.dll /Platform:x64 /InIsolation /Blame '/Logger:trx;LogFileName=gmsh-415-final-debug.trx' /ResultsDirectory:TestResults
```

The full suite runs without a test filter. These checks cover the cases above;
they do not establish correctness for every possible geometry or native algorithm.
