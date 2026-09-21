using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace CanvasFlexbox.Tests
{
    [TestFixture]
    public class FlexContainerTests
    {
        private GameObject _canvasObject;
        private RectTransform _canvasRect;

        [SetUp]
        public void SetUp()
        {
            _canvasObject = new GameObject("TestCanvas", typeof(RectTransform), typeof(Canvas));
            _canvasRect = _canvasObject.GetComponent<RectTransform>();
            _canvasRect.sizeDelta = new Vector2(800f, 600f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvasObject != null)
            {
                Object.DestroyImmediate(_canvasObject);
            }
        }

        [Test]
        public void FlexContainer_PositionsChildren_WithSpaceBetween()
        {
            var containerObj = new GameObject("Container", typeof(RectTransform), typeof(FlexContainer));
            containerObj.transform.SetParent(_canvasRect, false);
            var containerRect = containerObj.GetComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(500f, 100f);

            var container = containerObj.GetComponent<FlexContainer>();
            container.Direction = FlexDirection.Row;
            container.JustifyContent = JustifyContent.SpaceBetween;
            container.AlignItems = AlignItems.Center;

            var child1 = new GameObject("Child1", typeof(RectTransform));
            child1.transform.SetParent(containerRect, false);
            var rect1 = child1.GetComponent<RectTransform>();
            rect1.sizeDelta = new Vector2(100f, 40f);

            var child2 = new GameObject("Child2", typeof(RectTransform));
            child2.transform.SetParent(containerRect, false);
            var rect2 = child2.GetComponent<RectTransform>();
            rect2.sizeDelta = new Vector2(100f, 40f);

            // Execute Layout passes
            container.CalculateLayoutInputHorizontal();
            container.CalculateLayoutInputVertical();
            container.SetLayoutHorizontal();
            container.SetLayoutVertical();

            // Total width = 500. Items = 2 x 100 = 200. Free = 300.
            // Child1 at Left = 0. Child2 at Left = 400.
            // Child1 pivot default is (0.5, 0.5):
            // PosX = LayoutX + (LayoutWidth * 0.5) = 0 + 50 = 50.
            // PosY = -(LayoutY + (LayoutHeight * 0.5)) = -(30 + 20) = -50 (centered in 100 height).
            Assert.AreEqual(50f, rect1.anchoredPosition.x, 0.1f);
            Assert.AreEqual(450f, rect2.anchoredPosition.x, 0.1f);
            Assert.AreEqual(-50f, rect1.anchoredPosition.y, 0.1f);
            Assert.AreEqual(-50f, rect2.anchoredPosition.y, 0.1f);
        }

        [Test]
        public void FlexBuilder_FluentAPI_ConstructsValidHierarchy()
        {
            var root = FlexBuilder.Create("FluentRoot")
                .AsColumn()
                .WithGap(10f)
                .WithPadding(12f)
                .WithJustifyContent(JustifyContent.Center)
                .WithAlignItems(AlignItems.Stretch)
                .AttachTo(_canvasRect);

            root.AddChild("Header")
                .WithSize(200f, 40f)
                .WithFlex(grow: 0f, shrink: 0f);

            root.AddChild("Content")
                .WithSize(200f, 100f)
                .WithFlex(grow: 1f, shrink: 1f);

            Assert.IsNotNull(root.Container);
            Assert.AreEqual(FlexDirection.Column, root.Container.Direction);
            Assert.AreEqual(JustifyContent.Center, root.Container.JustifyContent);
            Assert.AreEqual(AlignItems.Stretch, root.Container.AlignItems);
            Assert.AreEqual(2, root.RectTransform.childCount);

            var contentItem = root.RectTransform.GetChild(1).GetComponent<FlexItem>();
            Assert.IsNotNull(contentItem);
            Assert.AreEqual(1f, contentItem.FlexGrow);
        }

        [Test]
        public void FlexContainer_FitToContent_ResizesRectTransform()
        {
            var containerObj = new GameObject("FitContainer", typeof(RectTransform), typeof(FlexContainer));
            containerObj.transform.SetParent(_canvasRect, false);
            var container = containerObj.GetComponent<FlexContainer>();
            container.Direction = FlexDirection.Row;
            container.Padding = new FlexOffsets(10f);
            container.ColumnGap = 15f;
            container.FitToContentWidth = true;
            container.FitToContentHeight = true;

            var child1 = new GameObject("C1", typeof(RectTransform));
            child1.transform.SetParent(container.RectTransform, false);
            child1.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 60f);

            var child2 = new GameObject("C2", typeof(RectTransform));
            child2.transform.SetParent(container.RectTransform, false);
            child2.GetComponent<RectTransform>().sizeDelta = new Vector2(120f, 40f);

            container.CalculateLayoutInputHorizontal();
            container.CalculateLayoutInputVertical();
            container.SetLayoutHorizontal();
            container.SetLayoutVertical();

            // Expected width: 10 (pad L) + 100 + 15 (gap) + 120 + 10 (pad R) = 255.
            // Expected height: 10 (pad T) + max(60, 40) + 10 (pad B) = 80.
            Assert.AreEqual(255f, container.RectTransform.rect.width, 0.1f);
            Assert.AreEqual(80f, container.RectTransform.rect.height, 0.1f);
        }
    }
}
