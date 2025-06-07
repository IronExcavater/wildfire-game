using System;
using UnityEngine;

namespace Utilities
{
    /// <summary>
    /// Attach to Singleton subclass to enable scene persistence
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DoNotDestroySingletonAttribute : Attribute { }

    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<T>();
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (GetType().GetCustomAttributes(typeof(DoNotDestroySingletonAttribute), true).Length > 0)
                    DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}
