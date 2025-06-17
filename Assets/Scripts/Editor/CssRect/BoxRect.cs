using UnityEditor;
using UnityEngine;
using Utilities;
using Utilities.Observables;

namespace Editor.CssRect
{
    public class BoxRect
    {
        public readonly ValueProperty<BoxRect> Parent = new();
        public readonly ObservableList<BoxRect> Children = new(new());
        public readonly ValueProperty<SerializedProperty> Property = new();

        public readonly ValueProperty<Vector2> BoundsPosition = new();
        public readonly ValueProperty<Vector2> BoundsSize = new();
        public readonly ValueProperty<Rect> Bounds = new();

        public readonly ValueProperty<Vector2> RectPosition = new();
        public readonly ValueProperty<Vector2> RectSize = new();
        public readonly ValueProperty<Rect> Rect = new();

        public readonly ValueProperty<BoxInsets> Margin = new();
        public readonly ValueProperty<BoxInsets> Padding = new();
        public readonly ValueProperty<BoxAlign> Align = new();
        public readonly ValueProperty<BoxDisplay> Display = new(BoxDisplay.Block);

        public BoxRect(Rect bounds, SerializedProperty property = null)
        {
            InitalizeListeners();
            BoundsPosition.Value = bounds.position;
            BoundsSize.Value = bounds.size;
            Property.Value = property;
        }

        private void InitalizeListeners()
        {
            Parent.AddListener((_, change) =>
            {
                change.OldValue?.Children.Value.Remove(this);
                change.OldValue?.BoundsPosition.RemoveListener(UpdatePositionFromParentListener);
                change.NewValue?.Children.Value.Add(this);
                change.NewValue?.BoundsPosition.AddListener(UpdatePositionFromParentListener);
            });
            Children.AddListener((_, change) =>
            {
                foreach (var child in change.GetAdded)
                    child.BoundsSize.AddListener(UpdateSizeFromChildrenListener);

                foreach (var child in change.GetRemoved)
                    child.BoundsSize.RemoveListener(UpdateSizeFromChildrenListener);
            });

            BoundsPosition.AddListener(UpdateBoundsListener);
            BoundsSize.AddListener(UpdateBoundsListener);
            Bounds.AddListener((_, _) => UpdateRect());

            Margin.AddListener((_, _) => UpdateRect());
            Padding.AddListener((_, _) => UpdateRect());
            Align.AddListener((_, _) => UpdateRect());
            Display.AddListener((_, _) =>
            {
                BoundsPosition.Value = Display.Value.PositionFromParent(this);
                BoundsSize.Value = Display.Value.SizeFromChildren(this);
            });

            RectPosition.AddListener(UpdateRectListener);
            RectSize.AddListener(UpdateRectListener);

        }

        private void UpdatePositionFromParentListener(PropertyBase<Vector2, Vector2, ValueChange<Vector2>> property, ValueChange<Vector2> change)
        {
            BoundsPosition.Value = Display.Value.PositionFromParent(this);
        }

        private void UpdateSizeFromChildrenListener(PropertyBase<Vector2, Vector2, ValueChange<Vector2>> property, ValueChange<Vector2> change)
        {
            BoundsSize.Value = Display.Value.SizeFromChildren(this);
        }

        private void UpdateBoundsListener(PropertyBase<Vector2, Vector2, ValueChange<Vector2>> property, ValueChange<Vector2> change)
        {
            Bounds.Value = new Rect(BoundsPosition.Value, BoundsSize.Value);
        }

        private void UpdateRectListener(PropertyBase<Vector2, Vector2, ValueChange<Vector2>> property, ValueChange<Vector2> change)
        {
            Rect.Value = new Rect(RectPosition.Value, RectSize.Value);
        }

        private void UpdateRect()
        {
            var margin = Margin.Value.ApplyTo(Bounds.Value);
            var align = Align.Value.ApplyTo(margin);
            var padding = Padding.Value.ApplyTo(new Rect(align, margin.size));

            RectPosition.Value = padding.position;
            RectSize.Value = padding.size;
        }
    }
}
