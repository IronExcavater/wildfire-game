using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Utilities
{
    public interface IObjectPool
    {
        Object Get();
        void Release(Object obj);
    }

    public class ObjectPool<T> : IObjectPool where T : MonoBehaviour
    {
        public Type Type => typeof(T);
        private readonly List<T> _objects = new();
        private readonly Queue<T> _pool = new();
        private readonly List<T> _prefabs;
        private readonly Transform _parent;

        public ObjectPool(int initialSize = 0, Transform parent = null, params T[] prefabs)
        {
            if (prefabs == null || prefabs.Length == 0)
                throw new ArgumentException("At least one prefab must be provided to the object pool.");

            _prefabs = new List<T>(prefabs);
            _parent = parent;

            for (var i = 0; i < initialSize; i++) Instantiate();
        }

        private T Instantiate()
        {
            var mono = Object.Instantiate(_prefabs[Random.Range(0, _prefabs.Count)], _parent);
            _objects.Add(mono);
            _pool.Enqueue(mono);
            mono.gameObject.SetActive(false);
            return mono;
        }

        public Object Get() => GetTyped();

        public T GetTyped()
        {
            if (_pool.Count <= 0) Instantiate();
            var mono = _pool.Dequeue();

            mono.gameObject.SetActive(true);
            return mono;
        }

        public void Release(Object obj) => Release((T)obj);

        public void Release(T mono)
        {
            mono.gameObject.SetActive(false);
            _pool.Enqueue(mono);
        }

        public void Clear()
        {
            foreach (var obj in _objects)
                Object.Destroy(obj);

            _objects.Clear();
            _pool.Clear();
        }

        public override string ToString()
        {
            var active = _objects.Count - _pool.Count;
            return $"ObjectPool<{typeof(T).Name}> {{Total: {_objects.Count}, Active: {active}, Inactive: {_pool.Count}}}";
        }
    }
}
