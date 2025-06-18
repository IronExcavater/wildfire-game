using System;
using UnityEditor;
using UnityEngine;
using Utilities.Observables;

namespace Editor.CssRect
{
    public class BoxRect : IObservable<BoxRect, ValueChange<BoxRect>>
    {
        public readonly Property<BoxRect> Parent = new(observeInnerValue: false);
        public readonly ObservableList<BoxRect> Children = new();
        public readonly Property<SerializedProperty> Property = new();

        public readonly Property<Vector2> ContainerPosition = new();
        public readonly Property<Vector2> ContainerSize = new();
        public readonly Property<Rect> Container = new();

        public readonly Property<Vector2> BoundsPosition = new();
        public readonly Property<Vector2> BoundsSize = new();
        public readonly Property<Rect> Bounds = new();

        public readonly Property<Vector2> RectPosition = new();
        public readonly Property<Vector2> RectSize = new();
        public readonly Property<Rect> Rect = new();

        public readonly Property<float> MaxWidth = new();
        public readonly Property<BoxInsets> Margin = new();
        public readonly Property<BoxInsets> Padding = new();
        public readonly Property<BoxAlign> Align = new();
        public readonly Property<BoxDisplay> Display = new();

        public BoxRect(Vector2 position, float maxWidth, SerializedProperty property = null)
        {
            InitalizeListeners();
            BoundsPosition.Value = position;
            MaxWidth.Value = maxWidth;
            BoundsSize.Value = new Vector2(maxWidth, 0);
            Property.Value = property;
        }

        public BoxRect(BoxRect parent, float? height = null, float? width = null)
        {
            InitalizeListeners();
            BoundsSize.Value = new Vector2(
                width ?? parent.BoundsSize.Value.x,
                height ?? EditorGUIUtility.singleLineHeight
            );
            Parent.Value = parent;
        }

        public BoxRect(BoxRect parent, SerializedProperty property, float? width = null)
        {
            InitalizeListeners();
            BoundsSize.Value = new Vector2(
                width ?? parent.BoundsSize.Value.x,
                0
            );
            Parent.Value = parent;
            Property.Value = property;
        }

        private void InitalizeListeners()
        {
            Parent.AddListener((_, change) =>
            {
                change.OldValue?.Children.Remove(this);
                change.OldValue?.BoundsPosition.RemoveListener(UpdatePositionFromParentListener);
                change.OldValue?.MaxWidth.RemoveListener(UpdateMaxWidthFromParentListener);

                change.NewValue?.Children.Add(this);
                change.NewValue?.BoundsPosition.AddListener(UpdatePositionFromParentListener);
                change.NewValue?.MaxWidth.AddListener(UpdateMaxWidthFromParentListener);

                if (change.NewValue != null)
                {
                    BoundsPosition.Value = Display.Value.PositionFromParent(this);
                    MaxWidth.Value = Display.Value.MaxWidthFromParent(this);
                }

                OnChanged?.Invoke(new ValueChange<BoxRect>(this, this));
            });
            Children.AddListener((_, change) =>
            {
                IsChildrenAndPropertyValid();

                foreach (var child in change.GetAdded)
                {
                    child.BoundsSize.AddListener(UpdateSizeFromChildrenListener);
                    UpdateBoundsSize();
                }

                foreach (var child in change.GetRemoved)
                    child.BoundsSize.RemoveListener(UpdateSizeFromChildrenListener);

                OnChanged?.Invoke(new ValueChange<BoxRect>(this, this));
            });
            Property.AddListener((_, _) =>
            {
                if (!IsChildrenAndPropertyValid()) return;
                UpdateBoundsSize();

                OnChanged?.Invoke(new ValueChange<BoxRect>(this, this));
            });

            BoundsPosition.AddListener(UpdateBoundsListener);
            BoundsSize.AddListener(UpdateBoundsListener);
            Bounds.AddListener((_, _) => UpdateRect());

            MaxWidth.AddListener((_, _) => UpdateBoundsSize());
            Margin.AddListener((_, _) => UpdateRect());
            Padding.AddListener((_, _) => UpdateRect());
            Align.AddListener((_, _) => UpdateRect());
            Display.AddListener((_, _) =>
            {
                BoundsPosition.Value = Display.Value.PositionFromParent(this);
                UpdateBoundsSize();

                OnChanged?.Invoke(new ValueChange<BoxRect>(this, this));
            });

            RectPosition.AddListener(UpdateRectListener);
            RectSize.AddListener(UpdateRectListener);

        }

        private void UpdatePositionFromParentListener(PropertyBase<Vector2, Vector2, ValueChange<Vector2>> property,
            ValueChange<Vector2> change)
        {
            BoundsPosition.Value = Display.Value.PositionFromParent(this);
            OnChanged?.Invoke(new ValueChange<BoxRect>(this, this));
        }

        private void UpdateMaxWidthFromParentListener(PropertyBase<float, float, ValueChange<float>> property,
            ValueChange<float> change)
        {
            MaxWidth.Value = Display.Value.MaxWidthFromParent(this);
            OnChanged?.Invoke(new ValueChange<BoxRect>(this, this));
        }

        private void UpdateSizeFromChildrenListener(PropertyBase<Vector2, Vector2, ValueChange<Vector2>> property,
            ValueChange<Vector2> change)
        {
            UpdateBoundsSize();
        }

        private void UpdateBoundsListener(PropertyBase<Vector2, Vector2, ValueChange<Vector2>> property,
            ValueChange<Vector2> change)
        {
            Bounds.Value = new Rect(BoundsPosition.Value, BoundsSize.Value);
            OnChanged?.Invoke(new ValueChange<BoxRect>(this, this));
        }

        private void UpdateRectListener(PropertyBase<Vector2, Vector2, ValueChange<Vector2>> property,
            ValueChange<Vector2> change)
        {
            Rect.Value = new Rect(RectPosition.Value, RectSize.Value);
            OnChanged?.Invoke(new ValueChange<BoxRect>(this, this));
        }

        private void UpdateBoundsSize()
        {
            BoundsSize.Value =
                Property.Value != null
                    ? Display.Value.SizeFromProperty(this)
                : Children.Count > 0
                    ? Display.Value.SizeFromChildren(this)
                    : BoundsSize.Value;
        }

        private void UpdateRect()
        {
            var align = Align.Value.ApplyTo(Bounds.Value);
            var margin = Margin.Value.ApplyTo(new Rect(align, Bounds.Value.size));
            var padding = Padding.Value.ApplyTo(margin);

            RectPosition.Value = padding.position;
            RectSize.Value = padding.size;
        }

        /// <summary>
        /// BoxRect Children and Property functionality is mutually exclusive. BoxRect automatically deals with this by
        /// connecting the Property to a new BoxRect and parenting it with original BoxRect.
        /// </summary>
        private bool IsChildrenAndPropertyValid()
        {
            if (!(Children.Count > 0 && Property.Value != null)) return true;

            var childBox = new BoxRect(this, Property.Value);
            Property.Value = null;
            return false;
        }

        public event Action<ValueChange<BoxRect>> OnChanged;
    }
}
