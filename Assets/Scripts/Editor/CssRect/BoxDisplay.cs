using System.Linq;
using Editor.CssRect;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;
using Utilities.Observables;

namespace Editor.CssRect
{
    public enum BoxDisplay
    {
        None,
        Block,
        Inline,
        Flex,
        Grid,
        Absolute,
        Relative,
    }

    public static class BoxDisplayExtensions
    {
        public static Vector2 MinSize(this BoxDisplay boxDisplay, ObservableList<BoxRect> children)
        {
            return boxDisplay switch
            {
                BoxDisplay.Block => ComputeBlockLayout(children),
                // TODO: Add Flex, Grid, Inline etc
                _ => Vector2.zero
            };
        }

        public static Rect MaxRect(this BoxDisplay boxDisplay, BoxRect parent)
        {
            return boxDisplay switch
            {
                _ => new()
            };
        }

        public static void ComputeLayout(this BoxDisplay boxDisplay, ObservableList<BoxRect> children)
        {
            switch (boxDisplay)
            {
                case BoxDisplay.Block:
                    ComputeBlockLayout(children, true);
                    break;
            }
        }

        private static Vector2 ComputeBlockLayout(ObservableList<BoxRect> children, bool updateChildren = false)
         {
             var width = 0f;
             var height = 0f;
             foreach (var child in children)
             {
                 width = Mathf.Max(width, child.Rect.Value.width);
                 height += child.Rect.Value.height;
             }

             return new Vector2(width, height);
         }
    }


}
