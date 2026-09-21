using System;
using System.Collections.Generic;
using UnityEngine;

namespace CanvasFlexbox
{
    /// <summary>
    /// Pure C# high-performance W3C-compliant Flexbox layout solver.
    /// Operates on FlexNode trees with zero native DLL dependencies.
    /// </summary>
    public static class FlexLayoutSolver
    {
        private class FlexItemContext
        {
            public FlexNode Node;
            public float HypoMainSize;
            public float HypoCrossSize;
            public float TargetMainSize;
            public float TargetCrossSize;
            public float MarginMainStart;
            public float MarginMainEnd;
            public float MarginCrossStart;
            public float MarginCrossEnd;
            public float MinMain;
            public float MaxMain;
            public float MinCross;
            public float MaxCross;
            public float MainPos;
            public float CrossPos;

            public float TotalMarginMain => MarginMainStart + MarginMainEnd;
            public float TotalMarginCross => MarginCrossStart + MarginCrossEnd;
            public float OuterMainSize => TargetMainSize + TotalMarginMain;
        }

        private class FlexLine
        {
            public readonly List<FlexItemContext> Items = new List<FlexItemContext>();
            public float CrossSize;
        }

        /// <summary>
        /// Solves the layout for the specified root node and its children recursively.
        /// </summary>
        /// <param name="root">The root flex node.</param>
        /// <param name="availableWidth">Available container width (e.g. from RectTransform).</param>
        /// <param name="availableHeight">Available container height (e.g. from RectTransform).</param>
        public static void CalculateLayout(FlexNode root, float availableWidth, float availableHeight)
        {
            if (root == null) return;

            float rootWidth = root.Width > 0f ? root.Width : availableWidth;
            float rootHeight = root.Height > 0f ? root.Height : availableHeight;

            root.LayoutWidth = Mathf.Clamp(rootWidth, root.MinWidth, root.MaxWidth);
            root.LayoutHeight = Mathf.Clamp(rootHeight, root.MinHeight, root.MaxHeight);

            if (root.Children.Count == 0) return;

            bool isRow = root.IsRow;
            float containerMain = isRow ? root.LayoutWidth : root.LayoutHeight;
            float containerCross = isRow ? root.LayoutHeight : root.LayoutWidth;

            float mainPadStart = isRow ? root.Padding.Left : root.Padding.Top;
            float mainPadEnd = isRow ? root.Padding.Right : root.Padding.Bottom;
            float crossPadStart = isRow ? root.Padding.Top : root.Padding.Left;
            float crossPadEnd = isRow ? root.Padding.Bottom : root.Padding.Right;

            float mainGap = isRow ? root.ColumnGap : root.RowGap;
            float crossGap = isRow ? root.RowGap : root.ColumnGap;

            float innerMain = Mathf.Max(0f, containerMain - (mainPadStart + mainPadEnd));
            float innerCross = Mathf.Max(0f, containerCross - (crossPadStart + crossPadEnd));

            // Step 1: Collect Item Contexts
            var items = new List<FlexItemContext>(root.Children.Count);
            foreach (var child in root.Children)
            {
                var ctx = new FlexItemContext { Node = child };

                if (isRow)
                {
                    ctx.MarginMainStart = child.Margin.Left;
                    ctx.MarginMainEnd = child.Margin.Right;
                    ctx.MarginCrossStart = child.Margin.Top;
                    ctx.MarginCrossEnd = child.Margin.Bottom;

                    float baseMain = child.FlexBasis.IsAuto
                        ? child.Width
                        : child.FlexBasis.Resolve(innerMain, child.Width);

                    ctx.MinMain = child.MinWidth;
                    ctx.MaxMain = child.MaxWidth;
                    ctx.MinCross = child.MinHeight;
                    ctx.MaxCross = child.MaxHeight;

                    ctx.HypoMainSize = Mathf.Clamp(baseMain, ctx.MinMain, ctx.MaxMain);
                    ctx.HypoCrossSize = Mathf.Clamp(child.Height, ctx.MinCross, ctx.MaxCross);
                }
                else
                {
                    ctx.MarginMainStart = child.Margin.Top;
                    ctx.MarginMainEnd = child.Margin.Bottom;
                    ctx.MarginCrossStart = child.Margin.Left;
                    ctx.MarginCrossEnd = child.Margin.Right;

                    float baseMain = child.FlexBasis.IsAuto
                        ? child.Height
                        : child.FlexBasis.Resolve(innerMain, child.Height);

                    ctx.MinMain = child.MinHeight;
                    ctx.MaxMain = child.MaxHeight;
                    ctx.MinCross = child.MinWidth;
                    ctx.MaxCross = child.MaxWidth;

                    ctx.HypoMainSize = Mathf.Clamp(baseMain, ctx.MinMain, ctx.MaxMain);
                    ctx.HypoCrossSize = Mathf.Clamp(child.Width, ctx.MinCross, ctx.MaxCross);
                }

                ctx.TargetMainSize = ctx.HypoMainSize;
                ctx.TargetCrossSize = ctx.HypoCrossSize;
                items.Add(ctx);
            }

            // Step 2: Line Breaking (Wrap)
            var lines = new List<FlexLine>();
            if (root.Wrap == FlexWrap.NoWrap || items.Count == 0)
            {
                var singleLine = new FlexLine();
                singleLine.Items.AddRange(items);
                lines.Add(singleLine);
            }
            else
            {
                var currentLine = new FlexLine();
                float currentLineMain = 0f;

                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    float itemOuter = item.HypoMainSize + item.TotalMarginMain;
                    float neededSpace = (currentLine.Items.Count > 0 ? mainGap : 0f) + itemOuter;

                    if (currentLine.Items.Count > 0 && currentLineMain + neededSpace > innerMain)
                    {
                        lines.Add(currentLine);
                        currentLine = new FlexLine();
                        currentLineMain = 0f;
                        neededSpace = itemOuter;
                    }

                    currentLine.Items.Add(item);
                    currentLineMain += neededSpace;
                }

                if (currentLine.Items.Count > 0)
                {
                    lines.Add(currentLine);
                }
            }

