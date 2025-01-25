using Godot;
using System.Collections.Generic;
using System.Linq;

public static class NodeExtensions {
    /**
     * Return true if we should recurse
     */
    public delegate bool TreeWalker(Node node);

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
    public static T FindChild<[MustBeVariant] T>(this Node node, string pattern, bool recursive = true, bool owned = true) where T : class {
        Node childNode = node.FindChild(pattern, recursive, owned);
        return childNode is T ? childNode as T : null;
    }

    /**
     * Generic version of FindChildren
     */
    public static Godot.Collections.Array<T> FindChildren<[MustBeVariant] T>(this Node node, string pattern = "", bool recursive = true, bool owned = true) where T : class {
        Godot.Collections.Array<T> typedArray = new();
        if (pattern == "") {
            // Find all children of the given type
            node.WalkTree(node => {
                if (node is T && (node.Owner != null || owned == false)) {
                    typedArray.Add(node as T);
                }

                return recursive && (node.Owner != null || owned == false);
            });
        } else {
            Godot.Collections.Array<Node> rawArray = node.FindChildren(pattern, "", recursive, owned);
            foreach (Node n in rawArray) {
                if (n is T) {
                    typedArray.Add(n as T);
                }
            }

        }

        return typedArray;
    }

    /**
     * Generic version of GetChildren. Only returns children which the matching type
     */
    public static Godot.Collections.Array<T> GetChildren<[MustBeVariant] T>(this Node node, bool includeInternal = false) where T : Node {
        var children = node.GetChildren(includeInternal);
        Godot.Collections.Array<T> filteredChildren = new();
        foreach (Node child in children) {
            if (child is T) {
                filteredChildren.Add((T)child);
            }
        }

        return filteredChildren;
    }
}