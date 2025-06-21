using System;
using UnityEngine;

namespace Utilities.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SerializedDictionaryFieldAttribute : PropertyAttribute
    {
        public string KeyLabel;
        public string ValueLabel;
    }
}
