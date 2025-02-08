using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// NodeSet is a simple set of nodes. It has extra logic to ensure you cannot add disposed nodes,
/// and if you ask for its content it will silently skip disposed nodes.
/// </summary>
/// <typeparam name="T"></typeparam>
public class NodeSet<T> : IEnumerable<T> where T : Node {
    private HashSet<T> innerSet = new();
    public IReadOnlySet<T> Nodes {
        get {
            CleanUp();
            return innerSet.AsReadOnly();
        }
    }

    public int Count {
        get {
            CleanUp();
            return innerSet.Count();
        }
    }

    private ulong lastCheckedFrame = 0;
    private List<T> removeList = new();

    public void Add(T node) {
        if (!GodotObject.IsInstanceValid(node)) {
            // Getting the name will probably cause another exception, but this makes sure!
            throw new ObjectDisposedException(node.Name);
        }

        innerSet.Add(node);
    }

    public void Remove(T node) {
        innerSet.Remove(node);
    }

    public IEnumerator GetEnumerator() {
        return (IEnumerator)DoGetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        return DoGetEnumerator();
    }

    private IEnumerator<T> DoGetEnumerator() {
        CleanUp();
        return innerSet.GetEnumerator();
    }

    private void CleanUp() {
        if (lastCheckedFrame == Engine.GetProcessFrames()) {
            return;
        }

        lastCheckedFrame = Engine.GetProcessFrames();
        innerSet.RemoveWhere(node => !GodotObject.IsInstanceValid(node));
    }
}