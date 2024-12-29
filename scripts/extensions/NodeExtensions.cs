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
}