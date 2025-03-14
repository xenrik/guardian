using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

/**
 * This node manages dependency injection of the Node and Singleton annotated 
 * fields/properties
 * 
 * It should be registered as an "autoloaded" script. This class will automatically
 * update nodes after they are added onto the tree. If you need to access singleton or
 * node properties in a _EnterTree lifecycle method you will need manually call Update
 * to get them loaded properly.
 */
public partial class DependencyInjector : Node {
    private Dictionary<Type, Node> singletonsByType = new Dictionary<Type, Node>();
    private Dictionary<string, Node> singletonsByName = new Dictionary<string, Node>();

    private static DependencyInjector instance;

    public static void Update(Node node) {
        if (instance == null) {
            throw new Exception("Dependency Injector has not been setup");
        }

        instance.DoUpdate(node);
    }

    public static T GetSingleton<T>() where T : class {
        if (instance == null) {
            throw new Exception("Dependency Injector has not been setup");
        }

        return instance.singletonsByType[typeof(T)] as T;
    }

    public override void _EnterTree() {
        base._EnterTree();

        instance = this;
        GetTree().Connect(SceneTree.SignalName.NodeAdded, Callable.From<Node>(OnNodeAdded));
        GetTree().Connect(SceneTree.SignalName.NodeRemoved, Callable.From<Node>(OnNodeRemoved));
    }

    public override void _ExitTree() {
        if (instance == this) {
            instance = null;
        }

        GetTree().Disconnect(SceneTree.SignalName.NodeAdded, Callable.From<Node>(OnNodeAdded));
        GetTree().Disconnect(SceneTree.SignalName.NodeRemoved, Callable.From<Node>(OnNodeRemoved));

        base._ExitTree();
    }

    private void OnNodeAdded(Node node) {
        DoUpdate(node);
    }

    private void OnNodeRemoved(Node node) {
        Type t = node.GetType();

        SingletonAttribute[] attrs = (SingletonAttribute[])t.GetCustomAttributes(typeof(SingletonAttribute), false);
        foreach (var attr in attrs) {
            Node current;
            if (singletonsByType.TryGetValue(t, out current) &&
                    current == node) {
                singletonsByType.Remove(t);
            }

            if (attr.Name != null &&
                    singletonsByName.TryGetValue(attr.Name, out current) &&
                    current == node) {
                singletonsByName.Remove(attr.Name);
            }
        }
    }

