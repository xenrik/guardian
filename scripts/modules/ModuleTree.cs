using System.Collections.Generic;
using Godot;
using Godot.Collections;

public class ModuleTree {
    private ModuleTreeNode rootNode;

    private ModuleTree(ModuleTreeNode rootNode) {
        this.rootNode = rootNode;
    }

    public bool Save(string filename) {
        using (var file = FileUtils.Open(filename, FileAccess.ModeFlags.Write)) {
            if (file == null) {
                Logger.Error("Cannot open output file: " + filename);
                return false;
            }

            Dictionary data = new Dictionary();
            data["rootNode"] = rootNode.ToDataMap();

            string json = Json.Stringify(data, "   ");
            file.StoreString(json);
        }

        return true;
    }

    public static ModuleTree ToModuleTree(Module module) {
        ModuleTreeNode node = ToNode(module);
        return new(node);
    }

    public static ModuleTree Load(string filename) {
        return null;
    }

    private static ModuleTreeNode ToNode(Module module) {
        ModuleTreeNode node = new();
        node.ModuleId = module.ModuleDef.ModuleId;

        List<ModuleTreeNode> childNodes = new();
        foreach (Module childModule in module.GetChildren<Module>()) {
            childNodes.Add(ToNode(childModule));
        }
        if (childNodes.Count > 0) {
            node.ChildNodes = childNodes.ToArray();
        }

        return node;
    }

    private class ModuleTreeNode {
        public string ModuleId;
        public ModuleTreeNode[] ChildNodes;

        public Dictionary ToDataMap() {
            Dictionary data = new Dictionary();
            data["ModuleId"] = ModuleId;
            if (ChildNodes != null && ChildNodes.Length > 0) {
                var childData = new Array();
                foreach (var node in ChildNodes) {
                    childData.Add(node.ToDataMap());
                }

                data["Nodes"] = childData;
            }

            return data;
        }
    }
}
