using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/// Used when tree walking. Return a Result enum to control what
/// happens after this node is processed
namespace TreeWalker {
    public delegate Result Walker(Node node);

    public enum Result {
        /** Walk into children of this node */
        RECURSE,

        /** Skip children of this node */
        SKIP_CHILDREN,

        /** Immediately stop walking */
        STOP
    };
}

public static class NodeExtensions {
    /// Walk the tree of nodes from this node. Breadth first.
    public static void WalkTree(this Node node, TreeWalker.Walker consumer, bool includeQueuedForDeletion = false) {
        var nodesToWalk = new Queue<Node>();
        nodesToWalk.Enqueue(node);

        while (nodesToWalk.Count > 0) {
            var currentNode = nodesToWalk.Dequeue();
            if (currentNode.IsQueuedForDeletion() && !includeQueuedForDeletion) {
                continue;
            }

            TreeWalker.Result result = (currentNode == node) ? TreeWalker.Result.RECURSE : consumer.Invoke(currentNode);
            switch (result) {
                case TreeWalker.Result.STOP:
                    return;

                case TreeWalker.Result.RECURSE:
                    foreach (var child in currentNode.GetChildren(false)) {
                        nodesToWalk.Enqueue(child);
                    }
                    break;

                default:
                    break;
            }
        }
    }

    /// Iterate the tree of nodes from this node. Breadth first
    public static IEnumerable<Node> IterateTree(this Node node, bool includeQueuedForDeletion = false) {
        var nodesToWalk = new Queue<Node>();
        nodesToWalk.Enqueue(node);

        while (nodesToWalk.Count > 0) {
            var currentNode = nodesToWalk.Dequeue();
            if (currentNode.IsQueuedForDeletion() && !includeQueuedForDeletion) {
                continue;
            }
            if (currentNode != node) {
                yield return currentNode;
            }

            foreach (var child in currentNode.GetChildren(false)) {
                nodesToWalk.Enqueue(child);
            }
        }
    }

    /// Returns true if this node is parented by the given node. 
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

    /// Return the closest parent of this node that has the given type searching recursively 
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

    /// Generic version of FindChild. 
    /// 
    /// This does no use the built-in FindChild, and instead:
    /// - Uses a bread-first search
    /// - Will always find "unowned" children
    public static T FindChild<[MustBeVariant] T>(this Node node, string pattern = "", bool recursive = true, bool includeQueuedForDeletion = false) where T : Node {
        var regex = pattern == "" ? null : new Regex(Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", "."));
        T matchedNode = null;
        node.WalkTree(node => {
            if (node.IsQueuedForDeletion() && !includeQueuedForDeletion) {
                // Exclude this node and any children
                return TreeWalker.Result.SKIP_CHILDREN;
            }

            if (node is T && (regex == null || regex.IsMatch(node.Name))) {
                matchedNode = (T)node;
                return TreeWalker.Result.STOP;
            }

            // Search children (if allowed)
            return recursive ? TreeWalker.Result.RECURSE : TreeWalker.Result.SKIP_CHILDREN;
        });

        return matchedNode;
    }

    /// Generic version of FindChildren. 
    /// 
    /// This does no use the built-in FindChildren, and instead:
    /// - Uses a bread-first search
    /// - Will always find "unowned" children
    public static Godot.Collections.Array<T> FindChildren<[MustBeVariant] T>(this Node node, string pattern = "", bool recursive = true, bool includeQueuedForDeletion = false) where T : Node {
        var regex = pattern == "" ? null : new Regex(Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", "."));
        Godot.Collections.Array<T> matchedNodes = new();
        node.WalkTree(node => {
            if (node.IsQueuedForDeletion() && !includeQueuedForDeletion) {
                // Exclude this node and any children
                return TreeWalker.Result.SKIP_CHILDREN;
            }

            if (node is T && (regex == null || regex.IsMatch(node.Name))) {
                matchedNodes.Add((T)node);
            }

            // Search children (if allowed)
            return recursive ? TreeWalker.Result.RECURSE : TreeWalker.Result.SKIP_CHILDREN;
        });

        return matchedNodes;
    }

    /// Generic version of GetChildren. Only returns children which the matching type - this is not recursive!
    public static Godot.Collections.Array<T> GetChildren<[MustBeVariant] T>(this Node node) where T : Node {
        Godot.Collections.Array<T> filteredChildren = new();
        var children = node.GetChildren(false);
        foreach (Node child in children) {
            if (child is T) {
                filteredChildren.Add((T)child);
            }
        }

        return filteredChildren;
    }
}