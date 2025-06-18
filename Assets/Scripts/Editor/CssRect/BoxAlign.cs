using System;
using UnityEngine;

namespace Editor.CssRect
{
    public readonly struct BoxAlign
    {
        public readonly BoxValue Horizontal;
        public readonly BoxValue Vertical;

        public BoxAlign(float horizontal = 0f, float vertical = 0f)
        {
            Horizontal = new(Math.Clamp(horizontal, 0, 100), Unit.Percent);
            Vertical = new(Math.Clamp(vertical, 0, 100), Unit.Percent);
        }

        public Vector2 ApplyTo(Rect container, Vector2 contentSize, ResolveMode mode = ResolveMode.Inner)
        {
            return new Vector2(
                container.x + Horizontal.Resolve(container.width, mode) - contentSize.x * Anchor.x,
                container.y + Vertical.Resolve(container.height, mode) - contentSize.y * Anchor.y
            );
        }

        public Vector2 Anchor => new(Horizontal.Resolve(1), Vertical.Resolve(1));
    }
}
