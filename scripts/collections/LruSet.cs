using System.Collections.Generic;

public class LruSet<V> {
    private LruMap<V, bool> innerMap;

    public LruSet(int capacity) {
        innerMap = new LruMap<V, bool>(capacity);
    }

    public void Add(V val) {
        innerMap.Set(val, true);
    }

    public void Contains(V val) {
        innerMap.ContainsKey(val);
    }
}