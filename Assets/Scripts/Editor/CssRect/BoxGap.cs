using UnityEngine;

namespace Editor.CssRect
{
    public readonly struct BoxGap
    {
        public readonly BoxValue Horizontal;
        public readonly BoxValue Vertical;

        public BoxGap(float horizontal = 0, float vertical = 0)
        {
            Horizontal = new BoxValue(horizontal);
            Vertical = new BoxValue(vertical);
        }

        public Vector2 Resolve(Vector2 size)
        {
            return new Vector2(
                Horizontal.Resolve(size.x),
                Vertical.Resolve(size.y)
            );
        }
    }
}
