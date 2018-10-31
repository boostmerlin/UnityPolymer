using UnityEngine;

namespace Ginkgo
{
    public class GinkgoConfig : ScriptableObject
    {
        public bool DebugMode = true;

        public bool PreBindCode = false;

        public string ServerUrl = "http://127.0.0.1";
        public int ServerPort = 8000;
        public string ExternalAssets = "externals";
        public string bundleExt = ".ab";

        // public long MaxCacheSize = 300 * 1024 * 1024;
        public string assemblyName = "Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";


        public string ServerRootPath
        {
            get
            {
                return string.Format("{0}:{1}/{2}", ServerUrl, ServerPort, ExternalAssets);
            }
        }
        public static GinkgoConfig Selfie
        {
            get
            {
                var ins = Resources.Load<GinkgoConfig>("GinkgoConfig");
                return ins ?? new GinkgoConfig();
            }
        }
    }
}