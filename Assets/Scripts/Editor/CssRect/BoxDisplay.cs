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
    }

    public enum BoxPosition
    {
        Static,
        Relative,
        Absolute
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
                        if (child.Position.Value == BoxPosition.Absolute) continue;

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
                        if (child.Position.Value == BoxPosition.Absolute) continue;

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

            var basePos = parent.RectPosition.Value;

            if (target.Position.Value == BoxPosition.Absolute)
                return basePos + target.Offset.Value.Resolve(parent.RectSize.Value);

            var gap = parent.Gap.Value.Resolve(parent.ContainerSize.Value);
            var offset = Vector2.zero;

            switch (parent.Display.Value)
            {
                case BoxDisplay.Block:
                    for (var i = 0; i < siblings.Count; i++)
                    {
                        var sibling = siblings[i];
                        var siblingSize = sibling.ContainerSize.Value;

                        if (sibling == target) break;

                        offset.y += siblingSize.y;
                        if (i < siblings.Count - 1) offset.y += gap.y;
                    }
                    break;
                case BoxDisplay.Inline:
                    var lineWidth = 0f;
                    var lineHeight = 0f;

                    for (var i = 0; i < siblings.Count; i++)
                    {
                        var sibling = siblings[i];
                        var siblingSize = sibling.ContainerSize.Value;

                        if (sibling == target) break;

                        if (lineWidth + siblingSize.x > parent.RectSize.Value.x && lineWidth > 0f)
                        {
                            offset.y += lineHeight + gap.y;

                            lineWidth = 0f;
                            lineHeight = 0f;
                        }
                        else
                            lineWidth += gap.x;

                        lineWidth += siblingSize.x;
                        lineHeight = Math.Max(lineHeight, siblingSize.y);
                    }

                    offset.x += lineWidth;
                    break;
            }

            if (target.Position.Value == BoxPosition.Relative)
                offset += target.Offset.Value.Resolve(parent.RectSize.Value);

            return basePos + offset;
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

            return new Vector2(width, height);
        }
    }
}
