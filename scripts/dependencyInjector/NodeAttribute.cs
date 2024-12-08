using System;

/**
 * Convenient way to reference child nodes on a scene script.
 *
 * Simply attach the [Node] attribute to a field/property to have the 
 * property set to the result of calling GetNode.
 *
 * The node will be found using the following search:
 * 1. If the optional path parameter is supplied, we use that
 * 2. If the path parameter is not supplied, look for a node with a name
 * that matches the name of the attached field/property
 * 3. If there is no matching node, look for a node with a name that
 * matches the type of the attached field/property
 *
 * If no direct match is found, we then optionally search children using the
 * same logic doing a breadth first search.
 */
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class NodeAttribute : Attribute {
    public string Path { get; }
    public bool SearchChildren { get; }

    public NodeAttribute(string path = null, bool searchChildren = true) {
        Path = path;
        SearchChildren = searchChildren;
    }
}