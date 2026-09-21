# Canvas Flexbox Layout for Unity (uGUI)

Canvas Flexbox Layout is a high-performance, pure C# Flexbox layout engine designed specifically for Unity uGUI (Canvas / RectTransform). It brings standard W3C CSS Flexbox layout semantics to Unity UI while eliminating runtime garbage collection allocations.

---

## Technical Overview

Unity's built-in uGUI layout system (`HorizontalLayoutGroup`, `VerticalLayoutGroup`, `GridLayoutGroup`) has fundamental architectural constraints:
- No native multi-line wrapping based on dynamic child dimensions.
- Missing standard alignment modes (`space-between`, `space-around`, `space-evenly`).
- Lack of proportional space distribution (`flex-grow`, `flex-shrink`).
- Inflexible cross-axis alignment per item (`align-self`).

Canvas Flexbox provides a complete W3C-compliant Flexbox implementation directly on `RectTransform` without external native binaries (such as Facebook Yoga C++ DLLs), ensuring seamless execution across all Unity target platforms including WebGL, iOS, Android, macOS, Windows, Linux, and Consoles.

---

## Performance and Architecture

### 1. Zero Garbage Collection Allocation (0 B GC)
Standard layout recalculations in games can happen frequently (e.g. during animations, screen resizes, or content changes). To prevent GC-induced frame drops, Canvas Flexbox utilizes:
- **Struct-based calculations**: Item contexts and line descriptors are implemented as value types (`FlexItemContext`, `FlexLine`).
- **Internal static memory pooling**: Reusable scratch buffers handle item collections, line slicing, and BFS traversal without heap allocations.
- **Node reuse**: `FlexContainer` pools and reuses `FlexNode` instances across layout passes rather than instantiating new objects.

### 2. Component Cache Optimization
In typical uGUI layouts, querying `GetComponent<T>()` across child transforms during rebuild passes causes significant CPU overhead. `FlexContainer` employs an internal child entry cache that updates only upon hierarchy changes (`OnTransformChildrenChanged`) or explicit invalidations, minimizing component query frequency.

### 3. Iterative Non-Recursive Resolution
Nested flex containers are solved iteratively using a breadth-first traversal queue (`s_NodeQueue`). This eliminates call stack overhead, avoids recursion depth limits, and maintains thread-safe buffer lifecycle boundaries.

### 4. Re-entrancy Protection
Self-sizing features (`FitToContentWidth`, `FitToContentHeight`) modify the container's own `RectTransform` dimensions during layout passes. `FlexContainer` includes layout recursion guards to prevent recursive rebuild triggers.

### 5. Pivot Preservation
Child transforms are positioned by calculating the exact offset according to each child's current `pivot`. UI elements can use custom pivots (e.g., center `(0.5, 0.5)` for scale animations) without visual offset artifacts.

---

## Installation

### Method 1: Git URL (Unity Package Manager)
1. Open the Unity Editor and navigate to `Window` > `Package Manager`.
2. Click the `+` button in the top-left corner and select `Add package from git URL...`.
3. Enter the repository URL:
```
https://github.com/nghinv-software/unity-flex-box.git
```
To lock to a specific release version:
```
https://github.com/nghinv-software/unity-flex-box.git#1.0.0
```

### Method 2: manifest.json
Add the package declaration directly to `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.nghinv.flexbox": "https://github.com/nghinv-software/unity-flex-box.git#1.0.0"
  }
}
```

---

## Usage Guide

### 1. Editor / Inspector Workflow (No Code)

#### Creating Containers
In the Hierarchy window, right-click any UI Canvas or select from the top menu `GameObject` > `UI` > `Flexbox`:
- **Flex Container (Row)**: Horizontal layout (Headers, toolbars, icon rows).
- **Flex Container (Column)**: Vertical layout (Menus, settings dialogs, modal cards).
- **Flex Container (Wrap Grid)**: Multi-line grid with automatic line breaking.

#### Configuring Container Properties
Select the container GameObject to configure layout parameters in the Inspector:
- **Quick Presets**: One-click configuration buttons (`Row Start`, `Row Between`, `Column Center`, `Grid Wrap`).
- **Flex Axis & Direction**:
  - `Direction`: `Row`, `RowReverse`, `Column`, `ColumnReverse`.
  - `Wrap`: `NoWrap`, `Wrap`, `WrapReverse`.
  - `Justify Content`: `FlexStart`, `FlexEnd`, `Center`, `SpaceBetween`, `SpaceAround`, `SpaceEvenly`.
