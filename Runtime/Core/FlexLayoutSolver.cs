using System;
using System.Collections.Generic;
using UnityEngine;

namespace CanvasFlexbox
{
    /// <summary>
    /// Pure C# high-performance W3C-compliant Flexbox layout solver.
    /// Operates on FlexNode trees with zero native DLL dependencies and 0 B GC allocation per layout solve.
    /// </summary>
    public static class FlexLayoutSolver
    {
        private struct FlexItemContext
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

            public readonly float TotalMarginMain => MarginMainStart + MarginMainEnd;
            public readonly float TotalMarginCross => MarginCrossStart + MarginCrossEnd;
            public readonly float OuterMainSize => TargetMainSize + TotalMarginMain;
        }

        private struct FlexLine
        {
            public int StartIndex;
            public int Count;
            public float CrossSize;
        }

        // Static scratch buffers to guarantee 0 B GC allocation during layout updates
        private static readonly List<FlexItemContext> s_Items = new List<FlexItemContext>(64);
        private static readonly List<FlexLine> s_Lines = new List<FlexLine>(16);
        private static readonly Queue<FlexNode> s_NodeQueue = new Queue<FlexNode>(16);

        /// <summary>
        /// Solves the layout for the specified root node and its descendants iteratively.
        /// Fully zero-allocation after initial static buffer capacity is warmed up.
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

            s_NodeQueue.Clear();
            s_NodeQueue.Enqueue(root);

            while (s_NodeQueue.Count > 0)
            {
                var current = s_NodeQueue.Dequeue();
                if (current.Children.Count == 0) continue;

                SolveSingleNode(current);

                for (int i = 0; i < current.Children.Count; i++)
                {
                    var child = current.Children[i];
                    if (child.Children.Count > 0)
                    {
                        s_NodeQueue.Enqueue(child);
                    }
                }
            }

