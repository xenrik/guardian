
using System;
using Godot;

public class Groups {
    public static class Module {
        public static readonly Groups Body = new("Module.Body");
        public static readonly Groups Snap = new("Module.Snap");
    }

    private string name;
    private Groups(string name) {
        this.name = name;
    }

    public Predicate<T> Filter<T>() where T : Node {
        return (node) => node.IsInGroup(name);
    }

    public static implicit operator StringName(Groups g) => g.name;
    public static implicit operator string(Groups g) => g.name;
}