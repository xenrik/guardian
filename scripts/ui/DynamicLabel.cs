using Godot;
using System;
using System.Collections.Generic;

public partial class DynamicLabel : Label {
    [Export]
    public Expression[] expressions = new Expression[0];

    [Export]
    public float UpdateSpeed = 1;

    private float update = 0;

    public override void _Process(double delta) {
        base._Process(delta);

        update += (float)delta;
        if (update < UpdateSpeed) {
            return;
        }

        var values = new Dictionary<string, string>();
        foreach (Expression expr in expressions) {
            expr.Resolve(this);
        }
    }
}
