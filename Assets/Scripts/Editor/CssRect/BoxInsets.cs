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

        public Vector4 Resolve(Vector2 size, ResolveMode mode = ResolveMode.Inner)
        {
            return new Vector4(
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
                rect.position.x + insets.w,
                rect.position.y + insets.x,
                rect.size.x - insets.w - insets.y,
                rect.size.y - insets.x - insets.z
            );
        }
    }
}
