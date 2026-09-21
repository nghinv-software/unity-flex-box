using System;
using UnityEngine;
using UnityEngine.UI;

namespace CanvasFlexbox
{
    /// <summary>
    /// Fluent builder for programmatically composing Canvas UI hierarchies using Flexbox.
    /// </summary>
    public class FlexBuilder
    {
        private readonly GameObject _gameObject;
        private readonly RectTransform _rectTransform;
        private FlexContainer _container;
        private FlexItem _item;

        public GameObject GameObject => _gameObject;
        public RectTransform RectTransform => _rectTransform;
        public FlexContainer Container => _container;
        public FlexItem Item => _item;

        public FlexBuilder(string name = "FlexContainer")
        {
            _gameObject = new GameObject(name, typeof(RectTransform));
            _rectTransform = _gameObject.GetComponent<RectTransform>();
        }

        public FlexBuilder(GameObject existingGameObject)
        {
            _gameObject = existingGameObject ?? throw new ArgumentNullException(nameof(existingGameObject));
            _rectTransform = _gameObject.GetComponent<RectTransform>();
            if (_rectTransform == null)
            {
                _rectTransform = _gameObject.AddComponent<RectTransform>();
            }
            _container = _gameObject.GetComponent<FlexContainer>();
            _item = _gameObject.GetComponent<FlexItem>();
        }

        public static FlexBuilder Create(string name = "FlexElement")
        {
            return new FlexBuilder(name);
        }

        private FlexContainer EnsureContainer()
        {
            if (_container == null)
            {
                _container = _gameObject.GetComponent<FlexContainer>();
                if (_container == null)
                {
                    _container = _gameObject.AddComponent<FlexContainer>();
                }
            }
            return _container;
        }

        private FlexItem EnsureItem()
        {
            if (_item == null)
            {
                _item = _gameObject.GetComponent<FlexItem>();
                if (_item == null)
                {
                    _item = _gameObject.AddComponent<FlexItem>();
                }
            }
            return _item;
        }

        #region Container Methods

        public FlexBuilder AsRow()
        {
            EnsureContainer().Direction = FlexDirection.Row;
            return this;
        }

        public FlexBuilder AsRowReverse()
        {
            EnsureContainer().Direction = FlexDirection.RowReverse;
            return this;
        }

        public FlexBuilder AsColumn()
        {
            EnsureContainer().Direction = FlexDirection.Column;
            return this;
        }

        public FlexBuilder AsColumnReverse()
        {
            EnsureContainer().Direction = FlexDirection.ColumnReverse;
            return this;
        }

        public FlexBuilder WithWrap(FlexWrap wrap)
        {
            EnsureContainer().Wrap = wrap;
            return this;
        }

        public FlexBuilder WithJustifyContent(JustifyContent justify)
        {
            EnsureContainer().JustifyContent = justify;
            return this;
        }

        public FlexBuilder WithAlignItems(AlignItems align)
        {
            EnsureContainer().AlignItems = align;
            return this;
        }

        public FlexBuilder WithAlignContent(AlignContent align)
        {
            EnsureContainer().AlignContent = align;
            return this;
        }

        public FlexBuilder WithPadding(float all)
        {
            EnsureContainer().Padding = new FlexOffsets(all);
            return this;
        }

        public FlexBuilder WithPadding(float horizontal, float vertical)
        {
            EnsureContainer().Padding = new FlexOffsets(horizontal, vertical);
            return this;
        }

        public FlexBuilder WithPadding(float left, float right, float top, float bottom)
        {
            EnsureContainer().Padding = new FlexOffsets(left, right, top, bottom);
            return this;
        }

        public FlexBuilder WithGap(float gap)
        {
            var c = EnsureContainer();
            c.RowGap = gap;
            c.ColumnGap = gap;
            return this;
        }

        public FlexBuilder WithGap(float columnGap, float rowGap)
        {
            var c = EnsureContainer();
            c.ColumnGap = columnGap;
            c.RowGap = rowGap;
            return this;
        }

        public FlexBuilder FitToContent(bool width = true, bool height = true)
        {
            var c = EnsureContainer();
            c.FitToContentWidth = width;
            c.FitToContentHeight = height;
            return this;
        }

        #endregion

        #region Item Methods

        public FlexBuilder WithFlex(float grow, float shrink = 1f)
        {
            var item = EnsureItem();
            item.FlexGrow = grow;
            item.FlexShrink = shrink;
            return this;
        }

        public FlexBuilder WithFlexBasis(FlexLength basis)
        {
            EnsureItem().FlexBasis = basis;
            return this;
        }

        public FlexBuilder WithFlexBasis(float pixels)
        {
            EnsureItem().FlexBasis = FlexLength.Pixels(pixels);
            return this;
        }

        public FlexBuilder WithAlignSelf(AlignSelf align)
        {
            EnsureItem().AlignSelf = align;
            return this;
        }

        public FlexBuilder WithMargin(float all)
        {
            EnsureItem().Margin = new FlexOffsets(all);
            return this;
        }

        public FlexBuilder WithMargin(float horizontal, float vertical)
        {
            EnsureItem().Margin = new FlexOffsets(horizontal, vertical);
            return this;
        }

        public FlexBuilder WithMargin(float left, float right, float top, float bottom)
        {
            EnsureItem().Margin = new FlexOffsets(left, right, top, bottom);
            return this;
        }

        public FlexBuilder WithSize(float width, float height)
        {
            _rectTransform.sizeDelta = new Vector2(width, height);
            return this;
        }

        public FlexBuilder WithMinWidth(float minW)
        {
            EnsureItem().MinWidth = minW;
            return this;
        }

        public FlexBuilder WithMinHeight(float minH)
        {
            EnsureItem().MinHeight = minH;
            return this;
        }

        #endregion

        #region Hierarchy & Building

        public FlexBuilder AttachTo(Transform parent, bool worldPositionStays = false)
        {
            _rectTransform.SetParent(parent, worldPositionStays);
            return this;
        }

        public FlexBuilder AddChild(string name, Action<FlexBuilder> configure = null)
        {
            var child = new FlexBuilder(name);
            child.AttachTo(_rectTransform);
            configure?.Invoke(child);
            return child;
        }

        public FlexBuilder AddChild(GameObject childObject, Action<FlexBuilder> configure = null)
        {
            var child = new FlexBuilder(childObject);
            child.AttachTo(_rectTransform);
            configure?.Invoke(child);
            return child;
        }

        #endregion
    }
}
