using NUnit.Framework;
using UnityEngine;

namespace CanvasFlexbox.Tests
{
    [TestFixture]
    public class FlexLayoutSolverTests
    {
        [Test]
        public void RowLayout_JustifyContent_SpaceBetween_PositionsCorrectly()
        {
            var root = new FlexNode
            {
                Width = 500f,
                Height = 100f,
                Direction = FlexDirection.Row,
                JustifyContent = JustifyContent.SpaceBetween,
                AlignItems = AlignItems.FlexStart
            };

            var item1 = new FlexNode { Width = 100f, Height = 50f };
            var item2 = new FlexNode { Width = 100f, Height = 50f };
            var item3 = new FlexNode { Width = 100f, Height = 50f };

            root.AddChild(item1);
            root.AddChild(item2);
            root.AddChild(item3);

            FlexLayoutSolver.CalculateLayout(root, 500f, 100f);

            // Remaining space = 500 - 300 = 200. 2 gaps of 100 each.
            Assert.AreEqual(0f, item1.LayoutX, 0.01f);
            Assert.AreEqual(200f, item2.LayoutX, 0.01f);
            Assert.AreEqual(400f, item3.LayoutX, 0.01f);
        }

        [Test]
        public void RowLayout_JustifyContent_Center_PositionsCorrectly()
        {
            var root = new FlexNode
            {
                Width = 400f,
                Height = 100f,
                Direction = FlexDirection.Row,
                JustifyContent = JustifyContent.Center
            };

            var item1 = new FlexNode { Width = 100f, Height = 50f };
            var item2 = new FlexNode { Width = 100f, Height = 50f };

            root.AddChild(item1);
            root.AddChild(item2);

            FlexLayoutSolver.CalculateLayout(root, 400f, 100f);

            // Free space = 400 - 200 = 200. Offset = 100.
            Assert.AreEqual(100f, item1.LayoutX, 0.01f);
            Assert.AreEqual(200f, item2.LayoutX, 0.01f);
        }

        [Test]
        public void RowLayout_FlexGrow_DistributesRemainingSpace()
        {
            var root = new FlexNode
            {
                Width = 400f,
                Height = 100f,
                Direction = FlexDirection.Row
            };

            var item1 = new FlexNode { Width = 100f, Height = 50f, FlexGrow = 1f };
            var item2 = new FlexNode { Width = 100f, Height = 50f, FlexGrow = 2f };

            root.AddChild(item1);
            root.AddChild(item2);

            FlexLayoutSolver.CalculateLayout(root, 400f, 100f);

            // Free space = 200. Total grow = 3.
            // Item 1 gets 100 + 200 * (1/3) = 166.67
            // Item 2 gets 100 + 200 * (2/3) = 233.33
            Assert.AreEqual(166.67f, item1.LayoutWidth, 0.1f);
            Assert.AreEqual(233.33f, item2.LayoutWidth, 0.1f);
            Assert.AreEqual(400f, item1.LayoutWidth + item2.LayoutWidth, 0.01f);
        }

        [Test]
        public void RowLayout_FlexShrink_ReducesOverflowingItems()
        {
            var root = new FlexNode
            {
                Width = 300f,
                Height = 100f,
                Direction = FlexDirection.Row
            };

            var item1 = new FlexNode { Width = 200f, Height = 50f, FlexShrink = 1f };
            var item2 = new FlexNode { Width = 200f, Height = 50f, FlexShrink = 1f };

            root.AddChild(item1);
            root.AddChild(item2);

            FlexLayoutSolver.CalculateLayout(root, 300f, 100f);

            // Deficit = 100. Equal shrink -> 150 each.
            Assert.AreEqual(150f, item1.LayoutWidth, 0.01f);
            Assert.AreEqual(150f, item2.LayoutWidth, 0.01f);
            Assert.AreEqual(300f, item1.LayoutWidth + item2.LayoutWidth, 0.01f);
        }

