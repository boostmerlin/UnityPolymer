using UnityEditor;
using System.IO;
using ML.UI;
using UnityEngine;
using ML;
using System.Collections.Generic;

class MLAutoInitialize
{
    static void FGUIPackageConfig()
    {
        var config = GUIConfig.Instance;
        if(!config.preloadAllLocalUI) return;
        string[] ids = AssetDatabase.FindAssets("@sprites t:textAsset");
        int cnt = ids.Length;
        List<string> packages = new List<string>();
        for (int i = 0; i < cnt; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(ids[i]);
            int pos = assetPath.LastIndexOf("@");
            if (pos == -1)
                continue;

            assetPath = assetPath.Substring(0, pos);
            //only care about Resources/*
            string uiassetsRoot = "Resources/" + config.LocalUIAssetsPath + "/";
            int slice = assetPath.LastIndexOf(uiassetsRoot, System.StringComparison.OrdinalIgnoreCase);
            if (slice == -1)
                continue;
            slice += uiassetsRoot.Length;
            if (AssetDatabase.AssetPathToGUID(assetPath) != null)
            {
                packages.Add(assetPath.Substring(slice));
            }
        }
        config.preloadAssets = packages.ToArray();
    }

    static void createConfig<T>(string name) where T : ScriptableObject
    {
        if (!File.Exists(name + ".asset"))
        {
            var configobj = ScriptableObject.CreateInstance<T>();
            EditorHelpers.CreateAsset(configobj, name);
        }
    }

    [MenuItem("Melin/Config")]
    static void initFramework()
    {
        var coms = Object.FindObjectsOfType<MSystem>();
        if(coms.Length > 1)
        {
            throw new UnityException("Multiple MLIntialize types");
        }
        else if(coms.Length == 0)
        {
            //EditorUtility.CreateGameObjectWithHideFlags("moonlight", HideFlags.HideInHierarchy, typeof(MLInitializer));
            GameObject moonlight = new GameObject("Melin");
            moonlight.AddComponent(typeof(MSystem));
        }
        else
        {
            Log.ML.Print("MLIntialize found in the scene.");
        }
    }

    [InitializeOnLoadMethod]
    static void OnProjectLoadedInEditor()
    {
        createConfig<MelinConfig>("Assets/Resources/MelinConfig");
        createConfig<GUIConfig>("Assets/Resources/GUIConfig");
        createConfig<BuilderConfig>("Assets/BuilderConfig");
        FGUIPackageConfig();
    }
}
