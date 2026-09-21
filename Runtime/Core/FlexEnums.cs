namespace CanvasFlexbox
{
    /// <summary>
    /// Defines the direction of the main axis along which flex items are laid out.
    /// </summary>
    public enum FlexDirection
    {
        /// <summary>
        /// Items are laid out horizontally from left to right.
        /// </summary>
        Row = 0,

        /// <summary>
        /// Items are laid out horizontally from right to left.
        /// </summary>
        RowReverse = 1,

        /// <summary>
        /// Items are laid out vertically from top to bottom.
        /// </summary>
        Column = 2,

        /// <summary>
        /// Items are laid out vertically from bottom to top.
        /// </summary>
        ColumnReverse = 3
    }

    /// <summary>
    /// Defines whether flex items are forced into a single line or can be wrapped onto multiple lines.
    /// </summary>
    public enum FlexWrap
    {
        /// <summary>
        /// All flex items will be on one line.
        /// </summary>
        NoWrap = 0,

        /// <summary>
        /// Flex items will wrap onto multiple lines, from top to bottom (in row) or left to right (in column).
        /// </summary>
        Wrap = 1,

        /// <summary>
        /// Flex items will wrap onto multiple lines in reverse order.
        /// </summary>
        WrapReverse = 2
    }

    /// <summary>
    /// Defines how the browser distributes space between and around content items along the main-axis of a flex container.
    /// </summary>
    public enum JustifyContent
    {
        /// <summary>
        /// Items are packed toward the start of the flex direction.
        /// </summary>
        FlexStart = 0,

        /// <summary>
        /// Items are packed toward the end of the flex direction.
        /// </summary>
        FlexEnd = 1,

        /// <summary>
        /// Items are centered along the line.
        /// </summary>
        Center = 2,

        /// <summary>
        /// Items are evenly distributed in the line; first item is at start, last item at end.
        /// </summary>
        SpaceBetween = 3,

        /// <summary>
        /// Items are evenly distributed in the line with equal space around them.
        /// </summary>
        SpaceAround = 4,

        /// <summary>
        /// Items are distributed so that the spacing between any two items (and space to the edges) is equal.
        /// </summary>
        SpaceEvenly = 5
    }

    /// <summary>
    /// Defines how flex items are aligned along the cross axis of the current line.
    /// </summary>
    public enum AlignItems
    {
        /// <summary>
        /// Items are aligned to the start of the cross axis.
        /// </summary>
        FlexStart = 0,

        /// <summary>
        /// Items are aligned to the end of the cross axis.
        /// </summary>
        FlexEnd = 1,

        /// <summary>
        /// Items are centered in the cross axis.
        /// </summary>
        Center = 2,

        /// <summary>
        /// Items are stretched to fill the cross axis of the line.
        /// </summary>
        Stretch = 3
    }

    /// <summary>
    /// Allows the default alignment (specified by align-items) to be overridden for individual flex items.
    /// </summary>
    public enum AlignSelf
    {
        /// <summary>
        /// Inherits the align-items value from parent container.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Align to the start of the cross axis.
        /// </summary>
        FlexStart = 1,

        /// <summary>
        /// Align to the end of the cross axis.
        /// </summary>
        FlexEnd = 2,

        /// <summary>
        /// Align to the center of the cross axis.
        /// </summary>
        Center = 3,

        /// <summary>
        /// Stretch to fill the cross axis.
        /// </summary>
        Stretch = 4
    }

    /// <summary>
    /// Defines how flex lines are distributed along the cross-axis when extra space is available in multi-line wrap mode.
    /// </summary>
    public enum AlignContent
    {
        /// <summary>
        /// Lines packed toward the start of the container.
        /// </summary>
        FlexStart = 0,

        /// <summary>
        /// Lines packed toward the end of the container.
        /// </summary>
        FlexEnd = 1,

        /// <summary>
        /// Lines centered in the container.
        /// </summary>
        Center = 2,

        /// <summary>
        /// Lines evenly distributed; first line is at start, last line at end.
        /// </summary>
        SpaceBetween = 3,

        /// <summary>
        /// Lines evenly distributed with equal space around each line.
        /// </summary>
        SpaceAround = 4,

        /// <summary>
        /// Lines stretch to take up the remaining space.
        /// </summary>
        Stretch = 5
    }
}
