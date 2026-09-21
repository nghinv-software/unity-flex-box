using System;
using UnityEngine;

namespace CanvasFlexbox
{
    /// <summary>
    /// Represents 4-directional offsets used for Margins and Paddings.
    /// </summary>
    [Serializable]
    public struct FlexOffsets : IEquatable<FlexOffsets>
    {
        [SerializeField] public float left;
        [SerializeField] public float right;
        [SerializeField] public float top;
        [SerializeField] public float bottom;

        public float Left => left;
        public float Right => right;
        public float Top => top;
        public float Bottom => bottom;

        public float Horizontal => left + right;
        public float Vertical => top + bottom;

        public static readonly FlexOffsets Zero = new FlexOffsets(0, 0, 0, 0);

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

        public bool Equals(FlexOffsets other)
        {
            return Mathf.Approximately(left, other.left) &&
                   Mathf.Approximately(right, other.right) &&
                   Mathf.Approximately(top, other.top) &&
                   Mathf.Approximately(bottom, other.bottom);
        }

        public override bool Equals(object obj)
        {
            return obj is FlexOffsets other && Equals(other);
        }

        public override int GetHashCode()
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

        public override string ToString()
        {
            return $"FlexOffsets(L:{left}, R:{right}, T:{top}, B:{bottom})";
        }
    }
}
