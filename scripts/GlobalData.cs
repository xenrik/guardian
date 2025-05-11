using System.Collections.Generic;
using Godot;

[Singleton]
public partial class GlobalData : Node {
    public enum Scope {
        GLOBAL, SCENE
    }

    public abstract class Key {
        public Scope Scope { get; private set; }
        private string key;

        protected Key(string key, Scope scope) {
            this.key = key;
            Scope = scope;
        }

        public override int GetHashCode() {
            return key.GetHashCode();
        }

        public override bool Equals(object obj) {
            return obj is Key && ((Key)obj).key.Equals(key);
        }
    }
    public class Key<T> : Key {
        public Key(string key, Scope scope = Scope.GLOBAL) : base(key, scope) {
        }
    }

    private Dictionary<Key, object> dataMap = new();

    public void SetData<T>(Key<T> key, T value) {
        dataMap[key] = value;
    }

    public T GetData<T>(Key<T> key, T defaultValue = default) {
        if (dataMap.ContainsKey(key)) {
            return (T)dataMap[key];
        } else {
            return defaultValue;
        }
    }
}
