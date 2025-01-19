using Godot;

[Singleton]
public partial class ModuleRegistry : Node {
    [Export]
    public ModuleRegistryElement[] Elements;

    public override void _Ready() {
        base._Ready();

        // Validate the registry if we in the editor
        if (OS.HasFeature("editor")) {
            foreach (ModuleRegistryElement elem in Elements) {
                var id = elem.ModuleId;

                Node gameNode = elem.GameScene.Instantiate();
                Assert.OfType<Module>(gameNode, $"The game scene for module {id} does not have the 'Module' script!")
                    .WithType((node) => {
                        Assert.Equals(id, node.ModuleId, $"The game scene for module {id} does not have a matching module id");
                    });

                Node editorNode = elem.EditorScene.Instantiate();
                Assert.OfType<EditorModule>(editorNode, $"The editor scene for module {id} does not have the 'EditorModule' script!")
                    .WithType((node) => {
                        Assert.Equals(id, node.ModuleId, $"The editor scene for module {id} does not have a matching module id");
                    });
            }
        }
    }
}