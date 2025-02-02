using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// NodeList is a simple list of nodes. It has extra logic to ensure you cannot add disposed nodes,
/// and if you ask for a list it will silently skip disposed nodes.
/// </summary>
/// <typeparam name="T"></typeparam>
public class NodeList<T> : IEnumerable<T> where T : Node {
    private List<T> innerList = new();
    public IList<T> Nodes {
        get {
            return innerList.AsReadOnly();
        }
    }

    public int Count {
        get {
            CleanUp();
            return innerList.Count();
        }
    }

    private ulong lastCheckedFrame = 0;

    public void Add(T node) {
        if (!GodotObject.IsInstanceValid(node)) {
            // Getting the name will probably cause another exception, but this makes sure!
            throw new ObjectDisposedException(node.Name);
        }

        innerList.Add(node);
    }

    public void Remove(T node) {
        innerList.Remove(node);
    }

    public IEnumerator GetEnumerator() {
        return (IEnumerator)DoGetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        return DoGetEnumerator();
    }

    public T this[int i] {
        get {
            CleanUp();
            return innerList[i];
        }
    }

    private IEnumerator<T> DoGetEnumerator() {
        CleanUp();
        return innerList.GetEnumerator();
    }

    private void CleanUp() {
        if (lastCheckedFrame == Engine.GetProcessFrames()) {
            return;
        }

        lastCheckedFrame = Engine.GetProcessFrames();
        for (int i = innerList.Count - 1; i >= 0; --i) {
            if (!GodotObject.IsInstanceValid(innerList.ElementAt(i))) {
                innerList.RemoveAt(i);
            }
        }
    }
}