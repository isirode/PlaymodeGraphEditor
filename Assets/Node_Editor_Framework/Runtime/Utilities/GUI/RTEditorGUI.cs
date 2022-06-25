using UnityEngine;
using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;

using Object = UnityEngine.Object;

public static class RTEditorGUI
{

	#region GUI Proportioning Utilities

	public static float labelWidth = 150;
	public static float fieldWidth = 50;
	public static float indent = 0;
	private static float textFieldHeight { get { return GUI.skin.textField.CalcHeight(new GUIContent("i"), 10); } }

	public static Rect PrefixLabel(Rect totalPos, GUIContent label, GUIStyle style)
	{
		if (label == GUIContent.none)
			return totalPos;//IndentedRect (totalPos);

		Rect labelPos = new Rect(totalPos.x + indent, totalPos.y, Mathf.Min(getLabelWidth() - indent, totalPos.width / 2), totalPos.height);
		GUI.Label(labelPos, label, style);

		return new Rect(totalPos.x + getLabelWidth(), totalPos.y, totalPos.width - getLabelWidth(), totalPos.height);
	}

	public static Rect PrefixLabel(Rect totalPos, float percentage, GUIContent label, GUIStyle style)
	{
		if (label == GUIContent.none)
			return totalPos;

		Rect labelPos = new Rect(totalPos.x + indent, totalPos.y, totalPos.width * percentage, totalPos.height);
		GUI.Label(labelPos, label, style);

		return new Rect(totalPos.x + totalPos.width * percentage, totalPos.y, totalPos.width * (1 - percentage), totalPos.height);
	}

	private static Rect IndentedRect(Rect source)
	{
		return new Rect(source.x + indent, source.y, source.width - indent, source.height);
	}

	private static float getLabelWidth()
	{
#if UNITY_EDITOR
		return UnityEditor.EditorGUIUtility.labelWidth;
#else
			if (labelWidth == 0)
			return 150;
			return labelWidth;
#endif
	}

	private static float getFieldWidth()
	{
#if UNITY_EDITOR
		return UnityEditor.EditorGUIUtility.fieldWidth;
#else
			if (fieldWidth == 0)
			return 50;
			return fieldWidth;
#endif
	}

	private static Rect GetFieldRect(GUIContent label, GUIStyle style, params GUILayoutOption[] options)
	{
		float minLabelW = 0, maxLabelW = 0;
		if (label != GUIContent.none)
			style.CalcMinMaxWidth(label, out minLabelW, out maxLabelW);
		return GUILayoutUtility.GetRect(getFieldWidth() + minLabelW + 5, getFieldWidth() + maxLabelW + 5, textFieldHeight, textFieldHeight, options);
	}

	private static Rect GetSliderRect(GUIContent label, GUIStyle style, params GUILayoutOption[] options)
	{
		float minLabelW = 0, maxLabelW = 0;
		if (label != GUIContent.none)
			style.CalcMinMaxWidth(label, out minLabelW, out maxLabelW);
		return GUILayoutUtility.GetRect(getFieldWidth() + minLabelW + 5, getFieldWidth() + maxLabelW + 5 + 100, textFieldHeight, textFieldHeight, options);
	}

	private static Rect GetSliderRect(Rect sliderRect)
	{
		return new Rect(sliderRect.x, sliderRect.y, sliderRect.width - getFieldWidth() - 5, sliderRect.height);
	}

	private static Rect GetSliderFieldRect(Rect sliderRect)
	{
		return new Rect(sliderRect.x + sliderRect.width - getFieldWidth(), sliderRect.y, getFieldWidth(), sliderRect.height);
	}

	#endregion

	#region Seperator

	/// <summary>
	/// Efficient space like EditorGUILayout.Space
	/// </summary>
	public static void Space()
	{
		Space(6);
	}
	/// <summary>
	/// Space like GUILayout.Space but more efficient
	/// </summary>
	public static void Space(float pixels)
	{
		GUILayoutUtility.GetRect(pixels, pixels);
	}


	/// <summary>
	/// A GUI Function which simulates the default seperator
	/// </summary>
	public static void Seperator()
	{
		setupSeperator();
		GUILayout.Box(GUIContent.none, seperator, new GUILayoutOption[] { GUILayout.Height(1) });
	}

