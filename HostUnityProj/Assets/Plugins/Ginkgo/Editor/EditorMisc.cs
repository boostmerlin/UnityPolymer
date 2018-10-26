using UnityEditor;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Ginkgo;
using Ginkgo.UI;

class EditorMisc : EditorWindow
{
    const string kCodeGenFolder = "Assets/Scripts/Generated/";

    const string kMenuInitialConfig = "Ginkgo/Init Config";
    const string kMenuDeletePrefs = "Ginkgo/Delete Editor Preferences";
    const string kMenuGenerateCode = "Ginkgo/Generate Code";

    static void FGUIPackageConfig()
    {
        var config = GUIConfig.Selfie;
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

    [MenuItem(kMenuDeletePrefs)]
    static void deletePrefs()
    {
        EditorPrefs.DeleteAll();
        AssetDatabase.Refresh();
    }

    [MenuItem(kMenuInitialConfig, priority = 0)]
    static void initFramework()
    {
        var coms = Object.FindObjectsOfType<MSystem>();
        if(coms.Length > 1)
        {
            throw new UnityException("Multiple MLIntialize types");
        }
        else if(coms.Length == 0)
        {
            //EditorUtility.CreateGameObjectWithHideFlags("Ginkgo", HideFlags.HideInHierarchy, typeof(MLInitializer));
            GameObject go = new GameObject("Ginkgo");
            go.AddComponent(typeof(MSystem));
        }
        else
        {
            Log.Common.Print("MLIntialize found in the scene.");
        }
    }

    [InitializeOnLoadMethod]
    static void OnProjectLoadedInEditor()
    {
        createConfig<GinkgoConfig>("Assets/Resources/GinkgoConfig");
        createConfig<GUIConfig>("Assets/Resources/GUIConfig");
        createConfig<BuilderConfig>("Assets/BuilderConfig");
        FGUIPackageConfig();
    }
}
