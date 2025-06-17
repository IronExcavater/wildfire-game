using System;
using UnityEditor;
using UnityEngine;
using Utilities;
using Utilities.Observables;

namespace Editor.CssRect
{
    public class BoxRect : IObservable<BoxRect, ValueChange<BoxRect>>
    {
        public readonly ValueProperty<BoxRect> Parent = new();
        public readonly ObservableList<BoxRect> Children = new();
        public readonly ValueProperty<SerializedProperty> Property = new();

        public readonly ValueProperty<Vector2> BoundsPosition = new();
        public readonly ValueProperty<Vector2> BoundsSize = new();
        public readonly ValueProperty<Rect> Bounds = new();

        public readonly ValueProperty<Vector2> RectPosition = new();
        public readonly ValueProperty<Vector2> RectSize = new();
        public readonly ValueProperty<Rect> Rect = new();

        public readonly ValueProperty<float> MaxWidth = new();
        public readonly ValueProperty<BoxInsets> Margin = new();
        public readonly ValueProperty<BoxInsets> Padding = new();
        public readonly ValueProperty<BoxAlign> Align = new();
        public readonly ValueProperty<BoxDisplay> Display = new();

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
            Children.AddListener((_, _) =>
            {
                IsChildrenAndPropertyValid();

                // TODO: Stop infinite loop between parent and child. Diagnose where the unwanted link (hopefully
                //  doesn't require redesign and is just incorrect implementation). Investigate ObservableList OnChange
                //  and UpdateBoundsSize()
                // UpdateBoundsSize();
                //OnChanged?.Invoke(new ValueChange<BoxRect>(this, this));
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
            var margin = Margin.Value.ApplyTo(Bounds.Value);
            var align = Align.Value.ApplyTo(margin);
            var padding = Padding.Value.ApplyTo(new Rect(align, margin.size));

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
