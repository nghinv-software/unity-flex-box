using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CanvasFlexbox
{
    /// <summary>
    /// Flexbox layout container for Unity UI Canvas.
    /// Manages child RectTransforms according to W3C CSS Flexbox rules with zero GC allocations during steady-state updates.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Layout/Flex Container", 101)]
    public class FlexContainer : UIBehaviour, ILayoutGroup, ILayoutSelfController, ILayoutElement
    {
        private struct ChildEntry
        {
            public RectTransform Rect;
            public FlexItem Item;
            public ILayoutIgnorer Ignorer;
        }

        [Header("Flex Layout")]
        [SerializeField] private FlexDirection _direction = FlexDirection.Row;
        [SerializeField] private FlexWrap _wrap = FlexWrap.NoWrap;
        [SerializeField] private JustifyContent _justifyContent = JustifyContent.FlexStart;
        [SerializeField] private AlignItems _alignItems = AlignItems.FlexStart;
        [SerializeField] private AlignContent _alignContent = AlignContent.FlexStart;

        [Header("Spacing")]
        [SerializeField] private FlexOffsets _padding = FlexOffsets.Zero;
        [SerializeField] private float _rowGap = 0f;
        [SerializeField] private float _columnGap = 0f;

        [Header("Self Sizing")]
        [Tooltip("If true, automatically adjusts container RectTransform width to wrap child contents.")]
        [SerializeField] private bool _fitToContentWidth = false;

        [Tooltip("If true, automatically adjusts container RectTransform height to wrap child contents.")]
        [SerializeField] private bool _fitToContentHeight = false;

        private RectTransform _rectTransform;
        public RectTransform RectTransform => _rectTransform != null ? _rectTransform : (_rectTransform = GetComponent<RectTransform>());

        private DrivenRectTransformTracker _tracker;

        // Pooled cache and node structures to prevent runtime GC allocations
        private readonly List<ChildEntry> _childEntries = new List<ChildEntry>(16);
        private readonly List<RectTransform> _activeChildren = new List<RectTransform>(16);
        private readonly List<FlexNode> _childNodes = new List<FlexNode>(16);
        private readonly FlexNode _rootNode = new FlexNode();

        private bool _childCacheDirty = true;
        private bool _isCalculatingLayout = false;

        private float _minWidth;
        private float _preferredWidth;
        private float _minHeight;
        private float _preferredHeight;

        public FlexDirection Direction
        {
            get => _direction;
            set { if (_direction != value) { _direction = value; SetDirty(); } }
        }

        public FlexWrap Wrap
        {
            get => _wrap;
            set { if (_wrap != value) { _wrap = value; SetDirty(); } }
        }

        public JustifyContent JustifyContent
        {
            get => _justifyContent;
            set { if (_justifyContent != value) { _justifyContent = value; SetDirty(); } }
        }

        public AlignItems AlignItems
        {
            get => _alignItems;
            set { if (_alignItems != value) { _alignItems = value; SetDirty(); } }
        }

        public AlignContent AlignContent
        {
            get => _alignContent;
            set { if (_alignContent != value) { _alignContent = value; SetDirty(); } }
        }

        public FlexOffsets Padding
        {
            get => _padding;
            set { if (_padding != value) { _padding = value; SetDirty(); } }
        }

        public float RowGap
        {
            get => _rowGap;
            set { if (!Mathf.Approximately(_rowGap, value)) { _rowGap = Mathf.Max(0f, value); SetDirty(); } }
        }

        public float ColumnGap
        {
            get => _columnGap;
            set { if (!Mathf.Approximately(_columnGap, value)) { _columnGap = Mathf.Max(0f, value); SetDirty(); } }
        }

        public bool FitToContentWidth
        {
            get => _fitToContentWidth;
            set { if (_fitToContentWidth != value) { _fitToContentWidth = value; SetDirty(); } }
        }

        public bool FitToContentHeight
        {
            get => _fitToContentHeight;
            set { if (_fitToContentHeight != value) { _fitToContentHeight = value; SetDirty(); } }
        }

        // ILayoutElement implementation
        public float minWidth => _minWidth;
        public float preferredWidth => _preferredWidth;
        public float flexibleWidth => -1f;
        public float minHeight => _minHeight;
        public float preferredHeight => _preferredHeight;
        public float flexibleHeight => -1f;
        public int layoutPriority => 0;

        public void InvalidateChildCache()
        {
            _childCacheDirty = true;
            SetDirty();
        }

        public void SetDirty()
        {
            if (!IsActive() || _isCalculatingLayout) return;
            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _childCacheDirty = true;
            SetDirty();
        }

        protected override void OnDisable()
        {
            _tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (_isCalculatingLayout) return;
            SetDirty();
        }

        protected void OnTransformChildrenChanged()
        {
            _childCacheDirty = true;
            SetDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _rowGap = Mathf.Max(0f, _rowGap);
            _columnGap = Mathf.Max(0f, _columnGap);
            _childCacheDirty = true;
            SetDirty();
        }
#endif

        private void CollectActiveChildren()
        {
            int childCount = RectTransform.childCount;

            if (_childCacheDirty || _childEntries.Count != childCount)
            {
                _childEntries.Clear();
                for (int i = 0; i < childCount; i++)
                {
                    var child = RectTransform.GetChild(i) as RectTransform;
                    if (child == null) continue;

                    _childEntries.Add(new ChildEntry
                    {
                        Rect = child,
                        Item = child.GetComponent<FlexItem>(),
                        Ignorer = child.GetComponent<ILayoutIgnorer>()
                    });
                }
                _childCacheDirty = false;
            }

            _activeChildren.Clear();
            for (int i = 0; i < _childEntries.Count; i++)
            {
                var entry = _childEntries[i];
                if (entry.Rect == null || !entry.Rect.gameObject.activeInHierarchy) continue;
                if (entry.Ignorer != null && entry.Ignorer.ignoreLayout) continue;

                _activeChildren.Add(entry.Rect);
            }
        }

        private void BuildNodeTree(float availableWidth, float availableHeight)
        {
            CollectActiveChildren();

            _rootNode.Width = availableWidth;
            _rootNode.Height = availableHeight;
            _rootNode.Direction = _direction;
            _rootNode.Wrap = _wrap;
            _rootNode.JustifyContent = _justifyContent;
            _rootNode.AlignItems = _alignItems;
            _rootNode.AlignContent = _alignContent;
            _rootNode.Padding = _padding;
            _rootNode.RowGap = _rowGap;
            _rootNode.ColumnGap = _columnGap;
            _rootNode.ClearChildren();

            int activeCount = _activeChildren.Count;
            while (_childNodes.Count < activeCount)
            {
                _childNodes.Add(new FlexNode());
            }

            for (int i = 0; i < activeCount; i++)
            {
                var child = _activeChildren[i];
                var flexItem = child.GetComponent<FlexItem>();
                var node = _childNodes[i];

                node.Width = child.rect.width;
                node.Height = child.rect.height;
                node.Tag = child;

                if (flexItem != null)
                {
                    node.FlexGrow = flexItem.FlexGrow;
                    node.FlexShrink = flexItem.FlexShrink;
                    node.FlexBasis = flexItem.FlexBasis;
                    node.AlignSelf = flexItem.AlignSelf;
                    node.Margin = flexItem.Margin;
                    node.MinWidth = flexItem.MinWidth;
                    node.MinHeight = flexItem.MinHeight;
                    node.MaxWidth = flexItem.MaxWidth;
                    node.MaxHeight = flexItem.MaxHeight;
                }
                else
                {
                    node.FlexGrow = 0f;
                    node.FlexShrink = 1f;
                    node.FlexBasis = FlexLength.Auto;
                    node.AlignSelf = AlignSelf.Auto;
                    node.Margin = FlexOffsets.Zero;
                    node.MinWidth = 0f;
                    node.MinHeight = 0f;
                    node.MaxWidth = float.PositiveInfinity;
                    node.MaxHeight = float.PositiveInfinity;
                }

                _rootNode.AddChild(node);
            }
        }

        public void CalculateLayoutInputHorizontal()
        {
            float currentWidth = RectTransform.rect.width;
            float currentHeight = RectTransform.rect.height;

            BuildNodeTree(currentWidth, currentHeight);

            var (min, pref) = FlexLayoutSolver.MeasureContentSize(_rootNode);
            _minWidth = min.x;
            _preferredWidth = pref.x;
        }

        public void CalculateLayoutInputVertical()
        {
            var (min, pref) = FlexLayoutSolver.MeasureContentSize(_rootNode);
            _minHeight = min.y;
            _preferredHeight = pref.y;
        }

        public void SetLayoutHorizontal()
        {
            _tracker.Clear();

            float currentWidth = RectTransform.rect.width;
            float currentHeight = RectTransform.rect.height;

            if (_fitToContentWidth)
            {
                currentWidth = _preferredWidth;
                _isCalculatingLayout = true;
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
                _isCalculatingLayout = false;
            }

            BuildNodeTree(currentWidth, currentHeight);
            FlexLayoutSolver.CalculateLayout(_rootNode, currentWidth, currentHeight);

            for (int i = 0; i < _activeChildren.Count; i++)
            {
                var node = _childNodes[i];
                var child = _activeChildren[i];

                _tracker.Add(this, child,
                    DrivenTransformProperties.AnchorMin |
                    DrivenTransformProperties.AnchorMax |
                    DrivenTransformProperties.AnchoredPositionX |
                    DrivenTransformProperties.SizeDeltaX);

                child.anchorMin = new Vector2(0f, 1f);
                child.anchorMax = new Vector2(0f, 1f);

                float posX = node.LayoutX + (node.LayoutWidth * child.pivot.x);
                child.anchoredPosition = new Vector2(posX, child.anchoredPosition.y);
                child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, node.LayoutWidth);
            }
        }

        public void SetLayoutVertical()
        {
            float currentHeight = RectTransform.rect.height;

            if (_fitToContentHeight)
            {
                currentHeight = _preferredHeight;
                _isCalculatingLayout = true;
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentHeight);
                _isCalculatingLayout = false;
            }

            for (int i = 0; i < _activeChildren.Count; i++)
            {
                var node = _childNodes[i];
                var child = _activeChildren[i];

                _tracker.Add(this, child,
                    DrivenTransformProperties.AnchoredPositionY |
                    DrivenTransformProperties.SizeDeltaY);

                float posY = -(node.LayoutY + (node.LayoutHeight * (1f - child.pivot.y)));
                child.anchoredPosition = new Vector2(child.anchoredPosition.x, posY);
                child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, node.LayoutHeight);
            }
        }
    }
}
