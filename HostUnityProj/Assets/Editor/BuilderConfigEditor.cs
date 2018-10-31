using UnityEditor;

namespace Ginkgo
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