	/// <summary>
	/// A GUI Function which simulates the default seperator
	/// </summary>
	public static void Seperator(Rect rect)
	{
		setupSeperator();
		GUI.Box(new Rect(rect.x, rect.y, rect.width, 1), GUIContent.none, seperator);
	}

	private static GUIStyle seperator;
	private static void setupSeperator()
	{
		if (seperator == null)
		{
			seperator = new GUIStyle();
			seperator.normal.background = ColorToTex(1, new Color(0.6f, 0.6f, 0.6f));
			seperator.stretchWidth = true;
			seperator.margin = new RectOffset(0, 0, 7, 7);
		}
	}

	#endregion

	#region Change Check

	private static Stack<bool> changeStack = new Stack<bool>();

	public static void BeginChangeCheck()
	{
		changeStack.Push(GUI.changed);
		GUI.changed = false;
	}

	public static bool EndChangeCheck()
	{
		bool changed = GUI.changed;
		if (changeStack.Count > 0)
		{
			GUI.changed = changeStack.Pop();
			if (changed && changeStack.Count > 0 && !changeStack.Peek())
			{ // Update parent change check
				changeStack.Pop();
				changeStack.Push(changed);
			}
		}
		else
			Debug.LogWarning("Requesting more EndChangeChecks than issuing BeginChangeChecks!");
		return changed;
	}

	#endregion

	/// <summary>
	/// Add copy-paste functionality to any text field
	/// Returns changed text or NULL.
	/// Usage: text = HandleCopyPaste (controlID) ?? text;
	/// </summary>
	public static string HandleCopyPaste(int controlID)
	{
		if (controlID == GUIUtility.keyboardControl)
		{
			if (Event.current.type == EventType.KeyUp && (Event.current.modifiers == EventModifiers.Control || Event.current.modifiers == EventModifiers.Command))
			{
				if (Event.current.keyCode == KeyCode.C)
				{
					Event.current.Use();
					TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
					editor.Copy();
				}
				else if (Event.current.keyCode == KeyCode.V)
				{
					Event.current.Use();
					TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
					editor.Paste();
#if UNITY_5_3_OR_NEWER || UNITY_5_3
					return editor.text;
#else
						return editor.content.text;
#endif
				}
				else if (Event.current.keyCode == KeyCode.A)
				{
					Event.current.Use();
					TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
					editor.SelectAll();
				}
			}
		}
		return null;
	}

	#region Low-Level Drawing

	private static Material lineMaterial;
	private static Texture2D lineTexture;

	private static void SetupLineMat(Texture tex, Color col)
	{
		if (lineMaterial == null)
		{
			Shader lineShader = Shader.Find("Hidden/LineShader");
			if (lineShader == null)
				throw new NotImplementedException("Missing line shader implementation!");
			lineMaterial = new Material(lineShader);
		}
		if (tex == null)
			tex = lineTexture != null ? lineTexture : lineTexture = ResourceManager.LoadTexture("Assets/Node_Editor_Framework/Runtime/Resources/Textures/AALine.png");
		if (tex == null)
        {
			throw new NotImplementedException("Missing texture!");
		}
		lineMaterial.SetTexture("_LineTexture", tex);
		lineMaterial.SetColor("_LineColor", col);
		lineMaterial.SetPass(0);
	}

	/// <summary>
	/// Draws a Bezier curve just as UnityEditor.Handles.DrawBezier, non-clipped. If width is 1, tex is ignored; Else if tex is null, a anti-aliased texture tinted with col will be used; else, col is ignored and tex is used.
	/// </summary>
	public static void DrawBezier(Vector2 startPos, Vector2 endPos, Vector2 startTan, Vector2 endTan, Color col, Texture2D tex, float width = 1)
	{
		if (Event.current.type != EventType.Repaint)
			return;

		Rect clippingRect = GUIScaleUtility.getTopRect;
		clippingRect.x = clippingRect.y = 0;
		Rect bounds = new Rect(Mathf.Min(startPos.x, endPos.x), Mathf.Min(startPos.y, endPos.y),
							Mathf.Abs(startPos.x - endPos.x), Mathf.Abs(startPos.y - endPos.y));
		if (!bounds.Overlaps(clippingRect))
			return;

		// Own Bezier Formula
		// Slower than handles because of the horrendous amount of calls into the native api

		// Calculate optimal segment count
		int segmentCount = CalculateBezierSegmentCount(startPos, endPos, startTan, endTan);
		// Draw bezier with calculated segment count
		DrawBezier(startPos, endPos, startTan, endTan, col, tex, segmentCount, width);
	}

