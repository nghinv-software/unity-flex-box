using System;
using UnityEngine;

namespace CanvasFlexbox
{
    public enum FlexUnit
    {
        Auto = 0,
        Pixel = 1,
        Percent = 2
    }

    /// <summary>
    /// Represents a length value that can be Auto, Pixel, or Percent.
    /// Optimized as an immutable-friendly value type with readonly members.
    /// </summary>
    [Serializable]
    public struct FlexLength : IEquatable<FlexLength>
    {
        [SerializeField] private FlexUnit _unit;
        [SerializeField] private float _value;

        public readonly FlexUnit Unit => _unit;
        public readonly float Value => _value;
        public readonly bool IsAuto => _unit == FlexUnit.Auto;
        public readonly bool IsPixel => _unit == FlexUnit.Pixel;
        public readonly bool IsPercent => _unit == FlexUnit.Percent;

        public static FlexLength Auto => new FlexLength(FlexUnit.Auto, 0f);

        public static FlexLength Pixels(float px) => new FlexLength(FlexUnit.Pixel, px);

        public static FlexLength Percent(float percent) => new FlexLength(FlexUnit.Percent, percent);

        public FlexLength(FlexUnit unit, float value)
        {
            _unit = unit;
            _value = value;
        }

        public readonly float Resolve(float parentDimension, float fallbackValue = 0f)
        {
            return _unit switch
            {
                FlexUnit.Auto => fallbackValue,
                FlexUnit.Pixel => _value,
                FlexUnit.Percent => parentDimension * (_value / 100f),
                _ => fallbackValue
            };
        }

        public readonly bool Equals(FlexLength other)
        {
            return _unit == other._unit && Mathf.Approximately(_value, other._value);
        }

        public override readonly bool Equals(object obj)
        {
            return obj is FlexLength other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            unchecked
            {
                return ((int)_unit * 397) ^ _value.GetHashCode();
            }
        }

        public static bool operator ==(FlexLength a, FlexLength b) => a.Equals(b);
        public static bool operator !=(FlexLength a, FlexLength b) => !a.Equals(b);

        public static implicit operator FlexLength(float pixels) => Pixels(pixels);

        public override readonly string ToString()
        {
            return _unit switch
            {
                FlexUnit.Auto => "auto",
                FlexUnit.Pixel => $"{_value}px",
                FlexUnit.Percent => $"{_value}%",
                _ => $"{_value}"
            };
        }
    }
}