            // Step 3: Resolve Main Sizes (Grow & Shrink)
            foreach (var line in lines)
            {
                float totalHypoMain = 0f;
                float totalGrow = 0f;
                float totalWeightedShrink = 0f;

                for (int i = 0; i < line.Items.Count; i++)
                {
                    var item = line.Items[i];
                    totalHypoMain += item.HypoMainSize + item.TotalMarginMain;
                    totalGrow += item.Node.FlexGrow;
                    totalWeightedShrink += item.Node.FlexShrink * item.HypoMainSize;
                }

                float totalGaps = Mathf.Max(0, line.Items.Count - 1) * mainGap;
                float freeSpace = innerMain - (totalHypoMain + totalGaps);

                if (freeSpace > 0f && totalGrow > 0f)
                {
                    float distributable = totalGrow < 1f ? freeSpace * totalGrow : freeSpace;
                    foreach (var item in line.Items)
                    {
                        if (item.Node.FlexGrow > 0f)
                        {
                            float share = distributable * (item.Node.FlexGrow / totalGrow);
                            item.TargetMainSize = Mathf.Clamp(item.HypoMainSize + share, item.MinMain, item.MaxMain);
                        }
                    }
                }
                else if (freeSpace < 0f && totalWeightedShrink > 0f)
                {
                    float deficit = -freeSpace;
                    foreach (var item in line.Items)
                    {
                        if (item.Node.FlexShrink > 0f)
                        {
                            float weight = item.Node.FlexShrink * item.HypoMainSize;
                            float reduction = deficit * (weight / totalWeightedShrink);
                            item.TargetMainSize = Mathf.Clamp(item.HypoMainSize - reduction, item.MinMain, item.MaxMain);
                        }
                    }
                }

                // Calculate Cross Size of the line
                float lineMaxCross = 0f;
                foreach (var item in line.Items)
                {
                    float itemCross = item.HypoCrossSize + item.TotalMarginCross;
                    if (itemCross > lineMaxCross)
                    {
                        lineMaxCross = itemCross;
                    }
                }

                line.CrossSize = lineMaxCross;
            }

            // Single line stretch fallback if container has available cross space
            if (lines.Count == 1 && root.Wrap == FlexWrap.NoWrap && root.AlignItems == AlignItems.Stretch)
            {
                if (innerCross > lines[0].CrossSize)
                {
                    lines[0].CrossSize = innerCross;
                }
            }

            // Step 4: Cross Axis Lines Distribution (AlignContent)
            float totalLinesCross = 0f;
            for (int i = 0; i < lines.Count; i++)
            {
                totalLinesCross += lines[i].CrossSize;
            }
            float totalCrossGaps = Mathf.Max(0, lines.Count - 1) * crossGap;
            float remainingCross = innerCross - (totalLinesCross + totalCrossGaps);

