# Changelog

All notable changes to this package will be documented in this file.

## [1.0.0] - 2026-09-21

### Added
- Pure C# Flexbox layout engine (`FlexLayoutSolver`, `FlexNode`, `FlexLength`, `FlexOffsets`).
- Unity UI Canvas components:
  - `FlexContainer`: Layout group with full support for FlexDirection, FlexWrap, JustifyContent, AlignItems, AlignContent, Padding, Gap, and FitToContent self-sizing.
  - `FlexItem`: Per-item flex properties (FlexGrow, FlexShrink, FlexBasis, AlignSelf, Margin, Min/Max dimensions, IgnoreLayout).
- Fluent Code API (`FlexBuilder`, `FlexExtensions`) for procedural Canvas layout creation.
- Custom Unity Inspectors (`FlexContainerEditor`, `FlexItemEditor`) with 1-click presets and Scene View Gizmos (bounds, padding lines, margins, direction arrows).
- Comprehensive NUnit test suite covering pure math layout cases and Canvas integration.
