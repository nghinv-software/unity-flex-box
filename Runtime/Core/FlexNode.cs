using System.Collections.Generic;

namespace CanvasFlexbox
{
    /// <summary>
    /// Pure C# tree node representing an element in the Flexbox layout hierarchy.
    /// Free of direct engine dependencies for fast, decoupled calculation and testing.
    /// </summary>
    public class FlexNode
    {
        // Dimensions
        public float Width { get; set; } = 0f;
        public float Height { get; set; } = 0f;
        public float MinWidth { get; set; } = 0f;
        public float MinHeight { get; set; } = 0f;
        public float MaxWidth { get; set; } = float.PositiveInfinity;
        public float MaxHeight { get; set; } = float.PositiveInfinity;

        // Container properties
        public FlexDirection Direction { get; set; } = FlexDirection.Row;
        public FlexWrap Wrap { get; set; } = FlexWrap.NoWrap;
        public JustifyContent JustifyContent { get; set; } = JustifyContent.FlexStart;
        public AlignItems AlignItems { get; set; } = AlignItems.FlexStart;
        public AlignContent AlignContent { get; set; } = AlignContent.FlexStart;
        public FlexOffsets Padding { get; set; } = FlexOffsets.Zero;
        public float RowGap { get; set; } = 0f;
        public float ColumnGap { get; set; } = 0f;

        // Item properties
        public float FlexGrow { get; set; } = 0f;
        public float FlexShrink { get; set; } = 1f;
        public FlexLength FlexBasis { get; set; } = FlexLength.Auto;
        public AlignSelf AlignSelf { get; set; } = AlignSelf.Auto;
        public FlexOffsets Margin { get; set; } = FlexOffsets.Zero;

        // Hierarchy
        public List<FlexNode> Children { get; } = new List<FlexNode>();
        public FlexNode Parent { get; internal set; }
        public object Tag { get; set; }

        // Layout outputs (computed by FlexLayoutSolver)
        public float LayoutX { get; internal set; }
        public float LayoutY { get; internal set; }
        public float LayoutWidth { get; internal set; }
        public float LayoutHeight { get; internal set; }

        public bool IsRow => Direction == FlexDirection.Row || Direction == FlexDirection.RowReverse;
        public bool IsColumn => Direction == FlexDirection.Column || Direction == FlexDirection.ColumnReverse;
        public bool IsReverse => Direction == FlexDirection.RowReverse || Direction == FlexDirection.ColumnReverse;

        public void AddChild(FlexNode child)
        {
            if (child == null) return;
            child.Parent = this;
            Children.Add(child);
        }

        public bool RemoveChild(FlexNode child)
        {
            if (child == null) return false;
            if (Children.Remove(child))
            {
                child.Parent = null;
                return true;
            }
            return false;
        }

        public void ClearChildren()
        {
            foreach (var child in Children)
            {
                child.Parent = null;
            }
            Children.Clear();
        }
    }
}
