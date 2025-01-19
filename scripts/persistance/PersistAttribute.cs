using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class PersistAttribute : Attribute {
    public string Name { get; }

    public PersistAttribute(string name = null) {
        Name = name;
    }
}