using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly Queue<T> _pool = new();
        private readonly List<T> _prefabs;
        private readonly Transform _parent;

        public ObjectPool(List<T> prefabs, int initialSize, Transform parent = null)
        {
            _prefabs = prefabs;
            _parent = parent;

            for (var i = 0; i < initialSize; i++)
            {
                var obj = Object.Instantiate(prefabs[i % prefabs.Count], parent);
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        public ObjectPool(T prefab, int initialSize, Transform parent = null)
            : this(new List<T> { prefab }, initialSize, parent) {}

        public T Get()
        {
            var obj = _pool.Count > 0
                ? _pool.Dequeue()
                : Object.Instantiate(_prefabs[Random.Range(0, _prefabs.Count)], _parent);
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Release(T obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}
