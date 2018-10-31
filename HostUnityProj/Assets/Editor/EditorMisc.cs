using UnityEditor;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Ginkgo;
using Ginkgo.UI;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

class EditorMisc : EditorWindow
{
    const string kCodeGenFolder = "Assets/Scripts/Generated/";

    const string kMenuInitialConfig = "Ginkgo/Init Config";
    const string kMenuDeletePrefs = "Ginkgo/Delete Editor Preferences";
    const string kMenuGenerateCode = "Ginkgo/Generate Code";

    const string kCanvasRootName = "CanvasRoot";

    public static void AddTag(string tag)
    {
        if (!isHasTag(tag))
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty it = tagManager.GetIterator();
            while (it.NextVisible(true))
            {
                if (it.name == "tags")
                {
                    bool findAvail = false;
                    for (int i = 0; i < it.arraySize; i++)
                    {
                        SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
                        if (string.IsNullOrEmpty(dataPoint.stringValue))
                        {
                            dataPoint.stringValue = tag;
                            findAvail = true;
                            break;
                        }
                    }
                    if (!findAvail)
                    {
                        int idx = it.arraySize;
                        it.InsertArrayElementAtIndex(idx);
                        it.GetArrayElementAtIndex(idx).stringValue = tag;
                    }
                    tagManager.ApplyModifiedProperties();
                }
            }
        }
    }

    public static void AddLayer(string layer)
    {
        if (!isHasLayer(layer))
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty it = tagManager.GetIterator();
            while (it.NextVisible(true))
            {
                if (it.name == "layers")
                {
                    for (int i = 0; i <= it.arraySize; i++)
                    {
                        SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
                        if (string.IsNullOrEmpty(dataPoint.stringValue) && i > 7)
                        {
                            dataPoint.stringValue = layer;
                            tagManager.ApplyModifiedProperties();
                            return;
                        }
                    }
                }
            }
        }
    }


    static bool isHasTag(string tag)
    {
        return UnityEditorInternal.InternalEditorUtility.tags.Contains(tag);
    }

    static bool isHasLayer(string layer)
    {
        return UnityEditorInternal.InternalEditorUtility.layers.Contains(layer);
    }

    static void FGUIPackageConfig()
    {
        var config = GUIConfig.Selfie;
        if (!config.preloadAllLocalUI) return;
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

    #if !USE_FGUI
    static void initUGUI()
    {
        //init CanvasRoot;
        if(Component.FindObjectOfType<Canvas>() != null)
        {
            Debug.Log("Find Canvas in the scene, skip.");
            return;
        }
        //add RootCanvas In the scene.
        GameObject canvasObj = new GameObject(kCanvasRootName);
        canvasObj.tag = kCanvasRootName;
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        GraphicRaycaster gr = canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler  = canvasObj.AddComponent<CanvasScaler>();
        GUIConfig config = GUIConfig.Selfie;
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

        if(scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            scaler.referenceResolution = new Vector2(config.designResoWidth, config.designResoHeight);
        }

        //add EventSystem
        GameObject eventObj = new GameObject("EventSystem");
        eventObj.AddComponent<EventSystem>();
#if UNITY_STANDALONE
        eventObj.AddComponent<StandaloneInputModule>();
#endif
    }
    #endif

    [MenuItem(kMenuInitialConfig, priority = 0)]
    static void initFramework()
    {
        var coms = Object.FindObjectsOfType<MSystem>();
        if (coms.Length > 1)
        {
            throw new UnityException("Multiple MIntialize types");
        }
        else if (coms.Length == 0)
        {
            //EditorUtility.CreateGameObjectWithHideFlags("Ginkgo", HideFlags.HideInHierarchy, typeof(MLInitializer));
            GameObject go = new GameObject("Ginkgo");
            go.AddComponent(typeof(MSystem));
#if !USE_FGUI
            //AddTag("CanvasRoot");
            initUGUI();
#endif
        }
        else
        {
            Log.Common.Print("MIntialize found in the scene.");
        }
    }

    [InitializeOnLoadMethod]
    static void OnProjectLoadedInEditor()
    {
        createConfig<GinkgoConfig>("Assets/Resources/GinkgoConfig");
        createConfig<GUIConfig>("Assets/Resources/GUIConfig");
        createConfig<BuilderConfig>("Assets/BuilderConfig");
#if USE_FGUI
        FGUIPackageConfig();
#endif
    }
}