            // Clear static references to avoid memory retention
            s_Items.Clear();
            s_Lines.Clear();
            s_NodeQueue.Clear();
        }

        private static void SolveSingleNode(FlexNode container)
        {
            s_Items.Clear();
            s_Lines.Clear();

            bool isRow = container.IsRow;
            float containerMain = isRow ? container.LayoutWidth : container.LayoutHeight;
            float containerCross = isRow ? container.LayoutHeight : container.LayoutWidth;

            float mainPadStart = isRow ? container.Padding.Left : container.Padding.Top;
            float mainPadEnd = isRow ? container.Padding.Right : container.Padding.Bottom;
            float crossPadStart = isRow ? container.Padding.Top : container.Padding.Left;
            float crossPadEnd = isRow ? container.Padding.Bottom : container.Padding.Right;

            float mainGap = isRow ? container.ColumnGap : container.RowGap;
            float crossGap = isRow ? container.RowGap : container.ColumnGap;

            float innerMain = Mathf.Max(0f, containerMain - (mainPadStart + mainPadEnd));
            float innerCross = Mathf.Max(0f, containerCross - (crossPadStart + crossPadEnd));

            // Step 1: Collect Item Contexts into flat buffer
            for (int i = 0; i < container.Children.Count; i++)
            {
                var child = container.Children[i];
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
                s_Items.Add(ctx);
            }

            // Step 2: Line Breaking (Wrap) using zero-allocation slice structs
            if (container.Wrap == FlexWrap.NoWrap || s_Items.Count == 0)
            {
                s_Lines.Add(new FlexLine { StartIndex = 0, Count = s_Items.Count, CrossSize = 0f });
            }
            else
            {
                int lineStart = 0;
                int lineCount = 0;
                float currentLineMain = 0f;

                for (int i = 0; i < s_Items.Count; i++)
                {
                    var item = s_Items[i];
                    float itemOuter = item.HypoMainSize + item.TotalMarginMain;
                    float neededSpace = (lineCount > 0 ? mainGap : 0f) + itemOuter;

                    if (lineCount > 0 && currentLineMain + neededSpace > innerMain)
                    {
                        s_Lines.Add(new FlexLine { StartIndex = lineStart, Count = lineCount, CrossSize = 0f });
                        lineStart = i;
                        lineCount = 0;
                        currentLineMain = 0f;
                        neededSpace = itemOuter;
                    }

                    lineCount++;
                    currentLineMain += neededSpace;
                }

                if (lineCount > 0)
                {
                    s_Lines.Add(new FlexLine { StartIndex = lineStart, Count = lineCount, CrossSize = 0f });
                }
            }

            // Step 3: Resolve Main Sizes (Grow & Shrink) and Line Cross Sizes
            for (int l = 0; l < s_Lines.Count; l++)
            {
                var line = s_Lines[l];
                float totalHypoMain = 0f;
                float totalGrow = 0f;
                float totalWeightedShrink = 0f;

                for (int i = line.StartIndex; i < line.StartIndex + line.Count; i++)
                {
                    var item = s_Items[i];
                    totalHypoMain += item.HypoMainSize + item.TotalMarginMain;
                    totalGrow += item.Node.FlexGrow;
                    totalWeightedShrink += item.Node.FlexShrink * item.HypoMainSize;
                }

                float totalGaps = Mathf.Max(0, line.Count - 1) * mainGap;
                float freeSpace = innerMain - (totalHypoMain + totalGaps);

                if (freeSpace > 0f && totalGrow > 0f)
                {
                    float distributable = totalGrow < 1f ? freeSpace * totalGrow : freeSpace;
                    for (int i = line.StartIndex; i < line.StartIndex + line.Count; i++)
                    {
                        var item = s_Items[i];
                        if (item.Node.FlexGrow > 0f)
                        {
                            float share = distributable * (item.Node.FlexGrow / totalGrow);
                            item.TargetMainSize = Mathf.Clamp(item.HypoMainSize + share, item.MinMain, item.MaxMain);
                            s_Items[i] = item;
                        }
                    }
                }
                else if (freeSpace < 0f && totalWeightedShrink > 0f)
                {
                    float deficit = -freeSpace;
                    for (int i = line.StartIndex; i < line.StartIndex + line.Count; i++)
                    {
                        var item = s_Items[i];
                        if (item.Node.FlexShrink > 0f)
                        {
                            float weight = item.Node.FlexShrink * item.HypoMainSize;
                            float reduction = deficit * (weight / totalWeightedShrink);
                            item.TargetMainSize = Mathf.Clamp(item.HypoMainSize - reduction, item.MinMain, item.MaxMain);
                            s_Items[i] = item;
                        }
                    }
                }

                // Calculate Cross Size of the line
                float lineMaxCross = 0f;
                for (int i = line.StartIndex; i < line.StartIndex + line.Count; i++)
                {
                    var item = s_Items[i];
                    float itemCross = item.HypoCrossSize + item.TotalMarginCross;
                    if (itemCross > lineMaxCross)
                    {
                        lineMaxCross = itemCross;
                    }
                }

                line.CrossSize = lineMaxCross;
                s_Lines[l] = line;
            }

            // Single line stretch fallback if container has available cross space
            if (s_Lines.Count == 1 && container.Wrap == FlexWrap.NoWrap && container.AlignItems == AlignItems.Stretch)
            {
                if (innerCross > s_Lines[0].CrossSize)
                {
                    var line0 = s_Lines[0];
                    line0.CrossSize = innerCross;
                    s_Lines[0] = line0;
                }
            }

            // Step 4: Cross Axis Lines Distribution (AlignContent)
            float totalLinesCross = 0f;
            for (int i = 0; i < s_Lines.Count; i++)
            {
                totalLinesCross += s_Lines[i].CrossSize;
            }
            float totalCrossGaps = Mathf.Max(0, s_Lines.Count - 1) * crossGap;
            float remainingCross = innerCross - (totalLinesCross + totalCrossGaps);

            float crossStartOffset = 0f;
            float extraCrossGap = 0f;

            if (s_Lines.Count > 0)
            {
                switch (container.AlignContent)
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
                        extraCrossGap = s_Lines.Count > 1 && remainingCross > 0f ? remainingCross / (s_Lines.Count - 1) : 0f;
                        break;
                    case AlignContent.SpaceAround:
                        float unitAround = remainingCross > 0f ? remainingCross / s_Lines.Count : 0f;
                        crossStartOffset = unitAround / 2f;
                        extraCrossGap = unitAround;
                        break;
                    case AlignContent.Stretch:
                        if (remainingCross > 0f)
                        {
                            float extraPerLine = remainingCross / s_Lines.Count;
                            for (int l = 0; l < s_Lines.Count; l++)
                            {
                                var line = s_Lines[l];
                                line.CrossSize += extraPerLine;
                                s_Lines[l] = line;
                            }
                        }
                        crossStartOffset = 0f;
                        extraCrossGap = 0f;
                        break;
                }
            }

            // Step 5: Justify Content (Main Axis) & Align Items (Cross Axis)
            float currentLineCross;
            bool isWrapReverse = container.Wrap == FlexWrap.WrapReverse;

            if (isWrapReverse)
            {
                currentLineCross = containerCross - crossPadEnd - crossStartOffset;
            }
            else
            {
                currentLineCross = crossPadStart + crossStartOffset;
            }

            for (int l = 0; l < s_Lines.Count; l++)
            {
                var line = s_Lines[l];
                float lineActualMain = 0f;
                for (int i = line.StartIndex; i < line.StartIndex + line.Count; i++)
                {
                    lineActualMain += s_Items[i].OuterMainSize;
                }
                lineActualMain += Mathf.Max(0, line.Count - 1) * mainGap;
                float freeMain = innerMain - lineActualMain;

                float mainOffset = 0f;
                float extraMainGap = 0f;

                if (freeMain > 0f)
                {
                    switch (container.JustifyContent)
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
                            extraMainGap = line.Count > 1 ? freeMain / (line.Count - 1) : 0f;
                            break;
                        case JustifyContent.SpaceAround:
                            float unitAround = freeMain / line.Count;
                            mainOffset = unitAround / 2f;
                            extraMainGap = unitAround;
                            break;
                        case JustifyContent.SpaceEvenly:
                            float unitEvenly = freeMain / (line.Count + 1);
                            mainOffset = unitEvenly;
                            extraMainGap = unitEvenly;
                            break;
                    }
                }

                // Position items on main axis and cross axis
                float currentMain = mainPadStart + mainOffset;
                float lineTop = isWrapReverse ? currentLineCross - line.CrossSize : currentLineCross;

                for (int i = line.StartIndex; i < line.StartIndex + line.Count; i++)
                {
                    var item = s_Items[i];
                    item.MainPos = currentMain + item.MarginMainStart;
                    currentMain += item.MarginMainStart + item.TargetMainSize + item.MarginMainEnd + mainGap + extraMainGap;

                    AlignItems effectiveAlign = item.Node.AlignSelf switch
                    {
                        AlignSelf.FlexStart => AlignItems.FlexStart,
                        AlignSelf.FlexEnd => AlignItems.FlexEnd,
                        AlignSelf.Center => AlignItems.Center,
                        AlignSelf.Stretch => AlignItems.Stretch,
                        _ => container.AlignItems
                    };

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

                    s_Items[i] = item;
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

            // Step 6: Map to final Layout Coordinates on child nodes
            bool isReverse = container.IsReverse;
            for (int i = 0; i < s_Items.Count; i++)
            {
                var item = s_Items[i];
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
