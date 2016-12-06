using UnityEngine;

public static class Extensions
{
    /// <summary>
    /// Scales a rect by a given amount around its center point
    /// </summary>
    /// <param name="rect">The given rect</param>
    /// <param name="scale">The scale factor</param>
    /// <returns>The given rect scaled around its center</returns>
    public static Rect ScaleSizeBy(this Rect rect, float scale) { return rect.ScaleSizeBy(scale, rect.center); }

    /// <summary>
    /// Scales a rect by a given amount and around a given point
    /// </summary>
    /// <param name="rect">The rect to size</param>
    /// <param name="scale">The scale factor</param>
    /// <param name="pivotPoint">The point to scale around</param>
    /// <returns>The rect, scaled around the given pivot point</returns>
    public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
    {
        Rect result = rect;

        //"translate" the top left to something like an origin
        result.x -= pivotPoint.x;
        result.y -= pivotPoint.y;

        //Scale the rect
        result.xMin *= scale;
        result.yMin *= scale;
        result.xMax *= scale;
        result.yMax *= scale;

        //"translate" the top left back to its original position
        result.x += pivotPoint.x;
        result.y += pivotPoint.y;

        return result;
    }
}