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
        public static Vector2 BoundsSizeFromChildren(this BoxDisplay boxDisplay, BoxRect target)
        {
            var width = 0f;
            var height = 0f;

            var children = target.Children.Value;
            var gap = target.Gap.Value.Resolve(target.BoundsSize.Value);

            switch (boxDisplay)
            {
                case BoxDisplay.Block:
                    for (var i = 0; i < children.Count; i++)
                    {
                        var child = children[i];

                        width = Math.Max(width, child.BoundsSize.Value.x);
                        height += child.BoundsSize.Value.y;

                        if (i < children.Count - 1)
                            height += gap.y;
                    }
                    break;
                case BoxDisplay.Inline:
                    var lineWidth = 0f;
                    var lineHeight = 0f;

                    for (var i = 0; i < children.Count; i++)
                    {
                        var child = children[i];

                        var childSize = child.BoundsSize.Value;
                        if (lineWidth + childSize.x > target.ContainerSize.Value.x && lineWidth > 0f)
                        {
                            width = Math.Max(width, lineWidth);
                            height += lineHeight + gap.y;

                            lineWidth = 0f;
                            lineHeight = 0f;
                        }

                        if (lineWidth > 0f)
                            lineWidth += gap.x;

                        lineWidth += childSize.x;
                        lineHeight = Math.Max(lineHeight, childSize.y);
                    }

                    width = Math.Max(width, lineWidth);
                    height += lineHeight;
                    break;
            }

            var rect = new Rect(new(), new Vector2(width, height));
            var padding = target.Padding.Value.ApplyTo(rect, ResolveMode.Outer);
            return padding.size;
        }

        public static Vector2 BoundsSizeFromProperty(this BoxDisplay boxDisplay, BoxRect target)
        {
            var propertyHeight = EditorGUI.GetPropertyHeight(target.Property.Value, true);
            var rect = new Rect(target.RectPosition.Value,
                new( target.RectSize.Value.x, propertyHeight));
            var padding = target.Padding.Value.ApplyTo(rect, ResolveMode.Outer);
            return padding.size;
        }

        public static Vector2 ContainerPositionFromParent(this BoxDisplay boxDisplay, BoxRect target)
        {
            var parent = target.Parent.Value;
            var siblings = parent.Children.Value;

            var x = parent.RectPosition.Value.x;
            var y = parent.RectPosition.Value.y;

            var gap = parent.Gap.Value.Resolve(parent.ContainerSize.Value);

            switch (parent.Display.Value)
            {
                case BoxDisplay.Block:
                    for (var i = 0; i < siblings.Count; i++)
                    {
                        var sibling = siblings[i];
                        if (sibling == target) break;

                        y += sibling.ContainerSize.Value.y;
                        if (i < siblings.Count - 1) y += gap.y;
                    }
                    break;
                case BoxDisplay.Inline:
                    var lineWidth = 0f;
                    var lineHeight = 0f;

                    for (var i = 0; i < siblings.Count; i++)
                    {
                        var sibling = siblings[i];
                        if (sibling == target) break;

                        var siblingSize = sibling.ContainerSize.Value;

                        if (lineWidth + siblingSize.x > parent.RectSize.Value.x && lineWidth > 0f)
                        {
                            y += lineHeight + gap.y;

                            lineWidth = 0f;
                            lineHeight = 0f;
                        }

                        if (lineWidth > 0f)
                            lineWidth += gap.x;

                        lineWidth += siblingSize.x;
                        lineHeight = Math.Max(lineHeight, siblingSize.y);
                    }

                    x += lineWidth;
                    break;
            }

            return new Vector2(x, y);
        }

        public static Vector2 ContainerSizeFromParent(this BoxDisplay boxDisplay, BoxRect target)
        {
            var width = 0f;
            var height = 0f;

            var parent = target.Parent.Value;

            switch (parent.Display.Value)
            {
                case BoxDisplay.Block:
                    width = Math.Max(parent.RectSize.Value.x, target.MinSize.Value.x);
                    height = Math.Max(target.BoundsSize.Value.y, target.MinSize.Value.y);
                    break;
                case BoxDisplay.Inline:
                    width = Math.Max(target.BoundsSize.Value.x, target.MinSize.Value.x);
                    height = Math.Max(target.BoundsSize.Value.y, target.MinSize.Value.y);
                    break;
            }

            var rect = new Rect(new(), new Vector2(width, height));
            var margin = target.Margin.Value.ApplyTo(rect);

            return margin.size;
        }
    }
}
