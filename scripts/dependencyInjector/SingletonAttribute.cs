using System;

/**
 * If this attribute is set on a Node class, it marks it as a singleton. When
 * the node is attached to the tree a record of the instance is kept.
 * 
 * If this attribute is set on a field or property, the field/property will be
 * set to the current value of the singleton instance.
 *
 * The name parameter is optional - if not set, singletons are organised by their
 * type instead
 */
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
public class SingletonAttribute : Attribute {
    public string Name { get; }

    public SingletonAttribute(string name = null) {
        Name = name;
    }
}

/**
 * Allows the resolution of a singleton to be delayed until it's first access. This
 * is generally only needed where singletons have cyclic references to each other.
 *
 * Do not assign a value to the field/property -- this will happen automatically.
 *
 * You do not need to also use the @Singleton annotation, but you can if you need
 * to specify the Name of the singleton
 */
public interface Singleton<T> where T : class {
    public T Get();
}