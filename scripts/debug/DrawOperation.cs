using Godot;

public interface DrawOperation {
    public float Timeout { get; set; }

    public void Draw(CanvasItem canvas);
}