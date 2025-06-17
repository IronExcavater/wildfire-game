using UnityEngine;

namespace Editor.CssRect
{
    public struct BoxAlign
    {
        public readonly BoxValue Horizontal;
        public readonly BoxValue Vertical;

        public BoxAlign(float horizontal, float vertical)
        {
            Horizontal = new(horizontal, Unit.Percent);
            Vertical = new(vertical, Unit.Percent);
        }

        public Vector2 ApplyTo(Rect rect, ResolveMode mode = ResolveMode.Inner)
        {
            return new Vector2(
                rect.x + Horizontal.Resolve(rect.width, mode),
                rect.y + Vertical.Resolve(rect.height, mode)
            );
        }

        public Vector2 Anchor => new(Horizontal.Resolve(1), Vertical.Resolve(1));
    }
}