            float crossStartOffset = 0f;
            float extraCrossGap = 0f;

            if (lines.Count > 0)
            {
                switch (root.AlignContent)
                {
                    case AlignContent.FlexStart:
                        crossStartOffset = 0f;
                        extraCrossGap = 0f;
                        break;
                    case AlignContent.FlexEnd:
                        crossStartOffset = Mathf.Max(0f, remainingCross);
                        extraCrossGap = 0f;
                        break;
                    case AlignContent.Center:
                        crossStartOffset = Mathf.Max(0f, remainingCross / 2f);
                        extraCrossGap = 0f;
                        break;
                    case AlignContent.SpaceBetween:
                        crossStartOffset = 0f;
                        extraCrossGap = lines.Count > 1 && remainingCross > 0f ? remainingCross / (lines.Count - 1) : 0f;
                        break;
                    case AlignContent.SpaceAround:
                        float unitAround = remainingCross > 0f ? remainingCross / lines.Count : 0f;
                        crossStartOffset = unitAround / 2f;
                        extraCrossGap = unitAround;
                        break;
                    case AlignContent.Stretch:
                        if (remainingCross > 0f)
                        {
                            float extraPerLine = remainingCross / lines.Count;
                            foreach (var line in lines)
                            {
                                line.CrossSize += extraPerLine;
                            }
                        }
                        crossStartOffset = 0f;
                        extraCrossGap = 0f;
                        break;
                }
            }

            // Step 5: Justify Content (Main Axis) & Align Items (Cross Axis)
            float currentLineCross;
            bool isWrapReverse = root.Wrap == FlexWrap.WrapReverse;

            if (isWrapReverse)
            {
                currentLineCross = containerCross - crossPadEnd - crossStartOffset;
            }
            else
            {
                currentLineCross = crossPadStart + crossStartOffset;
            }

            foreach (var line in lines)
            {
                float lineActualMain = 0f;
                foreach (var item in line.Items)
                {
                    lineActualMain += item.OuterMainSize;
                }
                lineActualMain += Mathf.Max(0, line.Items.Count - 1) * mainGap;
                float freeMain = innerMain - lineActualMain;

                float mainOffset = 0f;
                float extraMainGap = 0f;

                if (freeMain > 0f)
                {
                    switch (root.JustifyContent)
                    {
                        case JustifyContent.FlexStart:
                            mainOffset = 0f;
                            extraMainGap = 0f;
                            break;
                        case JustifyContent.FlexEnd:
                            mainOffset = freeMain;
                            extraMainGap = 0f;
                            break;
                        case JustifyContent.Center:
                            mainOffset = freeMain / 2f;
                            extraMainGap = 0f;
                            break;
                        case JustifyContent.SpaceBetween:
                            mainOffset = 0f;
                            extraMainGap = line.Items.Count > 1 ? freeMain / (line.Items.Count - 1) : 0f;
                            break;
                        case JustifyContent.SpaceAround:
                            float unitAround = freeMain / line.Items.Count;
                            mainOffset = unitAround / 2f;
                            extraMainGap = unitAround;
                            break;
                        case JustifyContent.SpaceEvenly:
                            float unitEvenly = freeMain / (line.Items.Count + 1);
                            mainOffset = unitEvenly;
                            extraMainGap = unitEvenly;
                            break;
                    }
                }

                // Position items on main axis
                float currentMain = mainPadStart + mainOffset;
                foreach (var item in line.Items)
                {
                    item.MainPos = currentMain + item.MarginMainStart;
                    currentMain += item.MarginMainStart + item.TargetMainSize + item.MarginMainEnd + mainGap + extraMainGap;

                    // Cross axis alignment for this item
                    AlignItems effectiveAlign = item.Node.AlignSelf switch
                    {
                        AlignSelf.FlexStart => AlignItems.FlexStart,
                        AlignSelf.FlexEnd => AlignItems.FlexEnd,
                        AlignSelf.Center => AlignItems.Center,
                        AlignSelf.Stretch => AlignItems.Stretch,
                        _ => root.AlignItems
                    };

                    float lineTop = isWrapReverse ? currentLineCross - line.CrossSize : currentLineCross;
                    float availableCross = line.CrossSize - item.TotalMarginCross;

                    switch (effectiveAlign)
                    {
                        case AlignItems.FlexStart:
                            item.TargetCrossSize = item.HypoCrossSize;
                            item.CrossPos = lineTop + item.MarginCrossStart;
                            break;
                        case AlignItems.FlexEnd:
                            item.TargetCrossSize = item.HypoCrossSize;
                            item.CrossPos = lineTop + line.CrossSize - item.MarginCrossEnd - item.TargetCrossSize;
                            break;
                        case AlignItems.Center:
                            item.TargetCrossSize = item.HypoCrossSize;
                            float rem = line.CrossSize - item.TotalMarginCross - item.TargetCrossSize;
                            item.CrossPos = lineTop + item.MarginCrossStart + (rem / 2f);
                            break;
                        case AlignItems.Stretch:
                            item.TargetCrossSize = Mathf.Clamp(availableCross, item.MinCross, item.MaxCross);
                            item.CrossPos = lineTop + item.MarginCrossStart;
                            break;
                    }
                }

                if (isWrapReverse)
                {
                    currentLineCross -= (line.CrossSize + crossGap + extraCrossGap);
                }
                else
                {
                    currentLineCross += (line.CrossSize + crossGap + extraCrossGap);
                }
            }

