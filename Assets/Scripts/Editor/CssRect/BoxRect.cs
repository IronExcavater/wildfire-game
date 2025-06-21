using System;
using Editor.Utilities;
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

        public readonly Property<Vector2> MinSize = new();

        public readonly Property<Vector2> ContainerPosition = new();
        public readonly Property<Vector2> ContainerSize = new();
        public readonly Property<Rect> Container = new();

        public readonly Property<Vector2> BoundsPosition = new();
        public readonly Property<Vector2> BoundsSize = new();
        public readonly Property<Rect> Bounds = new();

        public readonly Property<Vector2> RectPosition = new();
        public readonly Property<Vector2> RectSize = new();
        public readonly Property<Rect> Rect = new();

        public readonly Property<BoxInsets> Padding = new();
        public readonly Property<BoxAlign> Align = new();
        public readonly Property<BoxVec2> Gap = new(new BoxVec2(EditorUtils.SmallGap));

        public readonly Property<BoxDisplay> Display = new();
        public readonly Property<BoxPosition> Position = new();
        public readonly Property<BoxVec2> Offset = new();

        public BoxRect(Vector2 position, Vector2 minSize, SerializedProperty property = null)
        {
            InitalizeListeners();
            ContainerPosition.Value = position;
            Property.Value = property;
            MinSize.Value = minSize;
            BoundsSize.Value = MinSize.Value;
        }

        public BoxRect(BoxRect parent, float? height = null, float? width = null, Vector2? minSize = null)
        {
            InitalizeListeners();
            BoundsSize.Value = new Vector2(
                width ?? parent.RectSize.Value.x,
                height ?? EditorUtils.LineHeight
            );
            Parent.Value = parent;
            MinSize.Value = minSize ?? BoundsSize.Value;
        }

        public BoxRect(BoxRect parent, SerializedProperty property, float? width = null, Vector2? minSize = null)
        {
            InitalizeListeners();
            BoundsSize.Value = new Vector2(
                width ?? parent.RectSize.Value.x,
                EditorUtils.LineHeight
            );
            Parent.Value = parent;
            Property.Value = property;
            MinSize.Value = minSize ?? BoundsSize.Value;
        }

        private void InitalizeListeners()
        {
            Parent.AddListener((_, change) =>
            {
                change.OldValue?.Children.Remove(this);
                change.OldValue?.RectPosition.RemoveListener(UpdateContainerFromParentListener);

                change.NewValue?.Children.Add(this);
                change.NewValue?.RectPosition.AddListener(UpdateContainerFromParentListener);

                if (change.NewValue != null)
                {
                    UpdateContainerSize();
                    UpdateContainerPosition();
                }

                InvokeOnChanged();
            });
            Children.AddListener((_, _) =>
            {
                IsChildrenAndPropertyValid();
                UpdateBoundsSize();
                InvokeOnChanged();
            });
            Property.AddListener((_, _) =>
            {
                if (!IsChildrenAndPropertyValid()) return;
                UpdateBoundsSize();
                InvokeOnChanged();
            });

            MinSize.AddListener((_, _) =>
            {
                if (MinSize.Value.x > ContainerSize.Value.x || MinSize.Value.y > ContainerSize.Value.y)
                    UpdateContainerSize();
                InvokeOnChanged();
            });

            ContainerPosition.AddListener(UpdateContainerListener);
            ContainerSize.AddListener(UpdateContainerListener);
            Container.AddListener((_, _) =>
            {
                UpdateBoundsPosition();
                InvokeOnChanged();
            });

            BoundsPosition.AddListener((_, _) =>
            {
                UpdateBounds();
                InvokeOnChanged();
            });
            BoundsSize.AddListener((_, _) =>
            {
                UpdateBoundsPosition();
                UpdateBounds();
                UpdateContainerSize();
                InvokeOnChanged();
            });
            Bounds.AddListener((_, _) =>
            {
                UpdateRect();
                InvokeOnChanged();
            });
            Padding.AddListener((_, _) =>
            {
                UpdateRect();
                InvokeOnChanged();
            });
            Align.AddListener((_, _) =>
            {
                UpdateBoundsPosition();
                InvokeOnChanged();
            });
            Display.AddListener((_, _) =>
            {
                UpdateContainerSize();
                UpdateBoundsPosition();
                UpdateBoundsSize();
                InvokeOnChanged();
            });

            RectPosition.AddListener(UpdateRectListener);
            RectSize.AddListener(UpdateRectListener);
        }

        private void InvokeOnChanged()
        {
            OnChanged?.Invoke(new ValueChange<BoxRect>(this, this));
        }

        private void UpdateContainerFromParentListener(PropertyBase<Vector2, Vector2, ValueChange<Vector2>> property,
            ValueChange<Vector2> change)
        {
            UpdateContainerSize();
            UpdateContainerPosition();
            InvokeOnChanged();
        }

        private void UpdateContainerPosition()
        {
            ContainerPosition.Value = Display.Value.ContainerPositionFromParent(this);
        }

        private void UpdateContainerSize()
        {
            ContainerSize.Value =
                Parent.Value != null
                    ? Display.Value.ContainerSizeFromParent(this)
                    : MinSize.Value;
        }

        private void UpdateContainerListener(PropertyBase<Vector2, Vector2, ValueChange<Vector2>> property,
            ValueChange<Vector2> change)
        {
            Container.Value = new Rect(ContainerPosition.Value, ContainerSize.Value);
            InvokeOnChanged();
        }

        private void UpdateBoundsPosition()
        {
            BoundsPosition.Value = Align.Value.ApplyTo(Container.Value, BoundsSize.Value);
        }

        private void UpdateBoundsSize()
        {
            BoundsSize.Value =
                Property.Value != null
                    ? Display.Value.BoundsSizeFromProperty(this)
                : Children.Count > 0
                    ? Display.Value.BoundsSizeFromChildren(this)
                    : BoundsSize.Value;
        }

        private void UpdateBounds()
        {
            Bounds.Value = new Rect(BoundsPosition.Value, BoundsSize.Value);
        }

        private void UpdateRect()
        {
            var padding = Padding.Value.ApplyTo(Bounds.Value);

            RectPosition.Value = padding.position;
            RectSize.Value = padding.size;
        }

        private void UpdateRectListener(PropertyBase<Vector2, Vector2, ValueChange<Vector2>> property,
            ValueChange<Vector2> change)
        {
            Rect.Value = new Rect(RectPosition.Value, RectSize.Value);
            InvokeOnChanged();
        }

        public void DebugRect()
        {
            EditorGUI.DrawRect(Container.Value, Color.red);
            EditorGUI.DrawRect(Bounds.Value, Color.orange);
            EditorGUI.DrawRect(Rect.Value, Color.yellow);
        }

        /// <summary>
        /// BoxRect Children and Property functionality is mutually exclusive. BoxRect automatically deals with this by
        /// connecting the Property to a new BoxRect and parenting it with original BoxRect.
        /// </summary>
        private bool IsChildrenAndPropertyValid()
        {
            if (!(Children.Count > 0 && Property.Value != null)) return true;

            var childBox = new BoxRect(this, property: Property.Value);
            Property.Value = null;
            return false;
        }

        public event Action<ValueChange<BoxRect>> OnChanged;
    }
}
