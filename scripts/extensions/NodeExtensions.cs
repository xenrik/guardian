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
}