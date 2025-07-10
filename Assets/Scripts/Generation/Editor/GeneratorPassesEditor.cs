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

            EditorGUILayout.Space();

            if (!Application.isPlaying) return;
            if (GUILayout.Button("Regenerate World"))
                WorldGenerator.Regenerate();
        }
    }
}
#endif