        [Test]
        public void ColumnLayout_WithPaddingAndGap_CalculatesYOffsets()
        {
            var root = new FlexNode
            {
                Width = 200f,
                Height = 300f,
                Direction = FlexDirection.Column,
                Padding = new FlexOffsets(15f),
                RowGap = 10f
            };

            var item1 = new FlexNode { Width = 100f, Height = 50f };
            var item2 = new FlexNode { Width = 100f, Height = 80f };

            root.AddChild(item1);
            root.AddChild(item2);

            FlexLayoutSolver.CalculateLayout(root, 200f, 300f);

            Assert.AreEqual(15f, item1.LayoutY, 0.01f);
            Assert.AreEqual(15f + 50f + 10f, item2.LayoutY, 0.01f); // 75
        }

        [Test]
        public void MultiLine_Wrap_BreaksLineWhenExceedingWidth()
        {
            var root = new FlexNode
            {
                Width = 250f,
                Height = 300f,
                Direction = FlexDirection.Row,
                Wrap = FlexWrap.Wrap,
                RowGap = 10f
            };

            var item1 = new FlexNode { Width = 100f, Height = 40f };
            var item2 = new FlexNode { Width = 100f, Height = 40f };
            var item3 = new FlexNode { Width = 100f, Height = 50f }; // Should wrap

            root.AddChild(item1);
            root.AddChild(item2);
            root.AddChild(item3);

            FlexLayoutSolver.CalculateLayout(root, 250f, 300f);

            // Line 1: item1 at X=0, Y=0; item2 at X=100, Y=0. Height of line 1 is 40.
            Assert.AreEqual(0f, item1.LayoutX, 0.01f);
            Assert.AreEqual(0f, item1.LayoutY, 0.01f);

            Assert.AreEqual(100f, item2.LayoutX, 0.01f);
            Assert.AreEqual(0f, item2.LayoutY, 0.01f);

            // Line 2: item3 wrapped to X=0, Y = 40 + 10 (row gap) = 50.
            Assert.AreEqual(0f, item3.LayoutX, 0.01f);
            Assert.AreEqual(50f, item3.LayoutY, 0.01f);
        }

        [Test]
        public void AlignItems_Stretch_StretchesCrossDimension()
        {
            var root = new FlexNode
            {
                Width = 300f,
                Height = 150f,
                Direction = FlexDirection.Row,
                AlignItems = AlignItems.Stretch
            };

            var item1 = new FlexNode { Width = 80f, Height = 40f, AlignSelf = AlignSelf.Auto };
            var item2 = new FlexNode { Width = 80f, Height = 40f, AlignSelf = AlignSelf.Center };

            root.AddChild(item1);
            root.AddChild(item2);

            FlexLayoutSolver.CalculateLayout(root, 300f, 150f);

            // Item1 stretched to 150 height
            Assert.AreEqual(150f, item1.LayoutHeight, 0.01f);
            // Item2 centered: (150 - 40) / 2 = 55
            Assert.AreEqual(40f, item2.LayoutHeight, 0.01f);
            Assert.AreEqual(55f, item2.LayoutY, 0.01f);
        }

        [Test]
        public void ReverseDirection_RowReverse_InvertsMainOrder()
        {
            var root = new FlexNode
            {
                Width = 300f,
                Height = 100f,
                Direction = FlexDirection.RowReverse,
                JustifyContent = JustifyContent.FlexStart
            };

            var item1 = new FlexNode { Width = 80f, Height = 50f };
            var item2 = new FlexNode { Width = 100f, Height = 50f };

            root.AddChild(item1);
            root.AddChild(item2);

            FlexLayoutSolver.CalculateLayout(root, 300f, 100f);

            // In RowReverse:
            // First child (item1) is placed at the rightmost edge: 300 - 80 = 220
            Assert.AreEqual(220f, item1.LayoutX, 0.01f);
            // Second child (item2) is to the left of item1: 220 - 100 = 120
            Assert.AreEqual(120f, item2.LayoutX, 0.01f);
        }
    }
}
