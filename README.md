# Canvas Flexbox Layout for Unity (uGUI)

[![Unity 2022.3+](https://img.shields.io/badge/unity-2022.3%2B-blue.svg)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.md)
[![Pure C#](https://img.shields.io/badge/Pure%20C%23-Zero%20Native%20DLL-success.svg)](#)

A high-performance, **Pure C# Flexbox layout engine** specifically crafted for Unity **uGUI (Canvas / RectTransform)**. It brings the power and elegance of modern CSS Flexbox to Unity UI without requiring native DLLs or UI Toolkit.

---

## 🌟 Tại sao cần Canvas Flexbox? (Key Benefits)

Mặc định, Unity uGUI (`HorizontalLayoutGroup`, `VerticalLayoutGroup`, `GridLayoutGroup`) rất hạn chế:
- ❌ Không hỗ trợ **Wrap đa dòng** theo kích thước động của item.
- ❌ Thiếu các chế độ căn chỉnh chuẩn hiện đại: `space-between`, `space-around`, `space-evenly`.
- ❌ Không có cơ chế **Flex-Grow** và **Flex-Shrink** phân phối khoảng trống dư / bù trừ co giãn theo tỷ lệ mượt mà.
- ❌ Căn chỉnh trục phụ (`align-items`, `align-self`) rất khó khăn.

**Canvas Flexbox** giải quyết triệt để toàn bộ vấn đề trên:
- ✅ **Pure C# Layout Solver**: Không phụ thuộc file native C++ (`.dll`, `.so`, `.dylib`), chạy mượt mà trên 100% nền tảng: **WebGL, iOS, Android, PC, macOS, Consoles**.
- ✅ **Tương thích hoàn toàn Canvas & RectTransform**: Giữ nguyên pivot con, hỗ trợ animation (DOTween, Animator), Particle System (`UIParticle`), Spine Canvas, Canvas Scaler.
- ✅ **Hai phong cách làm việc linh hoạt**:
  1. **Inspector-First**: Kéo thả component `FlexContainer` và `FlexItem` trên Prefab với 1-Click Presets.
  2. **Fluent Code API (`FlexBuilder`)**: Dựng UI hoàn toàn bằng code C# mạch lạc, nhanh chóng.
- ✅ **Scene View Gizmos Trực Quan**: Vẽ đường viền padding (cyan chấm gạch), margin của item (cam), hướng trục chính (mũi tên vàng) ngay trên Scene View.
- ✅ **Tự động co giãn theo nội dung (`FitToContent`)**: Hoạt động mượt mà như ContentSizeFitter.

---

## 📦 Cài đặt (Installation)

### Cách 1: Embedded Package (Khuyên dùng trong project nội bộ)
Thư mục package đã nằm sẵn tại:
```
Packages/com.nghinv.flexbox
```
Package Manager của Unity sẽ tự động nạp package và compile các assembly definitions:
- `CanvasFlexbox.Runtime`
- `CanvasFlexbox.Editor`
- `CanvasFlexbox.Editor.Tests`

### Cách 2: Qua Git URL (UPM)
Trong Unity Editor: `Window` > `Package Manager` > `+` > `Add package from git URL...`:
```
https://github.com/nghinv-software/unity-flex-box.git
```
Hoặc chỉ định release tag:
```
https://github.com/nghinv-software/unity-flex-box.git#1.0.0
```

---

## 🚀 Hướng Dẫn Sử Dụng (Quick Start)

### 1. Sử dụng qua Inspector (Prefab / Visual Workflow)

1. Chọn bất kỳ UI GameObject nào trong Canvas (hoặc tạo Panel mới).
2. Nhấn **Add Component** $\rightarrow$ chọn **`Flex Container`**.
3. Sử dụng thanh công cụ **Quick Presets**:
   - `Row Start`: Xếp ngang từ trái sang phải.
   - `Row Between`: Căn 2 đầu đều nhau (`space-between`).
   - `Column Center`: Xếp dọc căn giữa (`center`).
   - `Grid Wrap`: Tự động xuống dòng khi đầy chiều rộng (`wrap`).
4. Thiết lập khoảng cách:
   - **Row Gap / Column Gap**: Khoảng cách giữa các hàng và cột (pixel).
   - **Padding**: Đệm lề trong (Left, Right, Top, Bottom).
5. (Tuỳ chọn) Gắn **`Flex Item`** lên GameObject con để tuỳ chỉnh:
   - **Flex Grow**: Cho phép co giãn phình to chiếm khoảng trống dư (ví dụ: Search bar grow = 1, nút bấm bên cạnh grow = 0).
   - **Flex Shrink**: Tỷ lệ co lại khi thiếu diện tích.
   - **Flex Basis**: Chiều dài cơ sở ban đầu (`Auto`, Pixel `px`, hoặc Phần trăm `%`).
   - **Align Self**: Ghi đè căn chỉnh riêng cho item này (`Stretch`, `Center`, `FlexStart`, `FlexEnd`).
   - **Margin**: Khoảng cách riêng quanh item.

---

### 2. Sử dụng qua Fluent Code API (`FlexBuilder`)

Bạn có thể tạo giao diện hoàn toàn bằng C# code trong vài dòng:

```csharp
using UnityEngine;
using CanvasFlexbox;

public class UIShowcase : MonoBehaviour
{
    [SerializeField] private RectTransform _canvasTransform;

    private void Start()
    {
        // 1. Tạo Header Bar nằm ngang, căn 2 đầu (Space-Between), căn giữa theo chiều dọc
        var header = FlexBuilder.Create("HeaderBar")
            .AsRow()
            .WithJustifyContent(JustifyContent.SpaceBetween)
            .WithAlignItems(AlignItems.Center)
            .WithPadding(horizontal: 20f, vertical: 10f)
            .WithSize(800f, 60f)
            .AttachTo(_canvasTransform);

        // Nút Back (kích thước cố định, không co giãn)
        header.AddChild("BtnBack")
            .WithSize(80f, 40f)
            .WithFlex(grow: 0f, shrink: 0f);

        // Tiêu đề ở giữa (chiếm phần trống còn lại)
        header.AddChild("TitleText")
            .WithSize(200f, 40f)
            .WithFlex(grow: 1f);

        // Nút Cài đặt
        header.AddChild("BtnSettings")
            .WithSize(40f, 40f)
            .WithFlex(grow: 0f, shrink: 0f);

        // 2. Tạo Grid tự động Wrap nhiều hàng với Gap 12px
        var cardGrid = FlexBuilder.Create("CardGrid")
            .AsRow()
            .WithWrap(FlexWrap.Wrap)
            .WithGap(columnGap: 16f, rowGap: 16f)
            .WithPadding(16f)
            .WithSize(600f, 400f)
            .AttachTo(_canvasTransform);

        for (int i = 0; i < 6; i++)
        {
            cardGrid.AddChild($"Card_{i}")
                .WithSize(180f, 100f)
                .WithMargin(4f);
        }
    }
}
```

---

## 🎨 Cấu Trúc Thông Số Flexbox (Cheat Sheet)

| Thuộc tính | Các giá trị hỗ trợ | Mô tả |
| :--- | :--- | :--- |
| **Direction** | `Row`, `RowReverse`, `Column`, `ColumnReverse` | Hướng của trục chính (Main Axis) |
| **Wrap** | `NoWrap`, `Wrap`, `WrapReverse` | Có cho phép rớt xuống dòng tiếp theo hay ép trên 1 hàng |
| **JustifyContent** | `FlexStart`, `FlexEnd`, `Center`, `SpaceBetween`, `SpaceAround`, `SpaceEvenly` | Phân phối khoảng cách trên trục chính |
| **AlignItems** | `FlexStart`, `FlexEnd`, `Center`, `Stretch` | Căn chỉnh các item trên trục phụ (Cross Axis) của dòng |
| **AlignContent** | `FlexStart`, `FlexEnd`, `Center`, `SpaceBetween`, `SpaceAround`, `Stretch` | Phân phối các dòng trên trục phụ khi có nhiều dòng (Wrap) |
| **AlignSelf** | `Auto`, `FlexStart`, `FlexEnd`, `Center`, `Stretch` | Item con tự ghi đè cách căn chỉnh của container |
| **FlexGrow** | Số thực $\ge 0$ | Trọng số hấp thụ khoảng trống dư |
| **FlexShrink** | Số thực $\ge 0$ | Trọng số bị co lại khi thiếu diện tích |
| **FlexBasis** | `Auto`, Pixels (`px`), Percent (`%`) | Kích thước ban đầu trước khi tính toán |
| **FitToContent** | `Width`, `Height` (Boolean) | Tự động resize RectTransform container bọc vừa khít con |

---

## 🧪 Kiểm Thử Tự Động (Unit Tests)

Package đi kèm bộ Unit Test đầy đủ trong `Tests/Editor`:
- `RowLayout_JustifyContent_SpaceBetween_PositionsCorrectly`
- `RowLayout_JustifyContent_Center_PositionsCorrectly`
- `RowLayout_FlexGrow_DistributesRemainingSpace`
- `RowLayout_FlexShrink_ReducesOverflowingItems`
- `ColumnLayout_WithPaddingAndGap_CalculatesYOffsets`
- `MultiLine_Wrap_BreaksLineWhenExceedingWidth`
- `AlignItems_Stretch_StretchesCrossDimension`
- `ReverseDirection_RowReverse_InvertsMainOrder`
- `FlexContainer_PositionsChildren_WithSpaceBetween`
- `FlexBuilder_FluentAPI_ConstructsValidHierarchy`
- `FlexContainer_FitToContent_ResizesRectTransform`

---

## 📄 License

Phát hành dưới giấy phép [MIT License](LICENSE.md). Bạn có thể tự do tích hợp vào mọi dự án game thương mại hoặc mã nguồn mở.
