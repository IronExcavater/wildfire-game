using Editor.Utilities;
using UnityEngine;

namespace Editor.CssRect
{
    public readonly struct BoxVec2
    {
        public readonly BoxValue X;
        public readonly BoxValue Y;

        public BoxVec2(float gap = 0)
        {
            X = new BoxValue(gap);
            Y = new BoxValue(gap);
        }

        public BoxVec2(float horizontal = 0, float vertical = 0)
        {
            X = new BoxValue(horizontal);
            Y = new BoxValue(vertical);
        }

        public Vector2 Resolve(Vector2 size)
        {
            return new Vector2(
                X.Resolve(size.x),
                Y.Resolve(size.y)
            );
        }
    }
}
