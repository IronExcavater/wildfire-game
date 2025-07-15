#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Generation.Editor
{
    [CustomEditor(typeof(GeneratorPasses))]
    public class GeneratorPassesEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var obj = target as GeneratorPasses;

            if (obj != null && GUILayout.Button("Randomise Seed"))
                obj.RandomizeSeed();

            EditorGUILayout.Space();

            if (Application.isPlaying && GUILayout.Button("Regenerate World"))
                WorldGenerator.Regenerate();
        }
    }
}
#endif
