
using System;
using System.Collections.ObjectModel;

public class LayerConstants {
    private static readonly LayerConstants[] RawLayers = new LayerConstants[32];
    public static ReadOnlyCollection<LayerConstants> Layers {
        get {
            return Array.AsReadOnly(RawLayers);
        }
    }

    public static readonly LayerConstants ModuleBody = new(2, "Module-Body");

    public static readonly LayerConstants Snap = new(10, "Snap");
    public static readonly LayerConstants Snap_NE = new(11, "Snap-NE");
    public static readonly LayerConstants Snap_NW = new(12, "Snap-NW");
    public static readonly LayerConstants Snap_E = new(13, "Snap-E");
    public static readonly LayerConstants Snap_W = new(14, "Snap-W");
    public static readonly LayerConstants Snap_SE = new(15, "Snap-SE");
    public static readonly LayerConstants Snap_SW = new(16, "Snap-SW");

    public readonly int Index;
    public readonly uint Mask;
    public readonly string Name;

    private LayerConstants(int index, string name) {
        this.Index = index;
        this.Mask = (uint)(1 << (index - 1));
        this.Name = name;

        RawLayers[Index - 1] = this;
    }

    public static string ToString(uint mask) {
        string result = "";
        for (int i = 0; i < 32; ++i) {
            if ((mask & (1 << i)) != 0) {
                LayerConstants layer = Layers[i];
                result += (layer != null ? Layers[i].Name : $"Unknown({i + 1})") + ",";
            }
        }

        if (result.Length > 0) {
            result = result.Substring(0, result.Length - 1);
        }

        return "[" + result + "]";
    }
}
