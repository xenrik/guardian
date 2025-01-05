using Godot;

public class DrawLineOperation : DrawOperation {
    public float Timeout { get; set; }

    private Vector2 start;
    private Vector2 end;
    private int width;
    private Color color;

    public DrawLineOperation(Vector2 start, Vector2 end, Color color, int width, float timeout) {
        this.start = start;
        this.end = end;
        this.color = color;
        this.width = width;
        this.Timeout = timeout;
    }

    public void Draw(CanvasItem canvas) {
        canvas.DrawLine(start, end, color, width, false);
    }
}