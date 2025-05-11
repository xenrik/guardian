using Godot;

public partial class TestArea : Node3D {
    public static readonly GlobalData.Key<string> TEST_SHIP_PATH_DATA = new("TestArea.TestShipPath");

    [Singleton]
    private GlobalData GlobalData;

    [Singleton]
    private ModuleRegistry ModuleRegistry;

    public override void _Ready() {
        base._Ready();

        string testShipPath = GlobalData.GetData(TEST_SHIP_PATH_DATA);
        if (testShipPath == null) {
            Logger.Error("Test Ship Path was not supplied!");
            return;
        }

        ModuleTree tree = ModuleTree.Load(testShipPath);
        if (tree == null) {
            Logger.Error("Failed to load test ship");
            return;
        }

        Module rootNode = tree.Instantiate(ModuleRegistry);
        AddChild(rootNode);
    }

}
