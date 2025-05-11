using System.Collections.Generic;
using Godot;

[Singleton]
public partial class ModuleRegistry : Node {
    [Export]
    public ModuleRegistryElement[] Elements;

    private Dictionary<string, ModuleRegistryElement> elementMap = new();

    public override void _Ready() {
        base._Ready();

        elementMap.Clear();

        bool valid = true;
        foreach (ModuleRegistryElement elem in Elements) {
            var id = elem.Definition.ModuleId;

            if (elementMap.ContainsKey(id)) {
                Logger.Error("The registry has multiple modules with the same id: " + id);
                valid = false;
            }
            elementMap[id] = elem;

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

    public Module Instantiate(string moduleId) {
        if (elementMap.ContainsKey(moduleId)) {
            return (Module)elementMap[moduleId].Scene.Instantiate();
        }

        return null;
    }
}