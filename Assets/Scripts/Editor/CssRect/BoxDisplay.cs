using System;
using UnityEditor;
using UnityEngine;

namespace Editor.CssRect
{
    public enum BoxDisplay
    {
        Block,
        Inline,
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
                        width = Math.Max(width, child.BoundsSize.Value.x);
                        height += child.BoundsSize.Value.y;
                    }
                    break;
                case BoxDisplay.Inline:
                    var lineWidth = 0f;
                    var lineHeight = 0f;

                    foreach (var child in children)
                    {
                        var childSize = child.BoundsSize.Value;
                        if (lineWidth + childSize.x > target.MaxWidth.Value && lineWidth > 0f)
                        {
                            width = Math.Max(width, lineWidth);
                            height += lineHeight;

                            lineWidth = 0f;
                            lineHeight = 0f;
                        }

                        lineWidth += childSize.x;
                        lineHeight = Math.Max(lineHeight, childSize.y);
                    }

                    width = Math.Max(width, lineWidth);
                    height += lineHeight;
                    break;
            }

            var rect = new Rect(new(), new Vector2(width, height));
            var padding = target.Padding.Value.ApplyTo(rect, ResolveMode.Outer);
            var align = target.Align.Value.ApplyTo(rect, ResolveMode.Outer);
            var margin = target.Margin.Value.ApplyTo(new Rect(align, padding.size), ResolveMode.Outer);

            return margin.size;
        }

        public static Vector2 SizeFromProperty(this BoxDisplay boxDisplay, BoxRect target)
        {

            var rect = new Rect(target.RectPosition.Value,
                new(target.RectSize.Value.x, EditorGUI.GetPropertyHeight(target.Property.Value, true)
            ));
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

            switch (parent.Display.Value)
            {
                case BoxDisplay.Block:
                    foreach (var sibling in siblings)
                    {
                        if (sibling == target) break;
                        y += sibling.BoundsSize.Value.y;
                    }
                    break;
                case BoxDisplay.Inline:
                    var lineWidth = 0f;
                    var lineHeight = 0f;

                    foreach (var sibling in siblings)
                    {
                        if (sibling == target) break;

                        var siblingSize = sibling.BoundsSize.Value;

                        if (lineWidth + siblingSize.x > parent.RectSize.Value.x && lineWidth > 0f)
                        {
                            y += lineHeight;

                            lineWidth = 0f;
                            lineHeight = 0f;
                        }

                        lineWidth += siblingSize.x;
                        lineHeight = Mathf.Max(lineHeight, siblingSize.y);
                    }

                    x += lineWidth;
                    break;
            }

            return new Vector2(x, y);
        }

        public static float MaxWidthFromParent(this BoxDisplay boxDisplay, BoxRect target)
        {
            var rect = new Rect(new(), new(target.Parent.Value.MaxWidth.Value, 0));

            var margin = target.Margin.Value.ApplyTo(rect);
            var align = target.Align.Value.ApplyTo(margin);
            var padding = target.Padding.Value.ApplyTo(new Rect(align, margin.size));

            return padding.width;
        }
    }
}