            // Step 6: Map to final Layout Coordinates
            bool isReverse = root.IsReverse;
            foreach (var line in lines)
            {
                foreach (var item in line.Items)
                {
                    if (isRow)
                    {
                        if (isReverse)
                        {
                            item.Node.LayoutX = containerMain - mainPadEnd - (item.MainPos - mainPadStart + item.TargetMainSize);
                        }
                        else
                        {
                            item.Node.LayoutX = item.MainPos;
                        }

                        item.Node.LayoutY = item.CrossPos;
                        item.Node.LayoutWidth = item.TargetMainSize;
                        item.Node.LayoutHeight = item.TargetCrossSize;
                    }
                    else
                    {
                        item.Node.LayoutX = item.CrossPos;

                        if (isReverse)
                        {
                            item.Node.LayoutY = containerMain - mainPadEnd - (item.MainPos - mainPadStart + item.TargetMainSize);
                        }
                        else
                        {
                            item.Node.LayoutY = item.MainPos;
                        }

                        item.Node.LayoutWidth = item.TargetCrossSize;
                        item.Node.LayoutHeight = item.TargetMainSize;
                    }

                    // Recursively layout children if child is also a flex container
                    if (item.Node.Children.Count > 0)
                    {
                        CalculateLayout(item.Node, item.Node.LayoutWidth, item.Node.LayoutHeight);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates min and preferred dimensions for a container based on its content.
        /// Useful for ContentSizeFitter and ILayoutElement integrations.
        /// </summary>
        public static (Vector2 min, Vector2 preferred) MeasureContentSize(FlexNode root)
        {
            if (root == null || root.Children.Count == 0)
            {
                float minW = root != null ? root.MinWidth : 0f;
                float minH = root != null ? root.MinHeight : 0f;
                return (new Vector2(minW, minH), new Vector2(minW, minH));
            }

            bool isRow = root.IsRow;
            float mainGap = isRow ? root.ColumnGap : root.RowGap;
            float crossGap = isRow ? root.RowGap : root.ColumnGap;

            float mainPad = isRow ? root.Padding.Horizontal : root.Padding.Vertical;
            float crossPad = isRow ? root.Padding.Vertical : root.Padding.Horizontal;

            float maxSingleItemMain = 0f;
            float sumAllItemsMain = 0f;
            float maxSingleItemCross = 0f;

            for (int i = 0; i < root.Children.Count; i++)
            {
                var child = root.Children[i];
                float childMain = isRow ? child.Width + child.Margin.Horizontal : child.Height + child.Margin.Vertical;
                float childCross = isRow ? child.Height + child.Margin.Vertical : child.Width + child.Margin.Horizontal;

                if (childMain > maxSingleItemMain) maxSingleItemMain = childMain;
                sumAllItemsMain += childMain;
                if (childCross > maxSingleItemCross) maxSingleItemCross = childCross;
            }

            sumAllItemsMain += Mathf.Max(0, root.Children.Count - 1) * mainGap;

            float minMain = maxSingleItemMain + mainPad;
            float prefMain = sumAllItemsMain + mainPad;
            float minCross = maxSingleItemCross + crossPad;
            float prefCross = maxSingleItemCross + crossPad;

            Vector2 min = isRow ? new Vector2(minMain, minCross) : new Vector2(minCross, minMain);
            Vector2 pref = isRow ? new Vector2(prefMain, prefCross) : new Vector2(prefCross, prefMain);

            return (min, pref);
        }
    }
}
