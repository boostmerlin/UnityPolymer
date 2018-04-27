using UnityEngine;
using UnityEditor;
using System.IO;

namespace ML
{

    public class BuilderConfig : ScriptableObject
    {
        public static BuilderConfig Selfie
        {
            get
            {
                var ins = AssetDatabase.LoadAssetAtPath<BuilderConfig>("BuilderConfig");
                return ins ?? new BuilderConfig();
            }
        }

        void OnEnable()
        {
            buildTarget = EditorUserBuildSettings.activeBuildTarget;

            string path = EditorPrefs.GetString("bundlePathRoot");
            if (string.IsNullOrEmpty(path))
            {
                path = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            }
            bundleOutRoot = Path.Combine(path, MelinConfig.Selfie.ExternalAssets);
        }
        public BuildTarget buildTarget;
        public BuildAssetBundleOptions bundleOptions = BuildAssetBundleOptions.None;
        public string bundleOutRoot;
        public bool removeManifestAfterBuild = false;

        //public string[] bundleAssetsPaths;
    }

    public class CustomBuilder : EditorWindow
    {
        const string kMenuXBuild = "Melin/BundleBuild/X-Build %#x";
        const string kMenuQuickBundle = "Melin/BundleBuild/Q-Build %#q";
        [MenuItem(kMenuXBuild)]
        static void XBuild()
        {
            var w = GetWindow<CustomBuilder>(true, "Builder options");
            w.Show();
        }

        [MenuItem(kMenuQuickBundle)]
        static void QuickBundle()
        {
            var path = BuilderConfig.Selfie.bundleOutRoot;
            path = Path.Combine(path, getBuildTargetName());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            BuildPipeline.BuildAssetBundles(path, BuilderConfig.Selfie.bundleOptions, BuilderConfig.Selfie.buildTarget);
        }

        static string getBuildTargetName()
        {
            string lower = BuilderConfig.Selfie.buildTarget.ToString().ToLower();

            string[] names = { "android", "osx", "win", "webgl", "linux", "ios" };
            foreach (var name in names)
            {
                if (lower.Contains(name))
                {
                    return name;
                }
            }
            return lower;
        }

        private void OnGUI()
        {
        }
    }
}