    /**
     * Call this to populate/update the annotated fields and properties on the given node
     */
    public void DoUpdate(Node node) {
        Type t = node.GetType();

        // Is the node a singleton?
        SingletonAttribute[] attrs = (SingletonAttribute[])t.GetCustomAttributes(typeof(SingletonAttribute), false);
        foreach (var attr in attrs) {
            singletonsByType[t] = node;
            if (attr.Name != null) {
                singletonsByName[attr.Name] = node;
            }
        }

        // Search for annotated fields and properties
        var fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var properties = t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var element in fields.Cast<MemberInfo>().Concat(properties)) {
            UpdateSingletonMembers(node, element);
            UpdateSingletonAttributes(node, element);
            UpdateNodeAttributes(node, element);
        }
    }

    private void UpdateSingletonMembers(Node node, MemberInfo member) {
        Type type = (member is FieldInfo) ? ((FieldInfo)member).FieldType : ((PropertyInfo)member).PropertyType;
        if (!type.IsGenericType) {
            return;
        }

        Type genericType = type.GetGenericTypeDefinition();
        if (genericType.IsAssignableFrom(typeof(Singleton<>))) {
            Type singletonType = type.GenericTypeArguments[0];

            string name = null;
            SingletonAttribute[] attrs = (SingletonAttribute[])member.GetCustomAttributes(typeof(SingletonAttribute), false);
            if (attrs.Length > 0) {
                name = attrs[0].Name;
            }

            var accessorType = typeof(SingletonAccessor<>).MakeGenericType(singletonType);
            var accessor = Activator.CreateInstance(accessorType, this, node, name, singletonType);

            if (member is FieldInfo) {
                ((FieldInfo)member).SetValue(node, accessor);
            } else {
                ((PropertyInfo)member).SetValue(node, accessor);
            }
        }
    }

    private void UpdateSingletonAttributes(Node node, MemberInfo member) {
        SingletonAttribute[] attrs = (SingletonAttribute[])member.GetCustomAttributes(typeof(SingletonAttribute), false);
        Type type = (member is FieldInfo) ? ((FieldInfo)member).FieldType : ((PropertyInfo)member).PropertyType;

        // Don't look at Singleton<> properties here
        if (type.IsAssignableFrom(typeof(Singleton<>))) {
            return;
        }

        foreach (var attr in attrs) {
            Node singleton;
            if (attr.Name != null) {
                if (!singletonsByName.TryGetValue(attr.Name, out singleton)) {
                    throw new Exception($"Could not find singleton with name: {attr.Name} - initialising node: {node.GetPath()}");
                }
            } else if (!singletonsByType.TryGetValue(type, out singleton)) {
                throw new Exception($"Could not find singleton with type: {type.Name} - initialising node: {node.GetPath()}");
            }

            if (member is FieldInfo) {
                ((FieldInfo)member).SetValue(node, singleton);
            } else {
                ((PropertyInfo)member).SetValue(node, singleton);
            }
        }
    }

    private void UpdateNodeAttributes(Node node, MemberInfo member) {
        NodeAttribute[] attrs = (NodeAttribute[])member.GetCustomAttributes(typeof(NodeAttribute), false);
        foreach (var attr in attrs) {
            var filters = new List<NodeFilter>();
            string path = attrs[0].Path;
            Type type = (member is FieldInfo) ? ((FieldInfo)member).FieldType : ((PropertyInfo)member).PropertyType;

            // If path is set, we only use that
            if (path != null) {
                filters.Add((node) => node.GetNodeOrNull(path));
            } else {
                // Otherwise try by name first
                var matchName = member.Name.ToLower();
                filters.Add((node) => {
                    foreach (var child in node.GetChildren()) {
                        if (child.Name.ToString().ToLower() == matchName) {
                            return child;
                        }
                    }

                    return null;
                });

                // Then by type
                filters.Add((node) => {
                    return type.IsAssignableFrom(node.GetType()) ? node : null;
                });
            }

            Node childNode = FindNode(node, attr.SearchChildren, filters.ToArray());
            if (childNode == null) {
                string recursive = $"(Searched children: {attr.SearchChildren}";
                if (path != null) {
                    throw new Exception($"Could not find node with path: {path} - from node: {node.GetPath()} {recursive}");
                } else {
                    throw new Exception($"Could not find node with name: {member.Name} or type: {type.Name} - from node: {node.GetPath()} {recursive}");
                }
            }

            if (member is FieldInfo) {
                ((FieldInfo)member).SetValue(node, childNode);
            } else {
                ((PropertyInfo)member).SetValue(node, childNode);
            }
        }
    }

    private delegate Node NodeFilter(Node node);
    private Node FindNode(Node rootNode, bool recurse, params NodeFilter[] filters) {
        foreach (NodeFilter filter in filters) {
            Queue<Node> toSearch = new Queue<Node>();
            toSearch.Enqueue(rootNode);

            Node matchedNode = null;
            while (toSearch.Count > 0 && matchedNode == null) {
                Node current = toSearch.Dequeue();
                Node node = filter(current);
                if (node != null) {
                    return node;
                } else if (recurse) {
                    foreach (Node child in current.GetChildren()) {
                        toSearch.Enqueue(child);
                    }
                }
            }
        }

        return null;
        /*
                    if (path != null) {
                        matchedNode = current.GetNodeOrNull(path);
                    } else {
                        matchedNode = current.GetNodeOrNull(name);

                        if (matchedNode == null && type.IsAssignableFrom(current.GetType())) {
                            matchedNode = current;
                        }
                    }

                    if (matchedNode == null && recurse) {
                        foreach (Node child in current.GetChildren()) {
                            toSearch.Enqueue(child);
                        }
                    }
                }

                return matchedNode;
            }
            */
    }


    /**
    * Accessor class used to lazy load singletons
    */
    private class SingletonAccessor<T> : Singleton<T> where T : class {
        public T Get() {
            if (resolved) {
                return this.singleton;
            }

            resolved = true;
            Node singleton;
            if (name != null) {
                if (!injector.singletonsByName.TryGetValue(name, out singleton)) {
                    throw new Exception($"Could not find singleton with name: {name} - initialising node: {node.GetPath()}");
                }
            } else if (!injector.singletonsByType.TryGetValue(type, out singleton)) {
                throw new Exception($"Could not find singleton with type: {type.Name} - initialising node: {node.GetPath()}");
            }

            this.singleton = singleton as T;
            return this.singleton;
        }

        private DependencyInjector injector;
        private Node node;
        private string name;
        private Type type;

        private bool resolved;
        private T singleton;

        public SingletonAccessor(DependencyInjector injector, Node node, string name, Type type) {
            this.injector = injector;
            this.node = node;
            this.name = name;
            this.type = type;
        }
    }
}