using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NodeEditorGUI
{
    // Curve parameters
    public static float curveBaseDirection = 1.5f, curveBaseStart = 2f, curveDirectionScale = 0.004f;

    public enum ConnectionDrawMethod { Bezier, StraightLine }

    /// <summary>
    /// Draws a node connection from start to end, horizontally
    /// </summary>
    public static void DrawConnection(Vector2 startPos, Vector2 endPos, Color col)
    {
        Vector2 startVector = startPos.x <= endPos.x ? Vector2.right : Vector2.left;
        DrawConnection(startPos, startVector, endPos, -startVector, col);
    }

    /// <summary>
    /// Draws a node connection from start to end, horizontally
    /// </summary>
    public static void DrawConnection(Vector2 startPos, Vector2 endPos, ConnectionDrawMethod drawMethod, Color col)
    {
        Vector2 startVector = startPos.x <= endPos.x ? Vector2.right : Vector2.left;
        DrawConnection(startPos, startVector, endPos, -startVector, drawMethod, col);
    }

    /// <summary>
    /// Draws a node connection from start to end with specified vectors
    /// </summary>
    public static void DrawConnection(Vector2 startPos, Vector2 startDir, Vector2 endPos, Vector2 endDir, Color col)
    {
#if NODE_EDITOR_LINE_CONNECTION
			DrawConnection (startPos, startDir, endPos, endDir, ConnectionDrawMethod.StraightLine, col);
#else
        DrawConnection(startPos, startDir, endPos, endDir, ConnectionDrawMethod.Bezier, col);
#endif
    }

    /// <summary>
    /// Draws a node connection from start to end with specified vectors
    /// </summary>
    public static void DrawConnection(Vector2 startPos, Vector2 startDir, Vector2 endPos, Vector2 endDir, ConnectionDrawMethod drawMethod, Color col)
    {
        if (drawMethod == ConnectionDrawMethod.Bezier)
        {
            OptimiseBezierDirections(startPos, ref startDir, endPos, ref endDir);
            float dirFactor = 80;//Mathf.Pow ((startPos-endPos).magnitude, 0.3f) * 20;
                                 //Debug.Log ("DirFactor is " + dirFactor + "with a bezier lenght of " + (startPos-endPos).magnitude);
            RTEditorGUI.DrawBezier(startPos, endPos, startPos + startDir * dirFactor, endPos + endDir * dirFactor, col * Color.gray, null, 3);
        }
        else if (drawMethod == ConnectionDrawMethod.StraightLine)
            RTEditorGUI.DrawLine(startPos, endPos, col * Color.gray, null, 3);
    }

    /// <summary>
    /// Optimises the bezier directions scale so that the bezier looks good in the specified position relation.
    /// Only the magnitude of the directions are changed, not their direction!
    /// </summary>
    public static void OptimiseBezierDirections(Vector2 startPos, ref Vector2 startDir, Vector2 endPos, ref Vector2 endDir)
    {
        Vector2 offset = (endPos - startPos) * curveDirectionScale;
        float baseDir = Mathf.Min(offset.magnitude / curveBaseStart, 1) * curveBaseDirection;
        Vector2 scale = new Vector2(Mathf.Abs(offset.x) + baseDir, Mathf.Abs(offset.y) + baseDir);
        // offset.x and offset.y linearly increase at scale of curveDirectionScale
        // For 0 < offset.magnitude < curveBaseStart, baseDir linearly increases from 0 to curveBaseDirection. For offset.magnitude > curveBaseStart, baseDir = curveBaseDirection
        startDir = Vector2.Scale(startDir.normalized, scale);
        endDir = Vector2.Scale(endDir.normalized, scale);
    }
}
