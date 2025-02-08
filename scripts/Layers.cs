
using System;
using System.Collections.ObjectModel;
using Godot;

public class Layers {
    private static readonly Layers[] layers = new Layers[32];

    public static readonly Layers Ship = new(1, "Ship");
    public static class Module {
        public static readonly Layers Body = new(2, "Module.Body");
        public static class Snap {
            public static readonly Layers All = new(10, "Module.Snap");

            public static readonly Layers NE = new(11, "Module.Snap.NE");
            public static readonly Layers NW = new(12, "Module.Snap.NW");
            public static readonly Layers E = new(13, "Module.Snap.E");
            public static readonly Layers W = new(14, "Module.Snap.W");
            public static readonly Layers SE = new(15, "Module.Snap.SE");
            public static readonly Layers SW = new(16, "Module.Snap.SW");
        }
    }

    private readonly int index;
    private readonly uint mask;
    private readonly string name;

    private Layers(int index, string name) {
        this.index = index;
        this.mask = (uint)(1 << (index - 1));
        this.name = name;

        layers[index - 1] = this;
    }

    public static implicit operator StringName(Layers layer) => layer.name;
    public static implicit operator string(Layers layer) => layer.name;
    public static implicit operator uint(Layers layer) => layer.mask;

    public static string ToString(uint mask) {
        string result = "";
        for (int i = 0; i < 32; ++i) {
            if ((mask & (1 << i)) != 0) {
                Layers layer = layers[i];
                result += (layer != null ? layers[i].name : $"Unknown({i + 1})") + ",";
            }
        }

        if (result.Length > 0) {
            result = result.Substring(0, result.Length - 1);
        }

        return "[" + result + "]";
    }
}
