using System.Collections.Generic;
using Godot;

public static class DebugDraw {
    public static void DrawLine(Vector2 start, Vector2 end, Color? color = null, int width = -1, float timeout = 0) {
        DebugDrawCanvas.AddOperation(new DrawLineOperation(start, end, color == null ? ColorConstants.RED : (Color)color, width, timeout));
    }
}