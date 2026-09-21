using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CanvasFlexbox
{
    /// <summary>
    /// Component attached to children of a FlexContainer to configure individual flex properties,
    /// such as flex-grow, flex-shrink, flex-basis, align-self, and margins.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Layout/Flex Item", 102)]
    public class FlexItem : UIBehaviour, ILayoutIgnorer
    {
        [Header("Flex Properties")]
        [Tooltip("Defines how much the item will grow relative to other items when positive free space is available.")]
        [SerializeField] private float _flexGrow = 0f;

        [Tooltip("Defines how much the item will shrink relative to other items when negative free space (deficit) occurs.")]
        [SerializeField] private float _flexShrink = 1f;

        [Tooltip("Initial main size of the item before remaining space is distributed. Default is Auto (uses RectTransform size).")]
        [SerializeField] private FlexLength _flexBasis = FlexLength.Auto;

        [Header("Alignment Override")]
        [Tooltip("Overrides align-items property of the parent FlexContainer for this individual item.")]
        [SerializeField] private AlignSelf _alignSelf = AlignSelf.Auto;

        [Header("Spacing")]
        [Tooltip("Margins applied around this item.")]
        [SerializeField] private FlexOffsets _margin = FlexOffsets.Zero;

        [Header("Constraints")]
        [SerializeField] private float _minWidth = 0f;
        [SerializeField] private float _minHeight = 0f;
        [SerializeField] private float _maxWidth = float.PositiveInfinity;
        [SerializeField] private float _maxHeight = float.PositiveInfinity;

        [Header("Ignore")]
        [SerializeField] private bool _ignoreLayout = false;

        private RectTransform _rectTransform;
        public RectTransform RectTransform => _rectTransform != null ? _rectTransform : (_rectTransform = GetComponent<RectTransform>());

        public float FlexGrow
        {
            get => _flexGrow;
            set
            {
                if (!Mathf.Approximately(_flexGrow, value))
                {
                    _flexGrow = Mathf.Max(0f, value);
                    SetDirty();
                }
            }
        }

        public float FlexShrink
        {
            get => _flexShrink;
            set
            {
                if (!Mathf.Approximately(_flexShrink, value))
                {
                    _flexShrink = Mathf.Max(0f, value);
                    SetDirty();
                }
            }
        }

        public FlexLength FlexBasis
        {
            get => _flexBasis;
            set
            {
                if (_flexBasis != value)
                {
                    _flexBasis = value;
                    SetDirty();
                }
            }
        }

        public AlignSelf AlignSelf
        {
            get => _alignSelf;
            set
            {
                if (_alignSelf != value)
                {
                    _alignSelf = value;
                    SetDirty();
                }
            }
        }

        public FlexOffsets Margin
        {
            get => _margin;
            set
            {
                if (_margin != value)
                {
                    _margin = value;
                    SetDirty();
                }
            }
        }

        public float MinWidth
        {
            get => _minWidth;
            set { _minWidth = value; SetDirty(); }
        }

        public float MinHeight
        {
            get => _minHeight;
            set { _minHeight = value; SetDirty(); }
        }

        public float MaxWidth
        {
            get => _maxWidth;
            set { _maxWidth = value; SetDirty(); }
        }

        public float MaxHeight
        {
            get => _maxHeight;
            set { _maxHeight = value; SetDirty(); }
        }

        public bool ignoreLayout
        {
            get => _ignoreLayout;
            set
            {
                if (_ignoreLayout != value)
                {
                    _ignoreLayout = value;
                    SetDirty();
                }
            }
        }

        public void SetDirty()
        {
            if (!IsActive()) return;

            var parentContainer = GetComponentInParent<FlexContainer>();
            if (parentContainer != null)
            {
                parentContainer.SetDirty();
            }
            else
            {
                LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            SetDirty();
            base.OnDisable();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _flexGrow = Mathf.Max(0f, _flexGrow);
            _flexShrink = Mathf.Max(0f, _flexShrink);
            _minWidth = Mathf.Max(0f, _minWidth);
            _minHeight = Mathf.Max(0f, _minHeight);
            _maxWidth = Mathf.Max(_minWidth, _maxWidth);
            _maxHeight = Mathf.Max(_minHeight, _maxHeight);
            SetDirty();
        }
#endif
    }
}
