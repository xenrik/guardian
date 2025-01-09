using Godot;
using System.Collections.Generic;

public static class NodeExtensions {
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
     * Generic version of FindChildren
     */
    public static Godot.Collections.Array<T> FindChildren<[MustBeVariant] T>(this Node node, string pattern, bool recursive = true, bool owned = true) where T : class {
        Godot.Collections.Array<Node> rawArray = node.FindChildren(pattern, "", recursive, owned);
        Godot.Collections.Array<T> typedArray = new();

        foreach (Node n in rawArray) {
            if (n is T) {
                typedArray.Add(n as T);
            }
        }

        return typedArray;
    }
}