	/// <summary>
	/// Draws a clipped Bezier curve just as UnityEditor.Handles.DrawBezier.
	/// If width is 1, tex is ignored; Else if tex is null, a anti-aliased texture tinted with col will be used; else, col is ignored and tex is used.
	/// </summary>
	public static void DrawBezier(Vector2 startPos, Vector2 endPos, Vector2 startTan, Vector2 endTan, Color col, Texture2D tex, int segmentCount, float width)
	{
		if (Event.current.type != EventType.Repaint && Event.current.type != EventType.KeyDown)
			return;

		Rect clippingRect = GUIScaleUtility.getTopRect;
		clippingRect.x = clippingRect.y = 0;
		Rect bounds = new Rect(Mathf.Min(startPos.x, endPos.x), Mathf.Min(startPos.y, endPos.y),
							Mathf.Abs(startPos.x - endPos.x), Mathf.Abs(startPos.y - endPos.y));
		if (!bounds.Overlaps(clippingRect))
			return;

		// Own Bezier Formula
		// Slower than handles because of the horrendous amount of calls into the native api

		// Calculate bezier points
		Vector2[] bezierPoints = new Vector2[segmentCount + 1];
		for (int pointCnt = 0; pointCnt <= segmentCount; pointCnt++)
			bezierPoints[pointCnt] = GetBezierPoint((float)pointCnt / segmentCount, startPos, endPos, startTan, endTan);
		// Draw polygon line from the bezier points
		DrawPolygonLine(bezierPoints, col, tex, width);
	}

	/// <summary>
	/// Draws a clipped polygon line from the given points. 
	/// If width is 1, tex is ignored; Else if tex is null, a anti-aliased texture tinted with col will be used; else, col is ignored and tex is used.
	/// </summary>
	public static void DrawPolygonLine(Vector2[] points, Color col, Texture2D tex, float width = 1)
	{
		if (Event.current.type != EventType.Repaint && Event.current.type != EventType.KeyDown)
			return;

		// Simplify basic cases
		if (points.Length == 1)
			return;
		else if (points.Length == 2)
			DrawLine(points[0], points[1], col, tex, width);

		// Setup for drawing
		SetupLineMat(tex, col);
		GL.Begin(GL.TRIANGLE_STRIP);
		GL.Color(Color.white);

		// Fetch clipping rect
		Rect clippingRect = GUIScaleUtility.getTopRect;
		clippingRect.x = clippingRect.y = 0;

		Vector2 curPoint = points[0], nextPoint, perpendicular;
		bool clippedP0, clippedP1;
		for (int pointCnt = 1; pointCnt < points.Length; pointCnt++)
		{
			nextPoint = points[pointCnt];

			// Clipping test
			Vector2 curPointOriginal = curPoint, nextPointOriginal = nextPoint;
			if (SegmentRectIntersection(clippingRect, ref curPoint, ref nextPoint, out clippedP0, out clippedP1))
			{ // (partially) visible
			  // Calculate apropriate perpendicular
				if (pointCnt < points.Length - 1) // Interpolate perpendicular inbetween the point chain
					perpendicular = CalculatePointPerpendicular(curPointOriginal, nextPointOriginal, points[pointCnt + 1]);
				else // At the end, there's no further point to interpolate the perpendicular from
					perpendicular = CalculateLinePerpendicular(curPointOriginal, nextPointOriginal);

				if (clippedP0)
				{ // Just became visible, so enable GL again and draw the clipped line start point
					GL.End();
					GL.Begin(GL.TRIANGLE_STRIP);
					DrawLineSegment(curPoint, perpendicular * width / 2);
				}

				// Draw first point before starting with the point chain. Placed here instead of before because of clipping
				if (pointCnt == 1)
					DrawLineSegment(curPoint, CalculateLinePerpendicular(curPoint, nextPoint) * width / 2);
				// Draw the actual point
				DrawLineSegment(nextPoint, perpendicular * width / 2);
			}
			else if (clippedP1)
			{ // Just became invisible, so disable GL
				GL.End();
				GL.Begin(GL.TRIANGLE_STRIP);
			}

			// Update state variable
			curPoint = nextPointOriginal;
		}
		// Finalize drawing
		GL.End();
	}

