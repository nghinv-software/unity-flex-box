using System;
using UnityEngine;

namespace CanvasFlexbox
{
    /// <summary>
    /// Extension methods for quick Flexbox setup on GameObjects and RectTransforms.
    /// </summary>
    public static class FlexExtensions
    {
        public static FlexContainer AddFlexContainer(this GameObject gameObject,
            FlexDirection direction = FlexDirection.Row,
            JustifyContent justify = JustifyContent.FlexStart,
            AlignItems align = AlignItems.FlexStart)
        {
            if (gameObject == null) return null;
            var container = gameObject.GetComponent<FlexContainer>() ?? gameObject.AddComponent<FlexContainer>();
            container.Direction = direction;
            container.JustifyContent = justify;
            container.AlignItems = align;
            return container;
        }

        public static FlexContainer AddFlexContainer(this RectTransform rectTransform,
            FlexDirection direction = FlexDirection.Row,
            JustifyContent justify = JustifyContent.FlexStart,
            AlignItems align = AlignItems.FlexStart)
        {
            return rectTransform != null ? rectTransform.gameObject.AddFlexContainer(direction, justify, align) : null;
        }

        public static FlexItem AddFlexItem(this GameObject gameObject, float grow = 0f, float shrink = 1f)
        {
            if (gameObject == null) return null;
            var item = gameObject.GetComponent<FlexItem>() ?? gameObject.AddComponent<FlexItem>();
            item.FlexGrow = grow;
            item.FlexShrink = shrink;
            return item;
        }

        public static FlexItem AddFlexItem(this RectTransform rectTransform, float grow = 0f, float shrink = 1f)
        {
            return rectTransform != null ? rectTransform.gameObject.AddFlexItem(grow, shrink) : null;
        }

        public static FlexBuilder ToFlexBuilder(this GameObject gameObject)
        {
            return new FlexBuilder(gameObject);
        }

        public static FlexBuilder ToFlexBuilder(this RectTransform rectTransform)
        {
            return rectTransform != null ? new FlexBuilder(rectTransform.gameObject) : null;
        }
    }
}
