using UnityEngine;

namespace KabreetGames.ParallaxSystem
{
    public static class RectExtensions
    {
        public static Rect[] SplitRectVertically(Rect rectToSplit, int n, float spacing)
        {
            var rects = new Rect[n];

            var totalSpacing = spacing * (n - 1);
            var heightPerRect = (rectToSplit.height - totalSpacing) / n;

            for (var i = 0; i < n; i++)
            {
                rects[i] = new Rect(
                    rectToSplit.x,
                    rectToSplit.y + i * (heightPerRect + spacing),
                    rectToSplit.width,
                    heightPerRect
                );
            }

            return rects;
        }

        public static Rect MergeRects(Rect[] rects)
        {
            if (rects == null || rects.Length == 0)
                return new Rect();

            var xMin = rects[0].xMin;
            var xMax = rects[0].xMax;
            var yMin = rects[0].yMin;
            var yMax = rects[0].yMax;

            for (var i = 1; i < rects.Length; i++)
            {
                xMin = Mathf.Min(xMin, rects[i].xMin);
                xMax = Mathf.Max(xMax, rects[i].xMax);
                yMin = Mathf.Min(yMin, rects[i].yMin);
                yMax = Mathf.Max(yMax, rects[i].yMax);
            }

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        public static Rect[] SplitRectHorizontal(Rect rectToSplit, int n, float spacing)
        {
            var rects = new Rect[n];

            var totalSpacing = spacing * (n - 1);
            var widthPerRect = (rectToSplit.width - totalSpacing) / n;

            for (var i = 0; i < n; i++)
            {
                rects[i] = new Rect(
                    rectToSplit.x + i * (widthPerRect + spacing),
                    rectToSplit.y,
                    widthPerRect,
                    rectToSplit.height
                );
            }

            return rects;
        }
    }
}