	/// <summary>
	/// Calculates the optimal bezier segment count for the given bezier
	/// </summary>
	private static int CalculateBezierSegmentCount(Vector2 startPos, Vector2 endPos, Vector2 startTan, Vector2 endTan)
	{
		float straightFactor = Vector2.Angle(startTan - startPos, endPos - startPos) * Vector2.Angle(endTan - endPos, startPos - endPos) * (endTan.magnitude + startTan.magnitude);
		straightFactor = 2 + Mathf.Pow(straightFactor / 400, 0.125f); // 1/8
		float distanceFactor = 1 + (startPos - endPos).magnitude;
		distanceFactor = Mathf.Pow(distanceFactor, 0.25f); // 1/4
		return 4 + (int)(straightFactor * distanceFactor);
	}

	/// <summary>
	/// Calculates the normalized perpendicular vector of the give line
	/// </summary>
	private static Vector2 CalculateLinePerpendicular(Vector2 startPos, Vector2 endPos)
	{
		return new Vector2(endPos.y - startPos.y, startPos.x - endPos.x).normalized;
	}

	/// <summary>
	/// Calculates the normalized perpendicular vector for the pointPos interpolated with its two neighbours prevPos and nextPos
	/// </summary>
	private static Vector2 CalculatePointPerpendicular(Vector2 prevPos, Vector2 pointPos, Vector2 nextPos)
	{
		return CalculateLinePerpendicular(pointPos, pointPos + (nextPos - prevPos));
	}

	/// <summary>
	/// Gets the point of the bezier at t
	/// </summary>
	private static Vector2 GetBezierPoint(float t, Vector2 startPos, Vector2 endPos, Vector2 startTan, Vector2 endTan)
	{
		float rt = 1 - t;
		float rtt = rt * t;

		return startPos * rt * rt * rt +
			startTan * 3 * rt * rtt +
			endTan * 3 * rtt * t +
			endPos * t * t * t;
	}

	/// <summary>
	/// Adds a line sgement to the GL buffer. Useed in a row to create a line
	/// </summary>
	private static void DrawLineSegment(Vector2 point, Vector2 perpendicular)
	{
		GL.TexCoord2(0, 0);
		GL.Vertex(point - perpendicular);
		GL.TexCoord2(0, 1);
		GL.Vertex(point + perpendicular);
	}

	/// <summary>
	/// Draws a non-clipped line. If tex is null, a anti-aliased texture tinted with col will be used; else, col is ignored and tex is used.
	/// </summary>
	public static void DrawLine(Vector2 startPos, Vector2 endPos, Color col, Texture2D tex, float width = 1)
	{
		if (Event.current.type != EventType.Repaint)
			return;

		// Setup
		SetupLineMat(tex, col);
		GL.Begin(GL.TRIANGLE_STRIP);
		GL.Color(Color.white);
		// Fetch clipping rect
		Rect clippingRect = GUIScaleUtility.getTopRect;
		clippingRect.x = clippingRect.y = 0;
		// Clip to rect
		if (SegmentRectIntersection(clippingRect, ref startPos, ref endPos))
		{ // Draw with clipped line if it is visible
			Vector2 perpWidthOffset = CalculateLinePerpendicular(startPos, endPos) * width / 2;
			DrawLineSegment(startPos, perpWidthOffset);
			DrawLineSegment(endPos, perpWidthOffset);
		}
		// Finalize drawing
		GL.End();
	}

