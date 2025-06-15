using UnityEngine;

namespace Editor.CssRect
{
    public enum Align
    {
        Start = 0,
        Center = 50,
        End = 100,
        Stretch = 50
    }

    public static class AlignExtensions
    {
        public static Rect ApplyToHorizontal(this Align align, Rect rect, float contentWidth)
        {
            return ApplyToAxis(align, rect, rect.x, rect.width, contentWidth, true);
        }

        public static Rect ApplyToVertical(this Align align, Rect rect, float contentHeight)
        {
            return ApplyToAxis(align, rect, rect.y, rect.height, contentHeight, false);
        }

        private static Rect ApplyToAxis(Align align, Rect rect, float origin, float containerSize, float contentSize, bool isHorizontal)
        {
            var start = align switch
            {
                Align.Center => origin + (containerSize - contentSize) / 2f,
                Align.End => origin + containerSize - contentSize,
                _ => origin
            };

            var size = align == Align.Stretch ? containerSize : contentSize;

            return isHorizontal
                ? new Rect(start, rect.y, size, rect.height)
                : new Rect(rect.x, start, rect.width, size);
        }
    }

    public struct BoxAlign
    {
        public Align HAlign;
        public Align VAlign;

        public BoxAlign(Align hAlign = Align.Start, Align vAlign = Align.Start)
        {
            HAlign = hAlign;
            VAlign = vAlign;
        }

        public Rect ApplyTo(Rect rect)
        {
            var hAlignedRect = HAlign.ApplyToHorizontal(rect, rect.width);
            var vAlignedRect = VAlign.ApplyToVertical(hAlignedRect, rect.height);
            return vAlignedRect;
        }

        public Vector2 Anchor => new((int)HAlign/100f, (int)VAlign/100f);
    }
}
