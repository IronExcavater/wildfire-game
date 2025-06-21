using System;
using UnityEngine;

namespace Editor.CssRect
{
    public enum ResolveMode
    {
        Inner,
        Outer
    }

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

        public (float top, float right, float bottom, float left) Resolve(Vector2 size, ResolveMode mode = ResolveMode.Inner)
        {
            return (
                Top.Resolve(size.y, mode),
                Right.Resolve(size.x, mode),
                Bottom.Resolve(size.y, mode),
                Left.Resolve(size.x, mode)
            );
        }

        public Rect ApplyTo(Rect rect, ResolveMode mode = ResolveMode.Inner)
        {
            var insets = Resolve(rect.size, mode);
            return new Rect(
                Math.Max(0, rect.position.x + insets.left),
                Math.Max(0, rect.position.y + insets.top),
                Math.Max(0, rect.size.x - insets.left - insets.right),
                Math.Max(0, rect.size.y - insets.top - insets.bottom)
            );
        }
    }
}
