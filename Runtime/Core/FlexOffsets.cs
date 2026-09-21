using System;
using UnityEngine;

namespace CanvasFlexbox
{
    /// <summary>
    /// Represents 4-directional offsets used for Margins and Paddings.
    /// Optimized as an immutable-friendly value type with readonly members.
    /// </summary>
    [Serializable]
    public struct FlexOffsets : IEquatable<FlexOffsets>
    {
        [SerializeField] public float left;
        [SerializeField] public float right;
        [SerializeField] public float top;
        [SerializeField] public float bottom;

        public readonly float Left => left;
        public readonly float Right => right;
        public readonly float Top => top;
        public readonly float Bottom => bottom;

        public readonly float Horizontal => left + right;
        public readonly float Vertical => top + bottom;

        public static readonly FlexOffsets Zero = new FlexOffsets(0f, 0f, 0f, 0f);

        public FlexOffsets(float all)
        {
            left = right = top = bottom = all;
        }

        public FlexOffsets(float horizontal, float vertical)
        {
            left = right = horizontal;
            top = bottom = vertical;
        }

        public FlexOffsets(float left, float right, float top, float bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        public readonly bool Equals(FlexOffsets other)
        {
            return Mathf.Approximately(left, other.left) &&
                   Mathf.Approximately(right, other.right) &&
                   Mathf.Approximately(top, other.top) &&
                   Mathf.Approximately(bottom, other.bottom);
        }

        public override readonly bool Equals(object obj)
        {
            return obj is FlexOffsets other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            unchecked
            {
                int hash = left.GetHashCode();
                hash = (hash * 397) ^ right.GetHashCode();
                hash = (hash * 397) ^ top.GetHashCode();
                hash = (hash * 397) ^ bottom.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(FlexOffsets left, FlexOffsets right) => left.Equals(right);
        public static bool operator !=(FlexOffsets left, FlexOffsets right) => !left.Equals(right);

        public override readonly string ToString()
        {
            return $"FlexOffsets(L:{left}, R:{right}, T:{top}, B:{bottom})";
        }
    }
}
