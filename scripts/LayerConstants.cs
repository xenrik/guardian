
public class LayerConstants {
    public static readonly LayerConstants ModuleBody = new(2, "Module-Body");

    public static readonly LayerConstants Snap = new(10, "Snap");
    public static readonly LayerConstants Snap_NE = new(11, "Snap-NE");
    public static readonly LayerConstants Snap_NW = new(12, "Snap-NW");
    public static readonly LayerConstants Snap_E = new(13, "Snap-E");
    public static readonly LayerConstants Snap_W = new(14, "Snap-W");
    public static readonly LayerConstants Snap_SE = new(15, "Snap-SE");
    public static readonly LayerConstants Snap_SW = new(16, "Snap-SW");

    public readonly int Index;
    public readonly string Name;

    private LayerConstants(int index, string name) {
        this.Index = index;
        this.Name = name;
    }
}
