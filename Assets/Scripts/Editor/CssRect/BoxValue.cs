using System.Globalization;
using UnityEngine;

namespace Editor.CssRect
{
    public readonly struct BoxValue
    {
        public float Value { get; }
        public Unit Unit { get; }

        public BoxValue(float value = 0, Unit unit = Unit.Rem)
        {
            Value = value;
            Unit = unit;
        }

        public float Resolve(float size, ResolveMode mode = ResolveMode.Inner)
        {
            if (mode == ResolveMode.Inner)
                return Unit switch
                {
                    Unit.Percent => size * Value / 100,
                    _ => Value
                };
            return Unit switch
            {
                Unit.Percent => -(size * (Value / 100) / (1 - (Value / 100))),
                _ => -Value
            };
        }
        public static implicit operator BoxValue(float value) => new(value);

        public static implicit operator BoxValue(string input)
        {
            input = input.Trim().ToLowerInvariant();
            var unit = Unit.Rem;
            var value = input;

            if (input.EndsWith("%"))
            {
                value = input[..^1];
                unit = Unit.Percent;
            }
            else if (value.EndsWith("rem")) value = value[..^3];

            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                return new BoxValue(result, unit);

            Debug.LogWarning($"Could not parse BoxValue from string: '{input}'");
            return new BoxValue();
        }

        public override string ToString() => Unit switch
        {
            Unit.Rem => $"{Value}rem",
            Unit.Percent => $"{Value}%",
            _ => $"{Value}"
        };
    }

    public enum Unit
    {
        Rem,
        Percent
    }
}
