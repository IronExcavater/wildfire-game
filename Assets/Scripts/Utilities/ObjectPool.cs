﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Utilities
{
    public interface IObjectPool {}

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

            for (var i = 0; i < initialSize; i++)
            {
                var obj = Object.Instantiate(prefabs[i % _prefabs.Count], parent);
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        private T Instantiate()
        {
            var obj = Object.Instantiate(_prefabs[Random.Range(0, _prefabs.Count)], _parent);
            _objects.Add(obj);
            _pool.Enqueue(obj);
            return obj;
        }

        public T Get()
        {
            if (_pool.Count <= 0) Instantiate();
            var obj = _pool.Dequeue();

            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Release(T obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
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
