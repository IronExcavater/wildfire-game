using UnityEngine;

namespace Editor.CssRect
{
    public enum BoxDisplay
    {
        None,
        Block,
        // Inline,
        // Flex,
        // Grid,
        // Absolute,
        // Relative,
    }

    public static class BoxDisplayExtensions
    {
        public static Vector2 SizeFromChildren(this BoxDisplay boxDisplay, BoxRect target)
        {
            var width = 0f;
            var height = 0f;

            var children = target.Children.Value;

            switch (boxDisplay)
            {
                case BoxDisplay.Block:
                    foreach (var child in children)
                    {
                        width = Mathf.Max(width, child.BoundsSize.Value.x);
                        height += child.BoundsSize.Value.y;
                    }
                    break;
            }

            var rect = new Rect(new(), new Vector2(width, height));
            var padding = target.Padding.Value.ApplyTo(rect, ResolveMode.Outer);
            var align = target.Align.Value.ApplyTo(rect, ResolveMode.Outer);
            var margin = target.Margin.Value.ApplyTo(new Rect(align, padding.size), ResolveMode.Outer);

            return margin.size;
        }

        public static Vector2 PositionFromParent(this BoxDisplay boxDisplay, BoxRect target)
        {
            var parent = target.Parent.Value;
            var siblings = parent.Children.Value;

            var x = parent.RectPosition.Value.x;
            var y = parent.RectPosition.Value.y;

            switch (boxDisplay)
            {
                case BoxDisplay.Block:
                    foreach (var sibling in siblings)
                    {
                        if (sibling == target) break;
                        y += sibling.BoundsSize.Value.y;
                    }
                    break;
            }

            return new Vector2(x, y);
        }
    }
}
