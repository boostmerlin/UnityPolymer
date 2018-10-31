using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ginkgo.UI
{
    [CustomEditor(typeof(GUIManager))]
    public class GUIMangerEditor : Editor
    {
        const string StartSpace = "  ";
        GUIManager inst;
        private void OnEnable()
        {
            inst = target as GUIManager;
        }
        public override void OnInspectorGUI()
        {
            GUILayout.Label("Layer Views: ");
            var layersViews = inst.GetLayerViews();
            if (layersViews == null)
            {
                return;
            }
            int i = 0;
            foreach(var onlayer in layersViews)
            {
                GUILayout.Label(StartSpace + "Layer " + i.ToString());
                int j = 0;
                foreach (var v in onlayer)
                {
                    GUILayout.Label(StartSpace + StartSpace + j.ToString() + ": " + v.Name);
                }
                i++;
            }
            GUILayout.Label("Min Unpop Layer: " + inst.MinUnpoppableLayer);
        }
    }
}
