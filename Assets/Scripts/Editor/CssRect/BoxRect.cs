using UnityEditor;
using UnityEngine;
using Utilities;
using Utilities.Observables;

namespace Editor.CssRect
{
    public class BoxRect
    {
        public readonly Property<BoxRect> Parent = new();
        //public int ChildIndex => Parent.IsBound ? Parent.Value.Children.Value.IndexOf(this) : -1;

        public readonly Property<ObservableList<BoxRect>> Children = new(new());
        //public readonly Property<ObservableList<Property<Rect>>> ChildRects = new(new());

        public readonly Property<Rect> MaxRect = new();
        public readonly Property<Rect> Rect = new();
        public readonly Property<Rect> MinRect = new();

        public readonly Property<Vector2> MinSize = new(new(EditorGUIUtility.fieldWidth, EditorGUIUtility.singleLineHeight));
        public readonly Property<Vector2> MaxSize = new(new(float.MaxValue, float.MaxValue));

        public BoxInsets Padding;
        public BoxInsets Margin;

        public BoxDisplay Display = BoxDisplay.Block;
        public BoxAlign Align;

        public BoxRect(Property<BoxRect> parent)
        {
            InitalizeListeners();
            Parent.Bind(parent);
        }

        public BoxRect(Rect maxSize)
        {
            InitalizeListeners();
            MaxRect.Value = maxSize;
        }

        private void InitalizeListeners()
        {
            Parent.AddListener((_, oldParent, newParent) =>
            {
                //oldParent?.ChildRects.Value.RemoveAt(oldParent.Children.Value.IndexOf(this));
                oldParent?.Children.Value.Remove(this);
                newParent?.Children.Value.Add(this);
                if (newParent != null) Display.ComputeMaxRect(newParent);
                //newParent?.ChildRects.Value.Add(new());
                //if (newParent != null) MaxRect.Bind(newParent.ChildRects.Value[ChildIndex]);
            });
            Children.AddListener((_, _, _) =>
            {
                ComputeMinRect();
                ComputeRect();
            });

            MaxRect.AddListener((_, _, newValue) =>
            {
                MaxSize.Value = new Vector2(
                    Mathf.Min(newValue.x, MaxRect.Value.width),
                    Mathf.Min(newValue.y, MaxRect.Value.height)
                );
                ComputeRect();
            });
            MinRect.AddListener((_, _, newValue) =>
            {
                MinSize.Value = new Vector2(
                    Mathf.Max(newValue.x, MinRect.Value.width),
                    Mathf.Max(newValue.y, MinRect.Value.height)
                );
                ComputeRect();
            });
            Rect.AddListener((_, _, _) =>
            {
                Display.ComputeLayout(Children.Value);
            });

            MaxSize.AddListener((_, _, newValue) =>
            {
                if (newValue.x > MaxRect.Value.width || newValue.y > MaxRect.Value.height)
                {
                    MaxSize.Value = new Vector2(
                        Mathf.Min(newValue.x, MaxRect.Value.width),
                        Mathf.Min(newValue.y, MaxRect.Value.height)
                    );
                    return;
                }
                ComputeRect();
            });
            MinSize.AddListener((_, _, newValue) =>
            {
                if (newValue.x < MinRect.Value.width || newValue.y < MinRect.Value.height)
                {
                    MinSize.Value = new Vector2(
                        Mathf.Max(newValue.x, MinRect.Value.width),
                        Mathf.Max(newValue.y, MinRect.Value.height)
                    );
                    return;
                }
                ComputeRect();
            });
        }

        private void ComputeMinRect()
        {
            var minRect = new Rect(MaxRect.Value.position, Display.MinSize(Children.Value));
            var paddingRect = Padding.ApplyTo(minRect, true);
            var marginRect = Margin.ApplyTo(paddingRect, true);
            MinRect.Value = marginRect;
        }

        private void ComputeMaxRect()
        {
            MaxRect.Value = new Rect(Parent.Value.position, )
        }

        private void ComputeRect()
        {
            var marginRect = Margin.ApplyTo(MaxRect.Value);
            var alignRect = Align.ApplyTo(marginRect);
            var clampedRect = ClampRect(alignRect, Align.Anchor);
            var paddingRect = Padding.ApplyTo(clampedRect);
            Rect.Value = paddingRect;
        }

        private Rect ClampRect(Rect container, Vector2 anchor)
        {
            var width = Mathf.Clamp(container.width, MinSize.Value.x, MaxSize.Value.x);
            var height = Mathf.Clamp(container.height, MinSize.Value.y, MaxSize.Value.y);

            return new Rect(
                container.x + anchor.x * (container.width - width),
                container.y + anchor.y * (container.height - height),
                width,
                height
            );
        }
    }
}
