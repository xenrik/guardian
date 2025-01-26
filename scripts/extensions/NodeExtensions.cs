using Godot;
using System.Collections.Generic;
using System.Linq;

public static class NodeExtensions {
    /**
     * Return true if we should recurse
     */
    public delegate bool TreeWalker(Node node);

    /**
     * Walk the tree of nodes from this node. Breadth first.
     */
    public static void WalkTree(this Node node, TreeWalker consumer, bool includeInternal = false) {
        var nodesToWalk = new Queue<Node>();
        nodesToWalk.Enqueue(node);

        while (nodesToWalk.Count > 0) {
            var currentNode = nodesToWalk.Dequeue();
            if (currentNode == node || consumer.Invoke(currentNode)) {
                foreach (var child in currentNode.GetChildren(includeInternal)) {
                    nodesToWalk.Enqueue(child);
                }
            }
        }
    }

    /**
     * Iterate the tree of nodes from this node. Breadth first
     */
    public static IEnumerable<Node> IterateTree(this Node node, bool includeInternal = false) {
        var nodesToWalk = new Queue<Node>();
        nodesToWalk.Enqueue(node);

        while (nodesToWalk.Count > 0) {
            var currentNode = nodesToWalk.Dequeue();
            if (currentNode != node) {
                yield return currentNode;
            }

            foreach (var child in currentNode.GetChildren(includeInternal)) {
                nodesToWalk.Enqueue(child);
            }
        }
    }

    /**
     * Returns true if this node is parented by the given node. 
     */
    public static bool HasParent(this Node node, Node test) {
        Node parent = node.GetParent();
        while (parent != null) {
            if (parent == test) {
                return true;
            }

            parent = parent.GetParent();
        }

        return false;
    }

    /**
     * Return the a parent of this node that has the given type searching recursively 
     */
    public static T FindParent<[MustBeVariant] T>(this Node node) where T : Node {
        node = node.GetParent();
        while (node != null) {
            if (node is T) {
                return (T)node;
            }

            node = node.GetParent();
        }

        // Not found
        return null;
    }

    /**
     * Generic version of FindChild
     */
    public static T FindChild<[MustBeVariant] T>(this Node node, string pattern = "", bool recursive = true, bool owned = true) where T : Node {
        if (pattern == "") {
            foreach (Node n in node.IterateTree()) {
                if (n is T) {
                    return (T)n;
                }
            }

            return null;
        } else {
            Node childNode = node.FindChild(pattern, recursive, owned);
            return childNode is T ? (T)childNode : null;
        }
    }

    /**
     * Generic version of FindChildren
     */
    public static Godot.Collections.Array<T> FindChildren<[MustBeVariant] T>(this Node node, string pattern = "", bool recursive = true, bool owned = true) where T : Node {
        Godot.Collections.Array<T> typedArray = new();
        if (pattern == "") {
            node.WalkTree(n => {
                if (n is T) {
                    typedArray.Add((T)n);
                }
                return true;
            });
        } else {
            Godot.Collections.Array<Node> rawArray = node.FindChildren(pattern, "", recursive, owned);
            foreach (Node n in rawArray) {
                if (n is T) {
                    typedArray.Add((T)n);
                }
            }
        }

        return typedArray;
    }

    /**
     * Generic version of GetChildren. Only returns children which the matching type, allows for a recursive search
     */
    public static Godot.Collections.Array<T> GetChildren<[MustBeVariant] T>(this Node node, bool includeInternal = false) where T : Node {
        Godot.Collections.Array<T> filteredChildren = new();
        var children = node.GetChildren(includeInternal);
        foreach (Node child in children) {
            if (child is T) {
                filteredChildren.Add((T)child);
            }
        }

        return filteredChildren;
    }
}