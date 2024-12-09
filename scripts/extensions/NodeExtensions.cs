using System;
using Godot;

public static class NodeExtensions {
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