using Godot;

[Singleton]
public partial class ModuleRegistry : Node {
    [Export]
    public ModuleRegistryElement[] Elements;

    public override void _Ready() {
        base._Ready();

        // Validate the registry if we in the editor
        if (OS.HasFeature("editor")) {
            bool valid = true;

            foreach (ModuleRegistryElement elem in Elements) {
                var id = elem.Definition.ModuleId;

                Node gameNode = elem.Scene.Instantiate();
                Assert.IsType<Module>(gameNode, $"The scene for module {id} does not have the 'Module' script!")
                    .WhenTrue((node) => {
                        valid = valid & Assert.NotNull(node.ModuleDef, $"The scene for module {id} does not have a module def");
                        valid = valid & Assert.AreEqual(id, node.ModuleDef.ModuleId, $"The scene for module {id} does not have a matching module id");
                    })
                    .WhenFalse((node) => valid = false);

                /*
                Node editorNode = elem.EditorScene.Instantiate();
                Assert.IsType<EditorModule>(editorNode, $"The editor scene for module {id} does not have the 'EditorModule' script!")
                    .WhenTrue((node) => {
                        valid = valid & Assert.AreEqual(id, node.ModuleDef.ModuleId, $"The editor scene for module {id} does not have a matching module id");
                    })
                    .WhenFalse((node) => valid = false);
                */
            }

            if (valid) {
                Logger.Debug("ModuleRegistry is valid");
            }
        }
    }
}