- **Cross Axis Alignment**:
  - `Align Items`: `FlexStart`, `FlexEnd`, `Center`, `Stretch`.
  - `Align Content`: Distribution of wrapped lines along the cross axis.
- **Spacing & Padding**:
  - `Row Gap` / `Column Gap`: Explicit pixel spacing between rows and columns.
  - `Padding`: Inset offsets (`Left`, `Right`, `Top`, `Bottom`).
- **Container Sizing**:
  - `Fit To Content Width` / `Fit To Content Height`: Resizes container automatically to enclose child items.

#### Configuring Child Elements
Add a `FlexItem` component to any child GameObject:
- **Flex Grow**: Ratio for absorbing available free space along the main axis.
- **Flex Shrink**: Ratio for contracting when total item size exceeds available space.
- **Flex Basis**: Initial size before remaining space is distributed (`Auto`, Pixels, or Percentage).
- **Align Self**: Individual cross-axis alignment override (`FlexStart`, `FlexEnd`, `Center`, `Stretch`).
- **Margins**: Per-item outer spacing offsets.
- **Constraints**: Minimum and maximum dimensions (`Min W`, `Max W`, `Min H`, `Max H`).

#### Scene View Gizmos
When a `FlexContainer` is selected in Edit mode:
- Solid cyan frame indicates the container's outer bounds.
- Dotted cyan inner frame indicates active padding areas.
- Yellow directional arrow indicates the main axis layout flow.
- Orange dotted frames indicate margin boundaries of child items.

---

### 2. Fluent Code API (C#)

Procedural UI can be composed using `FlexBuilder`:

```csharp
using UnityEngine;
using CanvasFlexbox;

public class UIController : MonoBehaviour
{
    [SerializeField] private RectTransform containerParent;

    private void Start()
    {
        // Construct a responsive top navigation bar
        var navBar = FlexBuilder.Create("NavigationBar")
            .AsRow()
            .WithJustifyContent(JustifyContent.SpaceBetween)
            .WithAlignItems(AlignItems.Center)
            .WithPadding(horizontal: 24f, vertical: 12f)
            .WithSize(800f, 64f)
            .AttachTo(containerParent);

        // Fixed-size left item
        navBar.AddChild("BackButton")
            .WithSize(48f, 48f)
            .WithFlex(grow: 0f, shrink: 0f);

        // Flexible center title
        navBar.AddChild("Title")
            .WithSize(200f, 48f)
            .WithFlex(grow: 1f);

        // Fixed-size right action
        navBar.AddChild("SettingsButton")
            .WithSize(48f, 48f)
            .WithFlex(grow: 0f, shrink: 0f);
    }
}
```

---

## Property Reference

| Property | Values | Description |
| :--- | :--- | :--- |
| `Direction` | `Row`, `RowReverse`, `Column`, `ColumnReverse` | Orientation of the main axis |
| `Wrap` | `NoWrap`, `Wrap`, `WrapReverse` | Controls whether items wrap onto multiple lines |
| `JustifyContent` | `FlexStart`, `FlexEnd`, `Center`, `SpaceBetween`, `SpaceAround`, `SpaceEvenly` | Main-axis item distribution |
| `AlignItems` | `FlexStart`, `FlexEnd`, `Center`, `Stretch` | Cross-axis item alignment within each line |
| `AlignContent` | `FlexStart`, `FlexEnd`, `Center`, `SpaceBetween`, `SpaceAround`, `Stretch` | Cross-axis line distribution for multi-line wrap |
| `AlignSelf` | `Auto`, `FlexStart`, `FlexEnd`, `Center`, `Stretch` | Per-item override of container `AlignItems` |
| `FlexGrow` | Float (>= 0) | Factor for growing into positive free space |
| `FlexShrink` | Float (>= 0) | Factor for shrinking under negative free space |
| `FlexBasis` | `Auto`, Pixel value, Percentage | Initial base size along the main axis |
| `RowGap` / `ColumnGap` | Float (pixels) | Spacing between lines and items |
| `FitToContent` | Boolean | Automatically sizes RectTransform to fit child contents |

---

## Verification and Testing

Automated NUnit tests are located in `Tests/Editor`:
- `FlexLayoutSolverTests`: Pure algorithm tests verifying space distribution, grow/shrink weighting, multi-line wrapping, and reverse layout calculations.
- `FlexContainerTests`: Integration tests verifying Canvas rebuild cycles, driven transform trackers, self-sizing, and child positioning accuracy.

---

## License

This project is licensed under the MIT License. See [LICENSE.md](LICENSE.md) for details.
