using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CanvasFlexbox
{
    /// <summary>
    /// Flexbox layout container for Unity UI Canvas.
    /// Manages child RectTransforms according to CSS Flexbox rules.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Layout/Flex Container", 101)]
    public class FlexContainer : UIBehaviour, ILayoutGroup, ILayoutSelfController, ILayoutElement
    {
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
        private readonly List<RectTransform> _activeChildren = new List<RectTransform>();
        private readonly List<FlexNode> _childNodes = new List<FlexNode>();
        private FlexNode _rootNode;

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

        public void SetDirty()
        {
            if (!IsActive()) return;
            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
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
            SetDirty();
        }

        protected void OnTransformChildrenChanged()
        {
            SetDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _rowGap = Mathf.Max(0f, _rowGap);
            _columnGap = Mathf.Max(0f, _columnGap);
            SetDirty();
        }
#endif

        private void CollectActiveChildren()
        {
            _activeChildren.Clear();
            for (int i = 0; i < RectTransform.childCount; i++)
            {
                var child = RectTransform.GetChild(i) as RectTransform;
                if (child == null || !child.gameObject.activeInHierarchy) continue;

                var ignorer = child.GetComponent<ILayoutIgnorer>();
                if (ignorer != null && ignorer.ignoreLayout) continue;

                _activeChildren.Add(child);
            }
        }

        private void BuildNodeTree(float availableWidth, float availableHeight)
        {
            CollectActiveChildren();

            _rootNode = new FlexNode
            {
                Width = availableWidth,
                Height = availableHeight,
                Direction = _direction,
                Wrap = _wrap,
                JustifyContent = _justifyContent,
                AlignItems = _alignItems,
                AlignContent = _alignContent,
                Padding = _padding,
                RowGap = _rowGap,
                ColumnGap = _columnGap
            };

            _childNodes.Clear();

            for (int i = 0; i < _activeChildren.Count; i++)
            {
                var child = _activeChildren[i];
                var flexItem = child.GetComponent<FlexItem>();

                var node = new FlexNode
                {
                    Width = child.rect.width,
                    Height = child.rect.height,
                    Tag = child
                };

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

                _childNodes.Add(node);
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
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
            }

            if (_rootNode == null || _rootNode.Children.Count != _activeChildren.Count)
            {
                BuildNodeTree(currentWidth, currentHeight);
            }
            else
            {
                _rootNode.Width = currentWidth;
                _rootNode.Height = currentHeight;
            }

            FlexLayoutSolver.CalculateLayout(_rootNode, currentWidth, currentHeight);

            for (int i = 0; i < _childNodes.Count; i++)
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
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentHeight);
            }

            for (int i = 0; i < _childNodes.Count; i++)
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
