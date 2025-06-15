using UnityEngine;

namespace Editor.CssRect
{
    public readonly struct BoxInsets
    {
        public readonly BoxValue Top;
        public readonly BoxValue Right;
        public readonly BoxValue Bottom;
        public readonly BoxValue Left;

        public BoxInsets(BoxValue all = default)
        {
            Top = Right = Bottom = Left = all;
        }

        public BoxInsets(BoxValue vertical = default, BoxValue horizontal = default)
        {
            Top = Bottom = vertical;
            Right = Left = horizontal;
        }

        public BoxInsets(BoxValue top = default, BoxValue right = default, BoxValue bottom = default, BoxValue left = default)
        {
            Top = top;
            Right = right;
            Bottom = bottom;
            Left = left;
        }

        public Vector4 Resolve(Rect relativeTo, bool isInverse = false)
        {
            return new Vector4(
                Top.Resolve(relativeTo.height, isInverse),
                Right.Resolve(relativeTo.width, isInverse),
                Bottom.Resolve(relativeTo.height, isInverse),
                Left.Resolve(relativeTo.width, isInverse)
            );
        }

        public Rect ApplyTo(Rect rect, bool isInverse = false)
        {
            var insets = Resolve(rect, isInverse);
            return new Rect(
                rect.x + insets.w,
                rect.y + insets.x,
                rect.width - insets.w - insets.y,
                rect.height - insets.x - insets.z
            );
        }
    }
}
