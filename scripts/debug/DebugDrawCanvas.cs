using System.Collections.Generic;
using Godot;

public partial class DebugDrawCanvas : Node2D {
    private static DebugDrawCanvas instance;
    private static List<DrawOperation> operations = new List<DrawOperation>();

    public static void AddOperation(DrawOperation op) {
        operations.Add(op);
        instance.QueueRedraw();
    }

    public override void _Ready() {
        base._Ready();

        instance = this;
    }

    public override void _Process(double delta) {
        base._Process(delta);

        bool redraw = false;
        float fDelta = (float)delta;
        foreach (DrawOperation op in operations) {
            op.Timeout -= fDelta;
            if (op.Timeout <= 0) {
                redraw = true;
            }
        }

        if (redraw) {
            QueueRedraw();
        }
    }

    public override void _Draw() {
        base._Draw();

        foreach (DrawOperation op in operations) {
            op.Draw(this);
        }

        int removed = operations.RemoveAll(e => {
            return e.Timeout <= 0;
        });
        if (removed > 0) {
            QueueRedraw();
        }
    }
}