	/// <summary>
	/// Clips the line between the points p1 and p2 to the bounds rect.
	/// Uses Liang-Barsky Line Clipping Algorithm.
	/// </summary>
	private static bool SegmentRectIntersection(Rect bounds, ref Vector2 p0, ref Vector2 p1)
	{
		bool cP0, cP1;
		return SegmentRectIntersection(bounds, ref p0, ref p1, out cP0, out cP1);
	}


	/// <summary>
	/// Clips the line between the points p1 and p2 to the bounds rect.
	/// Uses Liang-Barsky Line Clipping Algorithm.
	/// </summary>
	private static bool SegmentRectIntersection(Rect bounds, ref Vector2 p0, ref Vector2 p1, out bool clippedP0, out bool clippedP1)
	{
		float t0 = 0.0f;
		float t1 = 1.0f;
		float dx = p1.x - p0.x;
		float dy = p1.y - p0.y;

		if (ClipTest(-dx, p0.x - bounds.xMin, ref t0, ref t1)) // Left
		{
			if (ClipTest(dx, bounds.xMax - p0.x, ref t0, ref t1)) // Right
			{
				if (ClipTest(-dy, p0.y - bounds.yMin, ref t0, ref t1)) // Bottom
				{
					if (ClipTest(dy, bounds.yMax - p0.y, ref t0, ref t1)) // Top
					{
						clippedP0 = t0 > 0;
						clippedP1 = t1 < 1;

						if (clippedP1)
						{
							p1.x = p0.x + t1 * dx;
							p1.y = p0.y + t1 * dy;
						}

						if (clippedP0)
						{
							p0.x = p0.x + t0 * dx;
							p0.y = p0.y + t0 * dy;
						}

						return true;
					}
				}
			}
		}

		clippedP1 = clippedP0 = true;
		return false;
	}

	/// <summary>
	/// Liang-Barsky Line Clipping Test
	/// </summary>
	private static bool ClipTest(float p, float q, ref float t0, ref float t1)
	{
		float u = q / p;

		if (p < 0.0f)
		{
			if (u > t1)
				return false;
			if (u > t0)
				t0 = u;
		}
		else if (p > 0.0f)
		{
			if (u < t0)
				return false;
			if (u < t1)
				t1 = u;
		}
		else if (q < 0.0f)
			return false;

		return true;
	}

	#endregion

	#region Texture Utilities

	/// <summary>
	/// Create a 1x1 tex with color col
	/// </summary>
	public static Texture2D ColorToTex(int pxSize, Color col)
	{
		Color[] texCols = new Color[pxSize * pxSize];
		for (int px = 0; px < pxSize * pxSize; px++)
			texCols[px] = col;
		Texture2D tex = new Texture2D(pxSize, pxSize);
		tex.SetPixels(texCols);
		tex.Apply();
		return tex;
	}

	/// <summary>
	/// Tint the texture with the color. It's advised to use ResourceManager.GetTintedTexture to account for doubles.
	/// </summary>
	public static Texture2D Tint(Texture2D tex, Color color)
	{
		Texture2D tintedTex = UnityEngine.Object.Instantiate(tex);
		for (int x = 0; x < tex.width; x++)
			for (int y = 0; y < tex.height; y++)
				tintedTex.SetPixel(x, y, tex.GetPixel(x, y) * color);
		tintedTex.Apply();
		return tintedTex;
	}

	/// <summary>
	/// Rotates the texture Counter-Clockwise, 'quarterSteps' specifying the times
	/// </summary>
	public static Texture2D RotateTextureCCW(Texture2D tex, int quarterSteps)
	{
		if (tex == null)
			return null;
		// Copy and setup working arrays
		tex = UnityEngine.Object.Instantiate(tex);
		int width = tex.width, height = tex.height;
		Color[] col = tex.GetPixels();
		Color[] rotatedCol = new Color[width * height];
		for (int itCnt = 0; itCnt < quarterSteps; itCnt++)
		{ // For each iteration, perform rotation of 90 degrees
			for (int x = 0; x < width; x++)
				for (int y = 0; y < height; y++)
					rotatedCol[x * width + y] = col[(width - y - 1) * width + x];
			rotatedCol.CopyTo(col, 0); // Push rotation for next iteration
		}
		// Apply rotated working arrays
		tex.SetPixels(col);
		tex.Apply();
		return tex;
	}

	#endregion
}