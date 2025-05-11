using System.Collections.Generic;
using Godot;
using Godot.Collections;

public class ModuleTree {
    private const string CURRENT_VERSION = "1.0";

    private const string VERSION_PROP = "version";
    private const string ROOT_NODE_PROP = "rootNode";

    private ModuleTreeNode rootNode;

    public static ModuleTree ToModuleTree(Module module) {
        return new(ToNode(module, true));
    }

    private static ModuleTreeNode ToNode(Module module, bool isRoot) {
        ModuleTreeNode node = new();
        node.ModuleId = module.ModuleDef.ModuleId;
        if (!isRoot) {
            node.Transform = module.Transform;
        }

        foreach (Module childModule in module.GetChildren<Module>()) {
            node.ChildNodes.Add(ToNode(childModule, false));
        }

        return node;
    }

    public static ModuleTree Load(string filename) {
        string json;
        using (var file = FileUtils.Open(filename, FileAccess.ModeFlags.Read)) {
            if (file == null) {
                Logger.Error("Cannot open input file: " + filename);
                return null;
            }

            json = file.GetAsText();
        }

        Dictionary data = (Dictionary)Json.ParseString(json);
        string version = (string)data.GetValueOrDefault(VERSION_PROP);
        if (version == null) {
            Logger.Error("Module tree is missing a version: " + filename);
            return null;
        }

        if (!version.Equals(CURRENT_VERSION)) {
            Logger.Error("Upgrading module tree from version: " + version);
            data = ModuleTreeUpgrader.GetUpgrader(version).Upgrade(data);
        }

        Dictionary rootNodeData = (Dictionary)data.GetValueOrDefault(ROOT_NODE_PROP);
        if (rootNodeData == null) {
            Logger.Error("Module tree is missing a root node: " + filename);
            return null;
        }

        ModuleTreeNode rootNode = new();
        try {
            rootNode.FromDataMap(rootNodeData);
        } catch (MissingRequiredDataException e) {
            Logger.Error("Module tree is missing a required property: " + e);
            return null;
        }

        return new(rootNode);
    }

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
            data[VERSION_PROP] = CURRENT_VERSION;
            data[ROOT_NODE_PROP] = rootNode.ToDataMap();

            string json = Json.Stringify(data, "   ");
            file.StoreString(json);
        }

        return true;
    }

    public Module Instantiate(ModuleRegistry registry) {
        return Instantiate(registry, rootNode);
    }

    private Module Instantiate(ModuleRegistry registry, ModuleTreeNode treeNode) {
        Logger.Debug("Instantiate: " + treeNode.ModuleId);

        Module node = registry.Instantiate(treeNode.ModuleId);
        if (node == null) {
            Logger.Debug("No node for: " + treeNode.ModuleId);
            return null;
        }

        Logger.Debug("Before: " + node.Transform + " - tree has: " + treeNode.Transform);
        node.Transform = treeNode.Transform;
        Logger.Debug("After: " + node.Transform);

        foreach (ModuleTreeNode childTreeNode in treeNode.ChildNodes) {
            Module childNode = Instantiate(registry, childTreeNode);
            if (childNode != null) {
                //childNode.Reparent(node);
                node.AddChild(childNode);
            }
        }

        Logger.Debug("Returning: " + treeNode.ModuleId);
        return node;
    }

    private class MissingRequiredDataException : System.Exception {
        public MissingRequiredDataException(string message) : base(message) { }
    }

    private class ModuleTreeNode {
        private const string MODULE_ID_PROP = "ModuleId";
        private const string TRANSFORM_PROP = "Transform";
        private const string CHILD_NODES_PROP = "Nodes";

        public string ModuleId;
        public Transform3D Transform = Transform3D.Identity;

        public List<ModuleTreeNode> ChildNodes = new();

        public Dictionary ToDataMap() {
            Dictionary data = new Dictionary();
            data[MODULE_ID_PROP] = ModuleId;
            if (Transform != Transform3D.Identity) {
                data[TRANSFORM_PROP] = SerializerUtils.ToJson(Transform);
            }

            if (ChildNodes.Count > 0) {
                var childData = new Array();
                foreach (var node in ChildNodes) {
                    childData.Add(node.ToDataMap());
                }

                data[CHILD_NODES_PROP] = childData;
            }

            return data;
        }

        public void FromDataMap(Dictionary data) {
            ModuleId = GetRequiredData<string>(data, MODULE_ID_PROP);
            Transform = GetOptionalData(data, TRANSFORM_PROP, Transform3D.Identity);

            ChildNodes = new();
            Array nodes = GetOptionalData<Array>(data, CHILD_NODES_PROP);
            if (nodes != null) {
                foreach (Dictionary childData in nodes) {
                    ModuleTreeNode childNode = new();
                    childNode.FromDataMap(childData);

                    ChildNodes.Add(childNode);
                }
            }
        }

        private T GetRequiredData<[MustBeVariant] T>(Dictionary data, string key) {
            if (data.ContainsKey(key)) {
                Logger.Debug("Reading property: " + key + " -> " + data[key]);

                Variant v = data[key];
                return SerializerUtils.FromJson<T>(v);
            }

            throw new MissingRequiredDataException("Property " + key + " is missing");
        }

        private T GetOptionalData<[MustBeVariant] T>(Dictionary data, string key, T defaultValue = default) {
            if (data.ContainsKey(key)) {
                return GetRequiredData<T>(data, key);
            } else {
                Logger.Debug("Missing optional property: " + key);
                return defaultValue;
            }
        }
    }
}
