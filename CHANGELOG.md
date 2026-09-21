# Changelog

All notable changes to this package will be documented in this file.

## [1.0.1] - 2026-09-21

### Optimized
- Zero GC allocation solver: Refactored `FlexLayoutSolver` to use internal static scratch buffers and struct-based slices (`FlexItemContext`, `FlexLine`).
- Component caching: Implemented `ChildEntry` cache in `FlexContainer` to avoid repeated `GetComponent` lookups during layout passes.
- Non-recursive iterative resolution: Replaced recursion with an iterative breadth-first queue for nested flex containers.
- Re-entrancy protection: Added guards to prevent recursive layout rebuild loops during `FitToContentWidth` and `FitToContentHeight` execution.
- Performance: Marked methods on `FlexOffsets` and `FlexLength` as `readonly` to prevent defensive value-type copying.
- Documentation: Restructured documentation to professional technical standard without decorative AI icons.

## [1.0.0] - 2026-09-21

### Added
- Pure C# Flexbox layout engine (`FlexLayoutSolver`, `FlexNode`, `FlexLength`, `FlexOffsets`).
- Unity UI Canvas components (`FlexContainer`, `FlexItem`).
- Fluent Code API (`FlexBuilder`, `FlexExtensions`).
- Custom Unity Inspectors (`FlexContainerEditor`, `FlexItemEditor`) with 1-click presets and Scene View Gizmos.
- Hierarchy context menu commands (`GameObject/UI/Flexbox/...`).
- Automated NUnit test suite covering algorithm and Canvas integration.
