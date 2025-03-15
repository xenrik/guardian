using System.Collections.Generic;

public class LruMap<K, V> {
    private int capacity;
    private Dictionary<K, LinkedListNode<LruMapItem<K, V>>> cacheMap = new Dictionary<K, LinkedListNode<LruMapItem<K, V>>>();
    private LinkedList<LruMapItem<K, V>> lruList = new LinkedList<LruMapItem<K, V>>();

    public LruMap(int capacity) {
        this.capacity = capacity;
    }

    public bool ContainsKey(K key) {
        return cacheMap.ContainsKey(key);
    }

    public V Get(K key) {
        LinkedListNode<LruMapItem<K, V>> node;
        if (cacheMap.TryGetValue(key, out node)) {
            V value = node.Value.value;
            lruList.Remove(node);
            lruList.AddLast(node);
            return value;
        }
        return default(V);
    }

    public void Set(K key, V val) {
        if (cacheMap.TryGetValue(key, out var existingNode)) {
            lruList.Remove(existingNode);
        } else if (cacheMap.Count >= capacity) {
            RemoveFirst();
        }

        LruMapItem<K, V> cacheItem = new LruMapItem<K, V>(key, val);
        LinkedListNode<LruMapItem<K, V>> node = new LinkedListNode<LruMapItem<K, V>>(cacheItem);
        lruList.AddLast(node);
        cacheMap[key] = node;
    }

    private void RemoveFirst() {
        // Remove from LRUPriority
        LinkedListNode<LruMapItem<K, V>> node = lruList.First;
        lruList.RemoveFirst();

        // Remove from cache
        cacheMap.Remove(node.Value.key);
    }
}

class LruMapItem<K, V> {
    public LruMapItem(K k, V v) {
        key = k;
        value = v;
    }
    public K key;
    public V value;
}