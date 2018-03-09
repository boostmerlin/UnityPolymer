using UnityEditor;

namespace ML
{
    [CustomEditor(typeof(BuilderConfig))]
    public class BuilderConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}